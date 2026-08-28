import {spawnSync} from 'node:child_process';

const minimumSeverity = 'high';
const severityRanks = new Map([
  ['info', 0],
  ['low', 1],
  ['moderate', 2],
  ['high', 3],
  ['critical', 4],
]);

// image-size 2.0.2 is the latest release and Docusaurus has no patched
// dependency available yet. Keep this list limited to the upstream advisories
// that have been reviewed; dependency-chain entries are resolved to these IDs.
const allowedAdvisories = new Set([
  'GHSA-w3rx-r6r6-pgpr',
  'GHSA-5p2g-fcmc-qvqq',
]);

const npmCommand = process.platform === 'win32' ? 'npm.cmd' : 'npm';
const audit = spawnSync(npmCommand, ['audit', '--json'], {
  encoding: 'utf8',
  maxBuffer: 32 * 1024 * 1024,
});

if (audit.error) {
  console.error(`Failed to run npm audit: ${audit.error.message}`);
  process.exit(1);
}

let report;
try {
  report = JSON.parse(audit.stdout);
} catch {
  console.error('npm audit did not return valid JSON.');
  if (audit.stderr) console.error(audit.stderr.trim());
  process.exit(1);
}

if (report.error || !report.vulnerabilities || !report.metadata) {
  console.error(`npm audit failed: ${report.error?.summary ?? report.message ?? 'unknown error'}`);
  if (report.error?.detail) console.error(report.error.detail);
  process.exit(1);
}

const vulnerabilities = report.vulnerabilities;
const minimumRank = severityRanks.get(minimumSeverity);
const memoizedRoots = new Map();

function advisoryId(via) {
  const match = via.url?.match(/GHSA-[0-9a-z-]+/i);
  return match?.[0] ?? `source:${via.source ?? 'unknown'}`;
}

function resolveRootAdvisories(packageName, ancestors = new Set()) {
  if (memoizedRoots.has(packageName)) return memoizedRoots.get(packageName);
  if (ancestors.has(packageName)) return new Set([`dependency-cycle:${packageName}`]);

  const vulnerability = vulnerabilities[packageName];
  if (!vulnerability) return new Set([`unresolved:${packageName}`]);

  const nextAncestors = new Set(ancestors).add(packageName);
  const roots = new Set();

  for (const via of vulnerability.via ?? []) {
    if (typeof via === 'string') {
      for (const root of resolveRootAdvisories(via, nextAncestors)) roots.add(root);
    } else {
      roots.add(advisoryId(via));
    }
  }

  if (roots.size === 0) roots.add(`unresolved:${packageName}`);
  memoizedRoots.set(packageName, roots);
  return roots;
}

const blocked = [];
const allowedPackages = [];

for (const [packageName, vulnerability] of Object.entries(vulnerabilities)) {
  const rank = severityRanks.get(vulnerability.severity) ?? Number.POSITIVE_INFINITY;
  if (rank < minimumRank) continue;

  const roots = [...resolveRootAdvisories(packageName)];
  const unapprovedRoots = roots.filter((root) => !allowedAdvisories.has(root));

  if (unapprovedRoots.length > 0) {
    blocked.push({packageName, severity: vulnerability.severity, roots: unapprovedRoots});
  } else {
    allowedPackages.push(packageName);
  }
}

const counts = report.metadata.vulnerabilities;
console.log(
  `npm audit: ${counts.total} total ` +
    `(${counts.low} low, ${counts.moderate} moderate, ${counts.high} high, ${counts.critical} critical)`,
);

if (allowedPackages.length > 0) {
  console.warn(
    `Allowed reviewed advisories ${[...allowedAdvisories].join(', ')} ` +
      `affecting ${allowedPackages.length} dependency entries.`,
  );
}

if (blocked.length > 0) {
  console.error(`Found unapproved ${minimumSeverity} or higher vulnerabilities:`);
  for (const vulnerability of blocked) {
    console.error(
      `- ${vulnerability.packageName} (${vulnerability.severity}): ${vulnerability.roots.join(', ')}`,
    );
  }
  process.exit(1);
}

console.log(`No unapproved ${minimumSeverity} or higher vulnerabilities found.`);

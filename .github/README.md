# CI/CD configuration

## Documentation deployment

`deploy-docs.yml` builds `docs~` with Docusaurus and uploads the generated static files by FTP.

Required repository secrets:

- `FTP_SERVER`
- `FTP_USERNAME`
- `FTP_PASSWORD`

Optional repository variables:

- `FTP_REMOTE_DIR`: FTP upload destination. Defaults to `/`.
- `FTP_PROTOCOL`: `ftp`, `ftps`, or `ftps-legacy`. Defaults to `ftp`.
- `DOCS_URL`: Site origin used by Docusaurus. Defaults to `https://example.com`.
- `DOCS_BASE_URL`: Site base path. Defaults to `/`.

## Dependabot

`dependabot.yml` checks the Docusaurus npm project in `docs~` and GitHub Actions weekly.
The npm project keeps `docs~/.npmrc` in the same directory as `package.json`, so Dependabot uses Takumi Guard and writes lockfile tarball URLs through `https://npm.flatt.tech/`.

Version updates use `cooldown.default-days: 3` to match npm's `min-release-age=3` policy.

## VPM repository rebuild

`trigger-vpm-repo.yml` dispatches the VPM repository workflow when a GitHub Release changes.
This mirrors the Modular Avatar repository flow: create a draft release first, then publish it after checking the assets.

Required repository secrets:

- `VPM_REPO_TOKEN`: GitHub token that can dispatch workflows in the VPM repository.

Optional repository variables:

- `VPM_REPO_OWNER`: Target owner. Defaults to the current repository owner.
- `VPM_REPO_NAME`: Target VPM repository name. Defaults to `ustl-package-listing`.
- `VPM_REPO_WORKFLOW`: Target workflow file name. Defaults to `build-listing.yml`.
- `VPM_REPO_REF`: Target branch or ref. Defaults to `main`.

The VPM repository currently expects GitHub Release zip assets to be publicly downloadable. If this package repository remains private, mirror release zips to a public FTP path and list those URLs in the VPM repository instead of relying on `githubRepos`.

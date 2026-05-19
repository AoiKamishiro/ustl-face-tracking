using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace USTL.FaceTracking.Editor.Tests
{
    public sealed class HardwareSupportDataTests
    {
        private const string ProfileAssetsDirectoryGuid = "acb8333e55094b2caa2065288be3ac3e";

        [Test]
        public void Profiles_LoadsJsonInIdOrder()
        {
            int previousId = -1;
            foreach (HardwareSupportProfile profile in HardwareSupportData.Profiles)
            {
                Assert.That(profile.Id, Is.GreaterThan(previousId));
                Assert.That(profile.Flag, Is.EqualTo(1 << profile.Id));
                previousId = profile.Id;
            }
        }

        [Test]
        public void GetProfileDisplayName_UsesJsonDisplayName()
        {
            Assert.That(HardwareSupportData.GetProfileDisplayName(GetProfile("SamsungGalaxyXr")), Is.EqualTo("Samsung Galaxy XR"));
        }

        [Test]
        public void GetExpressionStatus_UsesExplicitJsonExpressions()
        {
            HardwareSupportProfile pimaxDroolonPi1 = GetProfile("PimaxDroolonPi1");
            HardwareSupportProfile projectBabble = GetProfile("ProjectBabble");

            Assert.That(HardwareSupportData.GetExpressionStatus(pimaxDroolonPi1, UnifiedExpression.EyeLookOutRight), Is.EqualTo(HardwareSupportStatus.Full));
            Assert.That(HardwareSupportData.GetExpressionStatus(pimaxDroolonPi1, UnifiedExpression.EyeClosedRight), Is.EqualTo(HardwareSupportStatus.Converted));
            Assert.That(HardwareSupportData.GetExpressionStatus(pimaxDroolonPi1, UnifiedExpression.BrowPinchRight), Is.EqualTo(HardwareSupportStatus.Unsupported));
            Assert.That(HardwareSupportData.GetExpressionStatus(projectBabble, UnifiedExpression.TongueTwistLeft), Is.EqualTo(HardwareSupportStatus.Full));
        }

        [Test]
        public void ProfileJsonExpressions_AllExistInUnifiedExpression()
        {
            HashSet<string> validExpressions = new(StringComparer.Ordinal);
            foreach (UnifiedExpression expression in Enum.GetValues(typeof(UnifiedExpression)))
            {
                if (expression != UnifiedExpression.None)
                {
                    validExpressions.Add(expression.ToString());
                }
            }

            List<string> errors = new();
            foreach (string path in GetProfileJsonPaths())
            {
                HardwareSupportProfileJson profile = LoadProfile(path);
                AddUnknownExpressions(errors, path, "full", profile.full, validExpressions);
                AddUnknownExpressions(errors, path, "converted", profile.converted, validExpressions);
                AddUnknownExpressions(errors, path, "unknown", profile.unknown, validExpressions);
            }

            Assert.That(errors, Is.Empty, string.Join("\n", errors));
        }

        private static List<string> GetProfileJsonPaths()
        {
            List<string> paths = new();
            string profileAssetsPath = AssetDatabase.GUIDToAssetPath(ProfileAssetsDirectoryGuid);
            Assert.That(profileAssetsPath, Is.Not.Empty, $"Profile JSON directory was not found: {ProfileAssetsDirectoryGuid}");

            foreach (string guid in AssetDatabase.FindAssets("t:TextAsset", new[] { profileAssetsPath, }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith(".json", StringComparison.Ordinal))
                {
                    paths.Add(path);
                }
            }

            paths.Sort(StringComparer.Ordinal);
            return paths;
        }

        private static HardwareSupportProfileJson LoadProfile(string path)
        {
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            Assert.That(asset, Is.Not.Null, $"Profile JSON was not found: {path}");
            return JsonUtility.FromJson<HardwareSupportProfileJson>(asset.text);
        }

        private static void AddUnknownExpressions(List<string> errors, string path, string fieldName, string[] expressions, HashSet<string> validExpressions)
        {
            foreach (string expression in expressions ?? Array.Empty<string>())
            {
                if (!validExpressions.Contains(expression))
                {
                    errors.Add($"{path}: {fieldName} contains unknown UnifiedExpression '{expression}'.");
                }
            }
        }

        private static HardwareSupportProfile GetProfile(string profileName)
        {
            foreach (HardwareSupportProfile profile in HardwareSupportData.Profiles)
            {
                if (profile.Name == profileName)
                {
                    return profile;
                }
            }

            Assert.Fail($"Profile was not found: {profileName}");
            return default;
        }

        [Serializable]
        private sealed class HardwareSupportProfileJson
        {
            public string[] full;
            public string[] converted;
            public string[] unknown;
        }
    }
}

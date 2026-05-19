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
        public void Profiles_LoadsJsonInDisplayOrder()
        {
            Assert.That(HardwareSupportData.Profiles, Is.EqualTo(new[]
            {
                FaceTrackingHardwareProfile.MetaQuestPro,
                FaceTrackingHardwareProfile.Pico4ProEnterprise,
                FaceTrackingHardwareProfile.ViveProEye,
                FaceTrackingHardwareProfile.ViveFacialTracker,
                FaceTrackingHardwareProfile.ViveFocus3EyeTrackingAddon,
                FaceTrackingHardwareProfile.ViveFocus3FacialTrackerAddon,
                FaceTrackingHardwareProfile.ViveXrEliteFullFaceTracker,
                FaceTrackingHardwareProfile.ViveFocusVisionFaceTrackingAddon,
                FaceTrackingHardwareProfile.VarjoAeroXr3Vr3,
                FaceTrackingHardwareProfile.PimaxDroolonPi1,
                FaceTrackingHardwareProfile.PimaxSuperCrystal,
                FaceTrackingHardwareProfile.Psvr2,
                FaceTrackingHardwareProfile.SamsungGalaxyXr,
                FaceTrackingHardwareProfile.HpReverbG2Omnicept,
                FaceTrackingHardwareProfile.ArkitIos,
                FaceTrackingHardwareProfile.AndroidMeowFace,
                FaceTrackingHardwareProfile.EyeTrackVR,
                FaceTrackingHardwareProfile.ProjectBabble,
            }));
        }

        [Test]
        public void GetProfileDisplayName_UsesJsonDisplayName()
        {
            Assert.That(HardwareSupportData.GetProfileDisplayName(FaceTrackingHardwareProfile.SamsungGalaxyXr), Is.EqualTo("Samsung Galaxy XR"));
        }

        [Test]
        public void GetExpressionStatus_UsesExplicitJsonExpressions()
        {
            Assert.That(HardwareSupportData.GetExpressionStatus(FaceTrackingHardwareProfile.PimaxDroolonPi1, UnifiedExpression.EyeLookOutRight), Is.EqualTo(HardwareSupportStatus.Full));
            Assert.That(HardwareSupportData.GetExpressionStatus(FaceTrackingHardwareProfile.PimaxDroolonPi1, UnifiedExpression.EyeClosedRight), Is.EqualTo(HardwareSupportStatus.Converted));
            Assert.That(HardwareSupportData.GetExpressionStatus(FaceTrackingHardwareProfile.PimaxDroolonPi1, UnifiedExpression.BrowPinchRight), Is.EqualTo(HardwareSupportStatus.Unsupported));
            Assert.That(HardwareSupportData.GetExpressionStatus(FaceTrackingHardwareProfile.ProjectBabble, UnifiedExpression.TongueTwistLeft), Is.EqualTo(HardwareSupportStatus.Full));
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

        [Serializable]
        private sealed class HardwareSupportProfileJson
        {
            public string[] full;
            public string[] converted;
            public string[] unknown;
        }
    }
}

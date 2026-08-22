using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace USTL.FaceTracking.Editor.Tests
{
    public sealed class FeatureSettingInitializationTests
    {
        [Test]
        public void AddComponent_InitializesFeatureSettingsWithoutOpeningInspector()
        {
            GameObject gameObject = new("FaceTrackingTest");
            try
            {
                USTLFaceTracking faceTracking = gameObject.AddComponent<USTLFaceTracking>();

                AssertNormalized(faceTracking.featureSettings);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Normalize_NullSettings_ReturnsNormalizedSettings()
        {
            AssertNormalized(FeatureSettingNormalizer.Normalize(null));
        }

        [Test]
        public void OnValidate_RepairsInvalidSettingsAndKeepsLastDuplicate()
        {
            GameObject gameObject = new("FaceTrackingTest");
            try
            {
                USTLFaceTracking faceTracking = gameObject.AddComponent<USTLFaceTracking>();
                VRCFTParameterSetId validOutput = FaceTrackingFeatureDefinition.All[FaceTrackingFeature.EyeLid].OutputFormats[1].Id;
                FeatureSetting kept = new()
                {
                    feature = FaceTrackingFeature.EyeLid,
                    outputFormatId = validOutput,
                    syncMode = ParameterSyncMode.Float8,
                };
                faceTracking.featureSettings = new[]
                {
                    null,
                    new FeatureSetting
                    {
                        feature = (FaceTrackingFeature)int.MaxValue,
                        outputFormatId = (VRCFTParameterSetId)int.MaxValue,
                        syncMode = (ParameterSyncMode)int.MaxValue,
                    },
                    new FeatureSetting
                    {
                        feature = FaceTrackingFeature.EyeLid,
                        outputFormatId = FaceTrackingFeatureDefinition.All[FaceTrackingFeature.EyeLid].OutputFormats[0].Id,
                        syncMode = ParameterSyncMode.LocalOnly,
                    },
                    kept,
                    new FeatureSetting
                    {
                        feature = FaceTrackingFeature.JawOpen,
                        outputFormatId = (VRCFTParameterSetId)int.MaxValue,
                        syncMode = (ParameterSyncMode)int.MaxValue,
                    },
                };

                InvokeOnValidate(faceTracking);

                AssertNormalized(faceTracking.featureSettings);
                Assert.That(faceTracking.featureSettings.Single(x => x.feature == FaceTrackingFeature.EyeLid), Is.SameAs(kept));
                FeatureSetting jawOpen = faceTracking.featureSettings.Single(x => x.feature == FaceTrackingFeature.JawOpen);
                Assert.That(jawOpen.outputFormatId, Is.EqualTo(FaceTrackingFeatureDefinition.All[FaceTrackingFeature.JawOpen].OutputFormats[0].Id));
                Assert.That(jawOpen.syncMode, Is.EqualTo(ParameterSyncMode.LocalOnly));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Normalize_PreservesValidValuesAndArrayInstance()
        {
            FeatureSetting[] settings = FeatureSettingNormalizer.Normalize(null);
            FeatureSetting eyeLid = settings.Single(x => x.feature == FaceTrackingFeature.EyeLid);
            eyeLid.outputFormatId = FaceTrackingFeatureDefinition.All[FaceTrackingFeature.EyeLid].OutputFormats[1].Id;
            eyeLid.syncMode = ParameterSyncMode.Binary3Bit;

            FeatureSetting[] normalized = FeatureSettingNormalizer.Normalize(settings);

            Assert.That(normalized, Is.SameAs(settings));
            Assert.That(eyeLid.outputFormatId, Is.EqualTo(FaceTrackingFeatureDefinition.All[FaceTrackingFeature.EyeLid].OutputFormats[1].Id));
            Assert.That(eyeLid.syncMode, Is.EqualTo(ParameterSyncMode.Binary3Bit));
        }

        private static void InvokeOnValidate(USTLFaceTracking faceTracking)
        {
            typeof(USTLFaceTracking).GetMethod("OnValidate", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(faceTracking, null);
        }

        private static void AssertNormalized(FeatureSetting[] settings)
        {
            FaceTrackingFeature[] expected = Enum.GetValues(typeof(FaceTrackingFeature))
                .Cast<FaceTrackingFeature>()
                .Where(feature => Convert.ToInt64(feature) >= 0)
                .ToArray();

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings, Has.None.Null);
            Assert.That(settings.Select(x => x.feature), Is.EqualTo(expected));
            Assert.That(settings.Select(x => x.feature), Is.Unique);
            Assert.That(settings.All(IsValid), Is.True);
        }

        private static bool IsValid(FeatureSetting setting)
        {
            FaceTrackingFeatureDefinition definition = FaceTrackingFeatureDefinition.All[setting.feature];
            bool validOutput = definition.OutputFormats.Count == 0
                ? setting.outputFormatId == VRCFTParameterSetId.None
                : definition.OutputFormats.Any(x => x.Id == setting.outputFormatId);
            return validOutput && Enum.IsDefined(typeof(ParameterSyncMode), setting.syncMode);
        }
    }
}

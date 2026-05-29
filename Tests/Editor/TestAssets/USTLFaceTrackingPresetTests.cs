using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using Object = UnityEngine.Object;

namespace USTL.FaceTracking.Editor.Tests
{
    public sealed class USTLFaceTrackingPresetTests
    {
        private const string USTL_FACE_TRACKING_PRESET_GUID = "237cbfc24ef164359af96382a4f3e984";
        private readonly List<GameObject> _gameObjects = new();

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject gameObject in _gameObjects)
            {
                if (gameObject)
                {
                    Object.DestroyImmediate(gameObject);
                }
            }

            _gameObjects.Clear();
        }

        [Test]
        public void PresetAsset_Exists()
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(USTL_FACE_TRACKING_PRESET_GUID);

            Assert.That(assetPath, Is.Not.Empty);
            Assert.That(AssetDatabase.LoadAssetAtPath<Preset>(assetPath), Is.Not.Null);
        }

        [Test]
        public void PresetAsset_CanBeAppliedToUSTLFaceTracking()
        {
            GameObject gameObject = new("USTLFaceTrackingPresetTest");
            _gameObjects.Add(gameObject);
            USTLFaceTracking target = gameObject.AddComponent<USTLFaceTracking>();
            string assetPath = AssetDatabase.GUIDToAssetPath(USTL_FACE_TRACKING_PRESET_GUID);
            Preset preset = AssetDatabase.LoadAssetAtPath<Preset>(assetPath);

            Assert.That(preset.CanBeAppliedTo(target), Is.True);
        }

        [Test]
        public void Preset_DefaultReferencesAreEmpty()
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(USTL_FACE_TRACKING_PRESET_GUID);
            Preset preset = AssetDatabase.LoadAssetAtPath<Preset>(assetPath);
            GameObject gameObject = new("USTLFaceTrackingPresetTest");
            _gameObjects.Add(gameObject);
            USTLFaceTracking target = gameObject.AddComponent<USTLFaceTracking>();
            preset.ApplyTo(target);

            Assert.That(target.faceMeshRenderer, Is.Null);
            Assert.That(target.trackingHardwareProfiles, Is.EqualTo(SupportedHardwares.None));
        }

        [Test]
        public void Preset_FeatureSettingsContainEveryFeatureOnceWithDefaultOutputFormatAndLocalOnlySync()
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(USTL_FACE_TRACKING_PRESET_GUID);
            Preset preset = AssetDatabase.LoadAssetAtPath<Preset>(assetPath);
            GameObject gameObject = new("USTLFaceTrackingPresetTest");
            _gameObjects.Add(gameObject);
            USTLFaceTracking target = gameObject.AddComponent<USTLFaceTracking>();
            preset.ApplyTo(target);

            IReadOnlyList<FaceTrackingFeature> expectedFeatures = EnumUtility.GetAllElements<FaceTrackingFeature>();

            Assert.That(target.featureSettings, Has.Length.EqualTo(expectedFeatures.Count));
            Assert.That(target.featureSettings.Select(x => x.feature), Is.Unique);
            Assert.That(target.featureSettings.Select(x => x.feature), Is.EquivalentTo(expectedFeatures));

            foreach (FeatureSetting setting in target.featureSettings)
            {
                VRCFTParameterSet expectedOutputFormat = FaceTrackingFeatureDefinition.All[setting.feature].OutputFormats[0];

                Assert.That(setting.outputFormatId, Is.EqualTo(expectedOutputFormat.Id), $"Feature: {setting.feature}");
                Assert.That(setting.syncMode, Is.EqualTo(ParameterSyncMode.LocalOnly), $"Feature: {setting.feature}");
            }
        }

        [Test]
        public void Preset_BlendShapeSettingsContainEveryUnifiedExpressionOnceWithMatchingNameAndMaxValue()
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(USTL_FACE_TRACKING_PRESET_GUID);
            Preset preset = AssetDatabase.LoadAssetAtPath<Preset>(assetPath);
            GameObject gameObject = new("USTLFaceTrackingPresetTest");
            _gameObjects.Add(gameObject);
            USTLFaceTracking target = gameObject.AddComponent<USTLFaceTracking>();
            preset.ApplyTo(target);

            IReadOnlyList<UnifiedExpression> expectedExpressions = EnumUtility.GetAllElements<UnifiedExpression>();

            Assert.That(target.blendShapeSettings, Has.Length.EqualTo(expectedExpressions.Count));
            Assert.That(target.blendShapeSettings.Select(x => x.expression), Is.Unique);
            Assert.That(target.blendShapeSettings.Select(x => x.expression), Is.EquivalentTo(expectedExpressions));

            foreach (BlendShapeSetting setting in target.blendShapeSettings)
            {
                Assert.That(setting.blendShapeName, Is.EqualTo(setting.expression.ToString()), $"Expression: {setting.expression}");
                Assert.That(setting.maxValue, Is.EqualTo(100.0f), $"Expression: {setting.expression}");
            }
        }
    }
}

using NUnit.Framework;
using UnityEngine;

namespace USTL.FaceTracking.Editor.Tests
{
    public sealed class VRCParameterUtilityTests
    {
        private GameObject _gameObject;
        private USTLFaceTracking _faceTracking;
        private Mesh _mesh;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("VRCParameterUtilityTests");
            _faceTracking = _gameObject.AddComponent<USTLFaceTracking>();
            _faceTracking.faceMeshRenderer = _gameObject.AddComponent<SkinnedMeshRenderer>();
            _mesh = new Mesh
            {
                vertices = new[] { Vector3.zero, },
            };
            Vector3[] deltas = { Vector3.one, };
            _mesh.AddBlendShapeFrame("Assigned", 100.0f, deltas, deltas, deltas);
            _faceTracking.faceMeshRenderer.sharedMesh = _mesh;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_mesh);
            Object.DestroyImmediate(_gameObject);
        }

        [TestCase(FaceTrackingFeature.EyeLid, VRCFTParameterSetId.UnifiedEyeLid, ParameterSyncMode.Float8, 8)]
        [TestCase(FaceTrackingFeature.JawOpen, VRCFTParameterSetId.SingleJawOpen, ParameterSyncMode.Binary2Bit, 2)]
        [TestCase(FaceTrackingFeature.TongueArchY, VRCFTParameterSetId.SingleTongueArchY, ParameterSyncMode.Binary2Bit, 3)]
        [TestCase(FaceTrackingFeature.EyeDirection, VRCFTParameterSetId.VRChatNative, ParameterSyncMode.None, 0)]
        public void CalculateUsage_CountsGeneratedParametersAndSignedBinaryBit(
            FaceTrackingFeature feature,
            VRCFTParameterSetId outputFormatId,
            ParameterSyncMode syncMode,
            int expectedUsage)
        {
            _faceTracking.featureSettings = new[]
            {
                CreateSetting(feature, outputFormatId, syncMode),
            };
            AssignFirstTarget(feature, outputFormatId);

            Assert.That(VRCParameterUtility.CalculateUsage(_faceTracking), Is.EqualTo(expectedUsage));
        }

        [Test]
        public void CalculateUsage_MergesDuplicateParameterToHighestSyncMode()
        {
            _faceTracking.featureSettings = new[]
            {
                CreateSetting(FaceTrackingFeature.TongueArchY, VRCFTParameterSetId.SingleTongueArchY, ParameterSyncMode.Binary2Bit),
                CreateSetting(FaceTrackingFeature.TongueArchY, VRCFTParameterSetId.SingleTongueArchY, ParameterSyncMode.Float8),
            };
            Assign(UnifiedExpression.TongueCurlUp);

            Assert.That(VRCParameterUtility.CalculateUsage(_faceTracking), Is.EqualTo(8));
        }

        [Test]
        public void CalculateUsage_UnassignedParameter_DoesNotConsumeBits()
        {
            _faceTracking.featureSettings = new[]
            {
                CreateSetting(FaceTrackingFeature.JawOpen, VRCFTParameterSetId.SingleJawOpen, ParameterSyncMode.Float8),
            };

            Assert.That(VRCParameterUtility.CalculateUsage(_faceTracking), Is.Zero);
        }

        [Test]
        public void CalculateUsage_ParameterWithOneAssignedTarget_CountsOnce()
        {
            _faceTracking.featureSettings = new[]
            {
                CreateSetting(FaceTrackingFeature.TongueArchY, VRCFTParameterSetId.SingleTongueArchY, ParameterSyncMode.Binary2Bit),
            };
            Assign(UnifiedExpression.TongueCurlUp);

            Assert.That(VRCParameterUtility.CalculateUsage(_faceTracking), Is.EqualTo(3));
        }

        [Test]
        public void CalculateUsage_NullComponent_ReturnsZero()
        {
            Assert.That(VRCParameterUtility.CalculateUsage(null), Is.Zero);
        }

        private static FeatureSetting CreateSetting(
            FaceTrackingFeature feature,
            VRCFTParameterSetId outputFormatId,
            ParameterSyncMode syncMode)
        {
            return new FeatureSetting
            {
                feature = feature,
                outputFormatId = outputFormatId,
                syncMode = syncMode,
            };
        }

        private void AssignFirstTarget(FaceTrackingFeature feature, VRCFTParameterSetId outputFormatId)
        {
            FaceTrackingFeatureDefinition featureDefinition = FaceTrackingFeatureDefinition.All[feature];
            VRCFTParameterSet set = featureDefinition.GetOutputFormatOrDefault(outputFormatId);
            if (set == null || set.Parameters.Count == 0)
            {
                return;
            }

            VRCFTParameterDefinition definition = VRCFTParameterDefinition.All[set.Parameters[0]];
            if (definition.ExpressionTargets.Count > 0)
            {
                Assign(definition.ExpressionTargets[0].Expression);
            }
        }

        private void Assign(UnifiedExpression expression)
        {
            _faceTracking.blendShapeSettings = new[]
            {
                new BlendShapeSetting
                {
                    expression = expression,
                    blendShapeName = "Assigned",
                    maxValue = 100.0f,
                },
            };
        }
    }
}

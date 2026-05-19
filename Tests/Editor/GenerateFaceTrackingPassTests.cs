using System.Collections.Generic;
using nadena.dev.modular_avatar.core;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace USTL.FaceTracking.Editor.Tests
{
    public sealed class GenerateFaceTrackingPassTests
    {
        private const string TempRoot = "Assets/__USTLFaceTrackingPassTests__";

        [SetUp]
        public void SetUp()
        {
            AssetDatabase.DeleteAsset(TempRoot);
            AssetDatabase.CreateFolder("Assets", "__USTLFaceTrackingPassTests__");
        }

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(TempRoot);
        }

        [Test]
        public void GetDefaultParameterValue_UsesNeutralValueForEyelidsOnly()
        {
            Assert.That(FaceTrackingParameterDefaults.GetDefaultValue(ParameterRangeKind.EyeLid), Is.EqualTo(0.75f).Within(0.0001f));
            Assert.That(FaceTrackingParameterDefaults.GetDefaultValue(ParameterRangeKind.Unsigned), Is.EqualTo(0f).Within(0.0001f));
            Assert.That(FaceTrackingParameterDefaults.GetDefaultValue(ParameterRangeKind.Signed), Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void EvaluateWeight_EyelidNeutralDoesNotDriveClosedOrWideBlendShapes()
        {
            float neutral = FaceTrackingParameterDefaults.GetDefaultValue(ParameterRangeKind.EyeLid);

            Assert.That(FaceTrackingWeightEvaluator.Evaluate(WeightCurveType.EyelidClosed, 0f), Is.EqualTo(1f).Within(0.0001f));
            Assert.That(FaceTrackingWeightEvaluator.Evaluate(WeightCurveType.EyelidClosed, neutral), Is.EqualTo(0f).Within(0.0001f));
            Assert.That(FaceTrackingWeightEvaluator.Evaluate(WeightCurveType.EyelidWide, neutral), Is.EqualTo(0f).Within(0.0001f));
            Assert.That(FaceTrackingWeightEvaluator.Evaluate(WeightCurveType.EyelidWide, 1f), Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void GetBinaryMagnitude_QuantizesEyelidNeutralInsteadOfDefaultingToClosed()
        {
            float neutral = FaceTrackingParameterDefaults.GetDefaultValue(ParameterRangeKind.EyeLid);

            Assert.That(BinaryParameterEncoding.GetMagnitude(neutral, 1), Is.EqualTo(1));
            Assert.That(BinaryParameterEncoding.GetMagnitude(neutral, 2), Is.EqualTo(2));
            Assert.That(BinaryParameterEncoding.GetMagnitude(neutral, 3), Is.EqualTo(5));
            Assert.That(BinaryParameterEncoding.GetMagnitude(neutral, 4), Is.EqualTo(11));
        }

        [Test]
        public void PopulateModularAvatarParameters_EyelidLocalOnlyUsesNeutralDefault()
        {
            GameObject gameObject = new("Parameters");

            try
            {
                ModularAvatarParameters parameters = gameObject.AddComponent<ModularAvatarParameters>();

                FaceTrackingModularAvatarParameterGenerator.Populate(parameters, new[]
                {
                    new SelectedParameterSetting(VRCFTParameter.EyeLidLeft, ParameterSyncMode.LocalOnly),
                });

                Assert.That(parameters.parameters, Has.Count.EqualTo(1));
                ParameterConfig config = parameters.parameters[0];
                Assert.That(config.nameOrPrefix, Is.EqualTo("USTLFT/v2/EyeLidLeft"));
                Assert.That(config.syncType, Is.EqualTo(ParameterSyncType.Float));
                Assert.That(config.localOnly, Is.True);
                Assert.That(config.defaultValue, Is.EqualTo(0.75f).Within(0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void PopulateModularAvatarParameters_EyelidBinaryUsesQuantizedNeutralDefaults()
        {
            GameObject gameObject = new("Parameters");

            try
            {
                ModularAvatarParameters parameters = gameObject.AddComponent<ModularAvatarParameters>();

                FaceTrackingModularAvatarParameterGenerator.Populate(parameters, new[]
                {
                    new SelectedParameterSetting(VRCFTParameter.EyeLid, ParameterSyncMode.Binary4Bit),
                });

                Assert.That(FindParameterConfig(parameters, "USTLFT/v2/EyeLid1").defaultValue, Is.EqualTo(1f));
                Assert.That(FindParameterConfig(parameters, "USTLFT/v2/EyeLid2").defaultValue, Is.EqualTo(1f));
                Assert.That(FindParameterConfig(parameters, "USTLFT/v2/EyeLid4").defaultValue, Is.EqualTo(0f));
                Assert.That(FindParameterConfig(parameters, "USTLFT/v2/EyeLid8").defaultValue, Is.EqualTo(1f));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void CalculateSyncParameterUsage_CountsSelectedParametersWithValidBlendShapeTargets()
        {
            GameObject gameObject = new("FaceTracking");
            GameObject rendererObject = new("Face");
            Mesh mesh = CreateFaceMesh("JawOpen", "JawRight");

            try
            {
                rendererObject.transform.SetParent(gameObject.transform, false);
                SkinnedMeshRenderer renderer = rendererObject.AddComponent<SkinnedMeshRenderer>();
                renderer.sharedMesh = mesh;

                USTLFaceTracking source = gameObject.AddComponent<USTLFaceTracking>();
                source.faceMeshRenderer = renderer;
                source.featureSettings = new[]
                {
                    new FaceTrackingFeatureSetting { feature = FaceTrackingFeature.JawOpen, syncMode = ParameterSyncMode.Float8, },
                    new FaceTrackingFeatureSetting { feature = FaceTrackingFeature.JawDirection, syncMode = ParameterSyncMode.Binary4Bit, },
                    new FaceTrackingFeatureSetting { feature = FaceTrackingFeature.JawClench, syncMode = ParameterSyncMode.Float8, },
                    new FaceTrackingFeatureSetting { feature = FaceTrackingFeature.MouthClosed, syncMode = ParameterSyncMode.None, },
                };
                source.blendShapeAssignments = new[]
                {
                    new BlendShapeAssignment { expression = UnifiedExpression.JawOpen, blendShapeName = "JawOpen", maxValue = 100f, },
                    new BlendShapeAssignment { expression = UnifiedExpression.JawRight, blendShapeName = "JawRight", maxValue = 100f, },
                    new BlendShapeAssignment { expression = UnifiedExpression.JawClench, blendShapeName = "Missing", maxValue = 100f, },
                    new BlendShapeAssignment { expression = UnifiedExpression.MouthClosed, blendShapeName = "JawOpen", maxValue = 100f, },
                };

                SyncParameterUsage usage = SyncParameterUsageCalculator.Calculate(source);

                Assert.That(usage.ConsumedBits, Is.EqualTo(13));
                Assert.That(usage.ConsumedParameterCount, Is.EqualTo(6));
                Assert.That(usage.ExpectedParameterCount, Is.EqualTo(12));
                Assert.That(usage.UnassignedParameterCount, Is.EqualTo(6));
            }
            finally
            {
                Object.DestroyImmediate(mesh);
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void CreateFloatParameterLayer_EyelidAnimatorParameterUsesNeutralDefault()
        {
            AnimatorController controller = CreateAssetController("Float.controller");
            List<Object> generatedAssets = new();

            FaceTrackingAnimatorControllerGenerator.CreateFloatParameterLayer(controller, VRCFTParameter.EyeLidLeft, VRCFTParameterDefinition.All[VRCFTParameter.EyeLidLeft], new[]
            {
                new BlendShapeTarget("Blink", 100f, WeightCurveType.EyelidClosed),
            }, string.Empty, generatedAssets);

            AnimatorControllerParameter parameter = FindAnimatorParameter(controller, "USTLFT/v2/EyeLidLeft");
            Assert.That(parameter.type, Is.EqualTo(AnimatorControllerParameterType.Float));
            Assert.That(parameter.defaultFloat, Is.EqualTo(0.75f).Within(0.0001f));
        }

        [Test]
        public void CreateBinaryParameterLayer_EyelidAnimatorDefaultsUseQuantizedNeutralState()
        {
            AnimatorController controller = CreateAssetController("Binary.controller");
            List<Object> generatedAssets = new();

            FaceTrackingAnimatorControllerGenerator.CreateBinaryParameterLayer(controller, new SelectedParameterSetting(VRCFTParameter.EyeLid, ParameterSyncMode.Binary4Bit), VRCFTParameterDefinition.All[VRCFTParameter.EyeLid], new[]
            {
                new BlendShapeTarget("Blink", 100f, WeightCurveType.EyelidClosed),
            }, string.Empty, generatedAssets);

            Assert.That(FindAnimatorParameter(controller, "USTLFT/v2/EyeLid1").defaultBool, Is.True);
            Assert.That(FindAnimatorParameter(controller, "USTLFT/v2/EyeLid2").defaultBool, Is.True);
            Assert.That(FindAnimatorParameter(controller, "USTLFT/v2/EyeLid4").defaultBool, Is.False);
            Assert.That(FindAnimatorParameter(controller, "USTLFT/v2/EyeLid8").defaultBool, Is.True);
            Assert.That(controller.layers[0].stateMachine.defaultState.name, Does.Contain("Binary 11"));
        }

        private static AnimatorController CreateAssetController(string fileName)
        {
            AnimatorController controller = AnimatorControllerNoUndoUtility.CreateAnimatorController(fileName);
            AssetDatabase.CreateAsset(controller, $"{TempRoot}/{fileName}");
            return controller;
        }

        private static Mesh CreateFaceMesh(params string[] blendShapeNames)
        {
            Mesh mesh = new()
            {
                name = "FaceTrackingEditorTestFace",
                vertices = new[] { Vector3.zero, Vector3.right, Vector3.up, },
                triangles = new[] { 0, 1, 2, },
            };
            Vector3[] deltaVertices = { Vector3.up, Vector3.up, Vector3.up, };

            foreach (string blendShapeName in blendShapeNames)
            {
                mesh.AddBlendShapeFrame(blendShapeName, 100f, deltaVertices, null, null);
            }

            return mesh;
        }

        private static ParameterConfig FindParameterConfig(ModularAvatarParameters parameters, string parameterName)
        {
            foreach (ParameterConfig config in parameters.parameters)
            {
                if (config.nameOrPrefix == parameterName)
                {
                    return config;
                }
            }

            Assert.Fail($"Parameter config '{parameterName}' was not generated.");
            return default;
        }

        private static AnimatorControllerParameter FindAnimatorParameter(AnimatorController controller, string parameterName)
        {
            foreach (AnimatorControllerParameter parameter in controller.parameters)
            {
                if (parameter.name == parameterName)
                {
                    return parameter;
                }
            }

            Assert.Fail($"Animator parameter '{parameterName}' was not generated.");
            return null;
        }
    }
}

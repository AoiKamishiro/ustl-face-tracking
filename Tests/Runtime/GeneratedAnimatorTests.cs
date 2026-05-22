using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using nadena.dev.modular_avatar.core;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.TestTools;
using USTL.FaceTracking.Editor;
using Object = UnityEngine.Object;

namespace USTL.FaceTracking.Runtime.Tests
{
    public sealed class GeneratedAnimatorTests
    {
        private const string PARAMETER_PREFIX = "USTL/v2/";
        private const float EYELID_NEUTRAL_VALUE = 0.75f;
        private const float BLENDSHAPE_TOLERANCE = 1.5f;
        private const int WAIT_FRAME_COUNT = 5;
        private const string UNIFIED_EXPRESSION_MESH_GUID = "c685687290a384d3aae25b8bf1fb69dc";
        private const string USTL_FACE_TRACKING_PRESET_GUID = "237cbfc24ef164359af96382a4f3e984";
        private static readonly int ParamIsLocal = Animator.StringToHash("IsLocal");
        private AnimatorController _animatorController;
        private GameObject _localGameObject;
        private Mesh _unifiedExpressionMesh;
        private Preset _ustlFaceTrackingPreset;

        private Mesh UnifiedExpressionMesh
        {
            get
            {
                if (!_unifiedExpressionMesh)
                {
                    _unifiedExpressionMesh = AssetDatabase.LoadAssetAtPath<Mesh>(AssetDatabase.GUIDToAssetPath(UNIFIED_EXPRESSION_MESH_GUID));
                }

                return _unifiedExpressionMesh;
            }
        }

        private Preset UstlFaceTrackingPreset
        {
            get
            {
                if (!_ustlFaceTrackingPreset)
                {
                    _ustlFaceTrackingPreset = AssetDatabase.LoadAssetAtPath<Preset>(AssetDatabase.GUIDToAssetPath(USTL_FACE_TRACKING_PRESET_GUID));
                }

                return _ustlFaceTrackingPreset;
            }
        }

        private static IEnumerable<FaceTrackingFeature> GetAllFeatures => FaceTrackingEditorUtility.AllFeatures;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _localGameObject = new GameObject("Local");
            GameObject child1 = new("Child1");
            GameObject child2 = new("Child2");
            child1.transform.SetParent(_localGameObject.transform);
            child2.transform.SetParent(_localGameObject.transform);
            SkinnedMeshRenderer smr = child1.AddComponent<SkinnedMeshRenderer>();
            smr.sharedMesh = UnifiedExpressionMesh;
            USTLFaceTracking target = child2.AddComponent<USTLFaceTracking>();
            UstlFaceTrackingPreset.ApplyTo(target);
            target.faceMeshRenderer = smr;

            if (!_animatorController)
            {
                List<ParameterConfig> parameterConfigs = new();
                _animatorController = GenerateFaceTrackingPass.GenerateAnimatorController("controllerName", target, _localGameObject.transform, ref parameterConfigs);
            }
        }

        [SetUp]
        public void Setup()
        {
            Animator animator = _localGameObject.AddComponent<Animator>();
            animator.runtimeAnimatorController = _animatorController;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_localGameObject.GetComponent<Animator>());
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Object.DestroyImmediate(_localGameObject.GetComponentInChildren<SkinnedMeshRenderer>());
            Object.DestroyImmediate(_localGameObject.GetComponentInChildren<USTLFaceTracking>());
            Object.DestroyImmediate(_localGameObject.transform.Find("Child1").gameObject);
            Object.DestroyImmediate(_localGameObject.transform.Find("Child2").gameObject);
            Object.DestroyImmediate(_localGameObject);

            List<Object> allAsset = GenerateFaceTrackingPass.CollectAllAsset(_animatorController);
            foreach (Object asset in allAsset)
            {
                Object.DestroyImmediate(asset);
            }
        }

        [UnityTest]
        public IEnumerator GeneratedAnimator_ShouldMapVRCFTParametersToBlendshapeWeights([ValueSource(nameof(GetAllFeatures))] FaceTrackingFeature feature)
        {
            Animator animator = _localGameObject.GetComponent<Animator>();
            USTLFaceTracking target = _localGameObject.GetComponentInChildren<USTLFaceTracking>();
            SkinnedMeshRenderer faceMeshRenderer = target.faceMeshRenderer;

            Assert.That(animator, Is.Not.Null);
            Assert.That(target, Is.Not.Null);
            Assert.That(faceMeshRenderer, Is.Not.Null);

            FeatureSetting featureSetting = (target.featureSettings ?? Array.Empty<FeatureSetting>()).FirstOrDefault(setting => setting != null && setting.feature == feature);
            Assert.That(featureSetting, Is.Not.Null, $"FeatureSetting for {feature} was not found.");

            Assert.That(FaceTrackingFeatureDefinition.All.TryGetValue(feature, out FaceTrackingFeatureDefinition featureDefinition), Is.True, $"FeatureDefinition for {feature} was not found.");
            VRCFTParameterSet parameterSet = featureDefinition.OutputFormats.FirstOrDefault(outputFormat => outputFormat.Id == featureSetting.outputFormatId);
            Assert.That(parameterSet, Is.Not.Null, $"{feature} uses unknown output format {featureSetting.outputFormatId}.");
            Assert.That(parameterSet.Parameters, Is.Not.Empty, $"{feature} uses {featureSetting.outputFormatId}, but it has no VRCFT parameters.");

            IReadOnlyDictionary<UnifiedExpression, BlendshapeSetting> blendshapeSettings = CollectBlendshapeSettings(target);

            animator.SetBool(ParamIsLocal, true);
            yield return null;

            foreach (VRCFTParameter parameter in parameterSet.Parameters)
            {
                Assert.That(VRCFTParameterDefinition.All.TryGetValue(parameter, out VRCFTParameterDefinition parameterDefinition), Is.True, $"VRCFT parameter definition for {parameter} was not found.");

                foreach (float parameterValue in GetTestValues(parameterDefinition.Range))
                {
                    Dictionary<UnifiedExpression, float> expectedWeights = CollectExpectedWeights(parameterDefinition.ExpressionTargets, blendshapeSettings, parameterValue);

                    ResetFloatParameters(animator);
                    animator.SetBool(ParamIsLocal, true);
                    animator.SetFloat(GetVRCFTParameterName(parameter), parameterValue);

                    for (int i = 0; i < WAIT_FRAME_COUNT; i++)
                    {
                        yield return null;
                    }

                    AssertAllExpressionWeights(faceMeshRenderer, blendshapeSettings, expectedWeights, feature, featureSetting.outputFormatId, parameter, parameterValue);
                }
            }
        }

        private static IReadOnlyDictionary<UnifiedExpression, BlendshapeSetting> CollectBlendshapeSettings(USTLFaceTracking target)
        {
            Dictionary<UnifiedExpression, BlendshapeSetting> result = new();
            foreach (BlendshapeSetting setting in target.blendshapeSettings ?? Array.Empty<BlendshapeSetting>())
            {
                if (setting == null || setting.expression == UnifiedExpression.None || string.IsNullOrWhiteSpace(setting.blendShapeName))
                {
                    continue;
                }

                result[setting.expression] = setting;
            }

            return result;
        }

        private static Dictionary<UnifiedExpression, float> CollectExpectedWeights(IReadOnlyList<ExpressionWeightTarget> expressionTargets, IReadOnlyDictionary<UnifiedExpression, BlendshapeSetting> blendshapeSettings, float parameterValue)
        {
            Dictionary<UnifiedExpression, float> expectedWeights = CreateInitialExpectedWeights(blendshapeSettings);
            foreach (ExpressionWeightTarget expressionTarget in expressionTargets)
            {
                if (expressionTarget.Expression == UnifiedExpression.None)
                {
                    continue;
                }

                Assert.That(blendshapeSettings.TryGetValue(expressionTarget.Expression, out BlendshapeSetting blendshapeSetting), Is.True, $"Blendshape setting for {expressionTarget.Expression} was not found.");

                float expectedWeight = EvaluateWeight(expressionTarget.Type, parameterValue) * Mathf.Clamp(blendshapeSetting.maxValue, 0.0f, 100.0f);
                if (expectedWeights[expressionTarget.Expression] < expectedWeight)
                {
                    expectedWeights[expressionTarget.Expression] = expectedWeight;
                }
            }

            return expectedWeights;
        }

        private static Dictionary<UnifiedExpression, float> CreateInitialExpectedWeights(IReadOnlyDictionary<UnifiedExpression, BlendshapeSetting> blendshapeSettings)
        {
            Dictionary<UnifiedExpression, float> expectedWeights = new();
            foreach (UnifiedExpression expression in FaceTrackingEditorUtility.AllExpressions)
            {
                Assert.That(blendshapeSettings.ContainsKey(expression), Is.True, $"Blendshape setting for {expression} was not found.");
                expectedWeights[expression] = 0.0f;
            }

            return expectedWeights;
        }

        private static void AssertAllExpressionWeights(
            SkinnedMeshRenderer faceMeshRenderer,
            IReadOnlyDictionary<UnifiedExpression, BlendshapeSetting> blendshapeSettings,
            IReadOnlyDictionary<UnifiedExpression, float> expectedWeights,
            FaceTrackingFeature feature,
            VRCFTParameterSetId outputFormatId,
            VRCFTParameter parameter,
            float parameterValue)
        {
            foreach (UnifiedExpression expression in FaceTrackingEditorUtility.AllExpressions)
            {
                Assert.That(expectedWeights.TryGetValue(expression, out float expectedWeight), Is.True, $"Expected weight for {expression} was not found.");
                BlendshapeSetting blendshapeSetting = blendshapeSettings[expression];
                int blendShapeIndex = faceMeshRenderer.sharedMesh.GetBlendShapeIndex(blendshapeSetting.blendShapeName);
                Assert.That(blendShapeIndex, Is.GreaterThanOrEqualTo(0), $"BlendShape '{blendshapeSetting.blendShapeName}' for {expression} was not found on the test mesh.");

                float actualWeight = faceMeshRenderer.GetBlendShapeWeight(blendShapeIndex);
                Assert.That(
                    actualWeight,
                    Is.EqualTo(expectedWeight).Within(BLENDSHAPE_TOLERANCE),
                    $"{feature} / {outputFormatId} / {parameter}={parameterValue:0.###} should set {expression} ('{blendshapeSetting.blendShapeName}') to {expectedWeight:0.###}, but was {actualWeight:0.###}.");
            }
        }

        private static IEnumerable<float> GetTestValues(ParameterRangeKind range)
        {
            switch (range)
            {
                case ParameterRangeKind.Signed:
                    yield return -1.0f;
                    yield return 1.0f;
                    break;
                case ParameterRangeKind.EyeLid:
                    yield return 0.0f;
                    yield return 1.0f;
                    break;
                default:
                    yield return 1.0f;
                    break;
            }
        }

        private static float EvaluateWeight(WeightCurveType curveType, float value)
        {
            return curveType switch
            {
                WeightCurveType.Linear => Mathf.Clamp01(value),
                WeightCurveType.PositiveSigned => Mathf.Clamp01(value),
                WeightCurveType.NegativeSigned => Mathf.Clamp01(-value),
                WeightCurveType.EyelidClosed => value < EYELID_NEUTRAL_VALUE ? Mathf.InverseLerp(EYELID_NEUTRAL_VALUE, 0.0f, value) : 0.0f,
                WeightCurveType.EyelidWide => value > EYELID_NEUTRAL_VALUE ? Mathf.InverseLerp(EYELID_NEUTRAL_VALUE, 1.0f, value) : 0.0f,
                _ => Mathf.Clamp01(value),
            };
        }

        private static void ResetFloatParameters(Animator animator)
        {
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.type == AnimatorControllerParameterType.Float)
                {
                    animator.SetFloat(parameter.nameHash, parameter.defaultFloat);
                }
            }
        }

        private static string GetVRCFTParameterName(VRCFTParameter parameter)
        {
            return $"{PARAMETER_PREFIX}{parameter}";
        }
    }
}

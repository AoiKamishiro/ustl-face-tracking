using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        private const float BLENDSHAPE_TOLERANCE = 1.5f;
        private const float EYELID_NEUTRAL_VALUE = 0.75f;
        private const int LOCAL_WAIT_FRAME_COUNT = 5;
        private const int REMOTE_WAIT_FRAME_COUNT = 32;
        private const string UNIFIED_EXPRESSION_MESH_GUID = "c685687290a384d3aae25b8bf1fb69dc";
        private const string USTL_FACE_TRACKING_PRESET_GUID = "237cbfc24ef164359af96382a4f3e984";
        private static readonly int ParamIsLocal = Animator.StringToHash("IsLocal");
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

        private static IEnumerable<(FaceTrackingFeature, VRCFTParameterSetId, ParameterSyncMode, bool)> GetAllTestCases
        {
            get
            {
                IReadOnlyList<FaceTrackingFeature> allFeatures = EnumUtility.GetAllElements<FaceTrackingFeature>();
                IReadOnlyList<ParameterSyncMode> allSyncModes = EnumUtility.GetAllElements<ParameterSyncMode>();
                List<(FaceTrackingFeature, VRCFTParameterSetId, ParameterSyncMode, bool)> list = new();
                foreach (FaceTrackingFeature feature in allFeatures)
                {
                    foreach (VRCFTParameterSet set in FaceTrackingFeatureDefinition.All[feature].OutputFormats)
                    {
                        foreach (ParameterSyncMode syncMode in allSyncModes)
                        {
                            list.Add((feature, set.Id, syncMode, true));
                            list.Add((feature, set.Id, syncMode, false));
                        }
                    }
                }

                return list;
            }
        }

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
        }

        [SetUp]
        public void Setup()
        {
            _localGameObject.AddComponent<Animator>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_localGameObject.GetComponent<Animator>());
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void QuantizeBinaryMagnitude_MatchesVRCFT545Boundaries(int bitCount)
        {
            const float boundaryOffset = 0.000001f;
            int stepCount = 1 << bitCount;
            int maxMagnitude = stepCount - 1;

            Assert.That(QuantizeBinaryMagnitude(0.0f, bitCount), Is.Zero);
            for (int magnitude = 1; magnitude < stepCount; magnitude++)
            {
                float boundary = magnitude / (float)stepCount;
                Assert.That(QuantizeBinaryMagnitude(boundary - boundaryOffset, bitCount), Is.EqualTo(magnitude - 1), $"Before {boundary}");
                Assert.That(QuantizeBinaryMagnitude(boundary, bitCount), Is.EqualTo(magnitude), $"At {boundary}");
                Assert.That(QuantizeBinaryMagnitude(boundary + boundaryOffset, bitCount), Is.EqualTo(magnitude), $"After {boundary}");
            }

            Assert.That(QuantizeBinaryMagnitude(0.99999f, bitCount), Is.EqualTo(maxMagnitude));
            Assert.That(QuantizeBinaryMagnitude(1.0f, bitCount), Is.EqualTo(maxMagnitude));
            Assert.That(QuantizeBinaryMagnitude(Mathf.Abs(-0.5f), bitCount), Is.EqualTo(Mathf.Min(stepCount / 2, maxMagnitude)));
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Object.DestroyImmediate(_localGameObject.GetComponentInChildren<SkinnedMeshRenderer>());
            Object.DestroyImmediate(_localGameObject.GetComponentInChildren<USTLFaceTracking>());
            Object.DestroyImmediate(_localGameObject.transform.Find("Child1").gameObject);
            Object.DestroyImmediate(_localGameObject.transform.Find("Child2").gameObject);
            Object.DestroyImmediate(_localGameObject);
        }

        [UnityTest]
        public IEnumerator GeneratedAnimator_ShouldMapVRCFTParametersToBlendShapeWeights([ValueSource(nameof(GetAllTestCases))] (FaceTrackingFeature feature, VRCFTParameterSetId setId, ParameterSyncMode syncMode, bool isLocal) testCase)
        {
            Animator animator = _localGameObject.GetComponent<Animator>();
            USTLFaceTracking target = _localGameObject.GetComponentInChildren<USTLFaceTracking>();
            SkinnedMeshRenderer smr = animator.GetComponentInChildren<SkinnedMeshRenderer>();
            FaceTrackingFeatureDefinition featureDefinition = FaceTrackingFeatureDefinition.All[testCase.feature];
            VRCFTParameterSet parameterSet = featureDefinition.OutputFormats.First(outputFormat => outputFormat.Id == testCase.setId);
            Dictionary<UnifiedExpression, WeightCurveType> weightCurveTypes = parameterSet.Parameters.SelectMany(x => VRCFTParameterDefinition.All[x].ExpressionTargets).ToDictionary(x => x.Expression, x => x.Type);

            target.featureSettings = new[]
            {
                new FeatureSetting
                {
                    feature = testCase.feature,
                    outputFormatId = testCase.setId,
                    syncMode = testCase.syncMode,
                },
            };

            Mesh originalMesh = smr.sharedMesh;
            List<FTBuildContext.BlendShapeBinding> bindings = new(EnumUtility.GetAllElements<UnifiedExpression>().Count);
            Dictionary<UnifiedExpression, string> generatedBlendShapeNames = new(bindings.Capacity);
            foreach (BlendShapeSetting setting in target.blendShapeSettings)
            {
                FTBuildContext.BlendShapeBinding binding = new(setting.expression, setting.blendShapeName, setting.maxValue);
                bindings.Add(binding);
                if (setting.expression != UnifiedExpression.None)
                {
                    generatedBlendShapeNames[setting.expression] = binding.GeneratedBlendShapeName;
                }
            }

            Mesh generatedMesh = MeshGenerator.Generate(originalMesh, bindings);
            smr.sharedMesh = generatedMesh;

            AnimatorController controller = AnimatorGenerator.Generate(_localGameObject.transform, target, bindings, out _);
            animator.runtimeAnimatorController = controller;

            if (testCase.isLocal)
            {
                yield return LocalUserTest(animator, smr, generatedBlendShapeNames, weightCurveTypes, parameterSet, testCase.syncMode);
            }
            else
            {
                yield return RemoteUserTest(animator, smr, generatedBlendShapeNames, weightCurveTypes, parameterSet, testCase.syncMode);
            }

            animator.runtimeAnimatorController = null;
            smr.sharedMesh = originalMesh;

            List<Object> allAsset = GenerateFaceTrackingPass.CollectAllAsset(controller);
            foreach (Object asset in allAsset)
            {
                Object.DestroyImmediate(asset);
            }

            Object.DestroyImmediate(generatedMesh);
        }

        private static IEnumerator LocalUserTest(Animator animator, SkinnedMeshRenderer smr, Dictionary<UnifiedExpression, string> generatedBlendShapeNames, Dictionary<UnifiedExpression, WeightCurveType> weightCurveTypes, VRCFTParameterSet parameterSet, ParameterSyncMode syncMode)
        {
            animator.SetBool(ParamIsLocal, true);
            AssertGeneratedBlendShapeWeightsAreZero(smr, generatedBlendShapeNames);

            if (syncMode == ParameterSyncMode.None)
            {
                yield break;
            }

            // Min-Value Test
            yield return LocalUserTestAssertion(animator, smr, generatedBlendShapeNames, weightCurveTypes, parameterSet, 0.0f, -1.0f, 0.0f);

            // Min-Mid-Value Test
            yield return LocalUserTestAssertion(animator, smr, generatedBlendShapeNames, weightCurveTypes, parameterSet, 0.25f, -0.5f, 0.375f);

            // Mid-Value Test
            yield return LocalUserTestAssertion(animator, smr, generatedBlendShapeNames, weightCurveTypes, parameterSet, 0.5f, 0.0f, 0.75f);

            // Max-Mid-Value Test
            yield return LocalUserTestAssertion(animator, smr, generatedBlendShapeNames, weightCurveTypes, parameterSet, 0.75f, 0.7f, 0.875f);

            // Max-Value Test
            yield return LocalUserTestAssertion(animator, smr, generatedBlendShapeNames, weightCurveTypes, parameterSet, 1.0f, 1.0f, 1.0f);
        }

        private static IEnumerator LocalUserTestAssertion(Animator animator, SkinnedMeshRenderer smr, Dictionary<UnifiedExpression, string> generatedBlendShapeNames, Dictionary<UnifiedExpression, WeightCurveType> weightCurveTypes, VRCFTParameterSet parameterSet, float unsigned, float signed, float eyelid)
        {
            foreach (VRCFTParameter param in parameterSet.Parameters)
            {
                string paramName = AnimatorGenerator.GetVRCFTParameterName(param);
                switch (VRCFTParameterDefinition.All[param].Range)
                {
                    case ParameterRangeKind.Unsigned:
                        animator.SetFloat(paramName, unsigned);
                        break;
                    case ParameterRangeKind.Signed:
                        animator.SetFloat(paramName, signed);
                        break;
                    case ParameterRangeKind.EyeLid:
                        animator.SetFloat(paramName, eyelid);
                        break;
                }
            }

            for (int i = 0; i < LOCAL_WAIT_FRAME_COUNT; i++)
            {
                yield return null;
            }

            foreach (KeyValuePair<UnifiedExpression, string> generatedBlendShape in generatedBlendShapeNames)
            {
                int index = smr.sharedMesh.GetBlendShapeIndex(generatedBlendShape.Value);
                float weight = smr.GetBlendShapeWeight(index);
                float expected = 0.0f;
                if (weightCurveTypes.TryGetValue(generatedBlendShape.Key, out WeightCurveType curveType))
                {
                    expected = CalculateExpectedWeight(curveType, unsigned, signed, eyelid);
                }

                Assert.That(weight, Is.EqualTo(expected).Within(BLENDSHAPE_TOLERANCE));
            }
        }


        private static IEnumerator RemoteUserTest(Animator animator, SkinnedMeshRenderer smr, Dictionary<UnifiedExpression, string> generatedBlendShapeNames, Dictionary<UnifiedExpression, WeightCurveType> weightCurveTypes, VRCFTParameterSet parameterSet, ParameterSyncMode syncMode)
        {
            animator.SetBool(ParamIsLocal, false);
            AssertGeneratedBlendShapeWeightsAreZero(smr, generatedBlendShapeNames);

            if (syncMode == ParameterSyncMode.None || syncMode == ParameterSyncMode.LocalOnly)
            {
                yield break;
            }

            // Min-Value Test
            yield return RemoteUserTestAssertion(animator, smr, generatedBlendShapeNames, weightCurveTypes, parameterSet, syncMode, 0.0f, -1.0f, 0.0f);

            // Min-Mid-Value Test
            yield return RemoteUserTestAssertion(animator, smr, generatedBlendShapeNames, weightCurveTypes, parameterSet, syncMode, 0.25f, -0.5f, 0.375f);

            // Mid-Value Test
            yield return RemoteUserTestAssertion(animator, smr, generatedBlendShapeNames, weightCurveTypes, parameterSet, syncMode, 0.5f, 0.0f, 0.75f);

            // Max-Mid-Value Test
            yield return RemoteUserTestAssertion(animator, smr, generatedBlendShapeNames, weightCurveTypes, parameterSet, syncMode, 0.75f, 0.7f, 0.875f);

            // Max-Value Test
            yield return RemoteUserTestAssertion(animator, smr, generatedBlendShapeNames, weightCurveTypes, parameterSet, syncMode, 1.0f, 1.0f, 1.0f);
        }

        private static IEnumerator RemoteUserTestAssertion(Animator animator, SkinnedMeshRenderer smr, Dictionary<UnifiedExpression, string> generatedBlendShapeNames, Dictionary<UnifiedExpression, WeightCurveType> weightCurveTypes, VRCFTParameterSet parameterSet, ParameterSyncMode syncMode, float unsignedOrigin, float signedOrigin, float eyelidOrigin)
        {
            float unsigned = GetExpectedValue(unsignedOrigin, syncMode, ParameterRangeKind.Unsigned);
            float signed = GetExpectedValue(signedOrigin, syncMode, ParameterRangeKind.Signed);
            float eyelid = GetExpectedValue(eyelidOrigin, syncMode, ParameterRangeKind.EyeLid);

            foreach (VRCFTParameter param in parameterSet.Parameters)
            {
                string paramName = AnimatorGenerator.GetVRCFTParameterName(param);
                ParameterRangeKind range = VRCFTParameterDefinition.All[param].Range;
                switch (range)
                {
                    case ParameterRangeKind.Unsigned:
                        SetRemoteParameterInput(animator, param, paramName, syncMode, range, unsignedOrigin);
                        break;
                    case ParameterRangeKind.Signed:
                        SetRemoteParameterInput(animator, param, paramName, syncMode, range, signedOrigin);
                        break;
                    case ParameterRangeKind.EyeLid:
                        SetRemoteParameterInput(animator, param, paramName, syncMode, range, eyelidOrigin);
                        break;
                }
            }

            for (int i = 0; i < REMOTE_WAIT_FRAME_COUNT; i++)
            {
                yield return null;
            }

            foreach (KeyValuePair<UnifiedExpression, string> generatedBlendShape in generatedBlendShapeNames)
            {
                int index = smr.sharedMesh.GetBlendShapeIndex(generatedBlendShape.Value);
                float weight = smr.GetBlendShapeWeight(index);
                float expected = 0.0f;
                if (weightCurveTypes.TryGetValue(generatedBlendShape.Key, out WeightCurveType curveType))
                {
                    expected = CalculateExpectedWeight(curveType, unsigned, signed, eyelid);
                }

                Assert.That(weight, Is.EqualTo(expected).Within(BLENDSHAPE_TOLERANCE));
            }
        }

        private static void AssertGeneratedBlendShapeWeightsAreZero(SkinnedMeshRenderer smr, IReadOnlyDictionary<UnifiedExpression, string> generatedBlendShapeNames)
        {
            foreach (string blendShapeName in generatedBlendShapeNames.Values)
            {
                int index = smr.sharedMesh.GetBlendShapeIndex(blendShapeName);
                Assert.That(index, Is.GreaterThanOrEqualTo(0), $"Generated BlendShape '{blendShapeName}' was not found.");
                Assert.That(smr.GetBlendShapeWeight(index), Is.EqualTo(0.0f).Within(BLENDSHAPE_TOLERANCE));
            }
        }

        private static void SetRemoteParameterInput(Animator animator, VRCFTParameter param, string paramName, ParameterSyncMode syncMode, ParameterRangeKind range, float value)
        {
            int bitCount = AnimatorGenerator.GetBinaryBitCount(syncMode);
            if (bitCount <= 0)
            {
                animator.SetFloat(paramName, value);
                return;
            }

            bool signed = range == ParameterRangeKind.Signed;
            bool negative = signed && value < 0.0f;
            int magnitude = QuantizeBinaryMagnitude(signed ? Mathf.Abs(value) : value, bitCount);
            for (int bitIndex = 0; bitIndex < bitCount; bitIndex++)
            {
                int bitValue = 1 << bitIndex;
                animator.SetBool(AnimatorGenerator.GetBinaryParameterName(param, bitValue), (magnitude & bitValue) != 0);
            }

            if (signed)
            {
                animator.SetBool(AnimatorGenerator.GetBinaryNegativeParameterName(param), negative);
            }

            float unusedValue = range switch
            {
                ParameterRangeKind.Signed => value < 0.0f ? 1.0f : -1.0f,
                ParameterRangeKind.EyeLid => Mathf.Abs(value - EYELID_NEUTRAL_VALUE) < 0.001f ? 0.0f : EYELID_NEUTRAL_VALUE,
                _ => value < 0.5f ? 1.0f : 0.0f,
            };
            animator.SetFloat(AnimatorGenerator.GetVRCFTParameterName(param), unusedValue);
        }

        private static float GetExpectedValue(float source, ParameterSyncMode syncMode, ParameterRangeKind range)
        {
            return syncMode switch
            {
                ParameterSyncMode.Binary1Bit => QuantizedValue(1, source, range == ParameterRangeKind.Signed),
                ParameterSyncMode.Binary2Bit => QuantizedValue(2, source, range == ParameterRangeKind.Signed),
                ParameterSyncMode.Binary3Bit => QuantizedValue(3, source, range == ParameterRangeKind.Signed),
                ParameterSyncMode.Binary4Bit => QuantizedValue(4, source, range == ParameterRangeKind.Signed),
                ParameterSyncMode.Float8 => source,
                _ => -1,
            };

            float QuantizedValue(int bitCount, float value, bool signed)
            {
                bool negative = signed && value < 0.0f;
                int magnitude = QuantizeBinaryMagnitude(signed ? Mathf.Abs(value) : value, bitCount);

                int maxMagnitude = (1 << bitCount) - 1;
                float newValue = maxMagnitude <= 0 ? 0.0f : magnitude / (float)maxMagnitude;
                return negative ? -newValue : newValue;
            }
        }

        private static int QuantizeBinaryMagnitude(float value, int bitCount)
        {
            // Mirrors VRCFT 5.4.5 BinaryBaseParameter.ProcessBinary at commit 2cafd0e.
            int maxMagnitude = (1 << bitCount) - 1;
            if (value > 0.99999f)
            {
                return maxMagnitude;
            }

            return Mathf.Clamp(Mathf.FloorToInt(value * (1 << bitCount)), 0, maxMagnitude);
        }

        private static float CalculateExpectedWeight(WeightCurveType type, float unsigned, float signed, float eyelid)
        {
            return type switch
            {
                WeightCurveType.Linear => Mathf.Clamp01(unsigned) * 100.0f,
                WeightCurveType.PositiveSigned => Mathf.Clamp01(signed) * 100.0f,
                WeightCurveType.NegativeSigned => Mathf.Clamp01(-signed) * 100.0f,
                WeightCurveType.EyelidClosed => eyelid < EYELID_NEUTRAL_VALUE ? Mathf.InverseLerp(EYELID_NEUTRAL_VALUE, 0.0f, eyelid) * 100.0f : 0.0f,
                WeightCurveType.EyelidWide => eyelid > EYELID_NEUTRAL_VALUE ? Mathf.InverseLerp(EYELID_NEUTRAL_VALUE, 1.0f, eyelid) * 100.0f : 0.0f,
                _ => -1.0f,
            };
        }
    }
}

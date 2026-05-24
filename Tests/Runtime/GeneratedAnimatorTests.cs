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
        private const int WAIT_FRAME_COUNT = 5;
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
                IReadOnlyList<FaceTrackingFeature> allFeatures = FaceTrackingEditorUtility.AllFeatures;
                List<(FaceTrackingFeature, VRCFTParameterSetId, ParameterSyncMode, bool)> list = new();
                foreach (FaceTrackingFeature feature in allFeatures)
                {
                    foreach (VRCFTParameterSet set in FaceTrackingFeatureDefinition.All[feature].OutputFormats)
                    {
                        foreach (ParameterSyncMode syncMode in FaceTrackingEditorUtility.AllSyncModes)
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
        public IEnumerator GeneratedAnimator_ShouldMapVRCFTParametersToBlendshapeWeights([ValueSource(nameof(GetAllTestCases))] (FaceTrackingFeature feature, VRCFTParameterSetId setId, ParameterSyncMode syncMode, bool isLocal) testCase)
        {
            Animator animator = _localGameObject.GetComponent<Animator>();
            USTLFaceTracking target = _localGameObject.GetComponentInChildren<USTLFaceTracking>();
            SkinnedMeshRenderer smr = animator.GetComponentInChildren<SkinnedMeshRenderer>();
            FaceTrackingFeatureDefinition featureDefinition = FaceTrackingFeatureDefinition.All[testCase.feature];
            VRCFTParameterSet parameterSet = featureDefinition.OutputFormats.First(outputFormat => outputFormat.Id == testCase.setId);
            Dictionary<UnifiedExpression, WeightCurveType> ps = parameterSet.Parameters.SelectMany(x => VRCFTParameterDefinition.All[x].ExpressionTargets).ToDictionary(x => x.Expression, x => x.Type);

            target.featureSettings = new[]
            {
                new FeatureSetting
                {
                    feature = testCase.feature,
                    outputFormatId = testCase.setId,
                    syncMode = testCase.syncMode,
                },
            };
            AnimatorController controller = GenerateFaceTrackingProcess.GenerateAnimatorController("testController", target, _localGameObject.transform, out _);
            animator.runtimeAnimatorController = controller;

            if (testCase.isLocal)
            {
                yield return LocalUserTest(animator, smr, ps, parameterSet, testCase.syncMode);
            }
            else
            {
                yield return RemoteUserTest(animator, smr, ps, parameterSet, testCase.syncMode);
            }

            // Clear Generated
            animator.runtimeAnimatorController = null;
            List<Object> allAsset = GenerateFaceTrackingPass.CollectAllAsset(controller);
            foreach (Object asset in allAsset)
            {
                Object.DestroyImmediate(asset);
            }
        }

        private static IEnumerator LocalUserTest(Animator animator, SkinnedMeshRenderer smr, Dictionary<UnifiedExpression, WeightCurveType> ps, VRCFTParameterSet parameterSet, ParameterSyncMode syncMode)
        {
            animator.SetBool(ParamIsLocal, true);

            foreach (UnifiedExpression expression in FaceTrackingEditorUtility.AllExpressions)
            {
                int index = smr.sharedMesh.GetBlendShapeIndex(expression.ToString());
                float weight = smr.GetBlendShapeWeight(index);
                Assert.That(weight, Is.EqualTo(0.0f).Within(BLENDSHAPE_TOLERANCE));
            }

            if (syncMode == ParameterSyncMode.None)
            {
                yield break;
            }

            // Min-Value Test
            yield return LocalUserTestAssertion(animator, smr, ps, parameterSet, 0.0f, -1.0f, 0.0f);

            // Min-Mid-Value Test
            yield return LocalUserTestAssertion(animator, smr, ps, parameterSet, 0.25f, -0.5f, 0.375f);

            // Mid-Value Test
            yield return LocalUserTestAssertion(animator, smr, ps, parameterSet, 0.5f, 0.0f, 0.75f);

            // Max-Mid-Value Test
            yield return LocalUserTestAssertion(animator, smr, ps, parameterSet, 0.75f, 0.7f, 0.875f);

            // Max-Value Test
            yield return LocalUserTestAssertion(animator, smr, ps, parameterSet, 1.0f, 1.0f, 1.0f);
        }

        private static IEnumerator LocalUserTestAssertion(Animator animator, SkinnedMeshRenderer smr, Dictionary<UnifiedExpression, WeightCurveType> ps, VRCFTParameterSet parameterSet, float unsigned, float signed, float eyelid)
        {
            foreach (VRCFTParameter param in parameterSet.Parameters)
            {
                string paramName = GenerateFaceTrackingProcess.GetVRCFTParameterName(param);
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

            for (int i = 0; i < WAIT_FRAME_COUNT; i++)
            {
                yield return null;
            }

            foreach (UnifiedExpression expression in FaceTrackingEditorUtility.AllExpressions)
            {
                int index = smr.sharedMesh.GetBlendShapeIndex(expression.ToString());
                float weight = smr.GetBlendShapeWeight(index);
                float expected = 0.0f;
                if (ps.TryGetValue(expression, out WeightCurveType p))
                {
                    expected = CalculateExpectedWeight(p, unsigned, signed, eyelid);
                }

                Assert.That(weight, Is.EqualTo(expected).Within(BLENDSHAPE_TOLERANCE));
            }
        }


        private static IEnumerator RemoteUserTest(Animator animator, SkinnedMeshRenderer smr, Dictionary<UnifiedExpression, WeightCurveType> ps, VRCFTParameterSet parameterSet, ParameterSyncMode syncMode)
        {
            animator.SetBool(ParamIsLocal, false);

            foreach (UnifiedExpression expression in FaceTrackingEditorUtility.AllExpressions)
            {
                int index = smr.sharedMesh.GetBlendShapeIndex(expression.ToString());
                float weight = smr.GetBlendShapeWeight(index);
                Assert.That(weight, Is.EqualTo(0.0f).Within(BLENDSHAPE_TOLERANCE));
            }

            if (syncMode == ParameterSyncMode.None || syncMode == ParameterSyncMode.LocalOnly)
            {
                yield break;
            }

            // Min-Value Test
            yield return RemoteUserTestAssertion(animator, smr, ps, parameterSet, syncMode, 0.0f, -1.0f, 0.0f);

            // Min-Mid-Value Test
            yield return RemoteUserTestAssertion(animator, smr, ps, parameterSet, syncMode, 0.25f, -0.5f, 0.375f);

            // Mid-Value Test
            yield return RemoteUserTestAssertion(animator, smr, ps, parameterSet, syncMode, 0.5f, 0.0f, 0.75f);

            // Max-Mid-Value Test
            yield return RemoteUserTestAssertion(animator, smr, ps, parameterSet, syncMode, 0.75f, 0.7f, 0.875f);

            // Max-Value Test
            yield return RemoteUserTestAssertion(animator, smr, ps, parameterSet, syncMode, 1.0f, 1.0f, 1.0f);
        }

        private static IEnumerator RemoteUserTestAssertion(Animator animator, SkinnedMeshRenderer smr, Dictionary<UnifiedExpression, WeightCurveType> ps, VRCFTParameterSet parameterSet, ParameterSyncMode syncMode, float unsignedOrigin, float signedOrigin, float eyelidOrigin)
        {
            float unsigned = GetExpectedValue(unsignedOrigin, syncMode, ParameterRangeKind.Unsigned);
            float signed = GetExpectedValue(signedOrigin, syncMode, ParameterRangeKind.Signed);
            float eyelid = GetExpectedValue(eyelidOrigin, syncMode, ParameterRangeKind.EyeLid);

            foreach (VRCFTParameter param in parameterSet.Parameters)
            {
                string paramName = GenerateFaceTrackingProcess.GetVRCFTParameterName(param);
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

            for (int i = 0; i < WAIT_FRAME_COUNT; i++)
            {
                yield return null;
            }

            foreach (UnifiedExpression expression in FaceTrackingEditorUtility.AllExpressions)
            {
                int index = smr.sharedMesh.GetBlendShapeIndex(expression.ToString());
                float weight = smr.GetBlendShapeWeight(index);
                float expected = 0.0f;
                if (ps.TryGetValue(expression, out WeightCurveType p))
                {
                    expected = CalculateExpectedWeight(p, unsigned, signed, eyelid);
                }

                Assert.That(weight, Is.EqualTo(expected).Within(BLENDSHAPE_TOLERANCE));
            }
        }

        private static void SetRemoteParameterInput(Animator animator, VRCFTParameter param, string paramName, ParameterSyncMode syncMode, ParameterRangeKind range, float value)
        {
            int bitCount = GenerateFaceTrackingProcess.GetBinaryBitCount(syncMode);
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
                animator.SetBool(GenerateFaceTrackingProcess.GetBinaryParameterName(param, bitValue), (magnitude & bitValue) != 0);
            }

            if (signed)
            {
                animator.SetBool(GenerateFaceTrackingProcess.GetBinaryNegativeParameterName(param), negative);
            }

            float unusedValue = range switch
            {
                ParameterRangeKind.Signed => value < 0.0f ? 1.0f : -1.0f,
                ParameterRangeKind.EyeLid => Mathf.Abs(value - EYELID_NEUTRAL_VALUE) < 0.001f ? 0.0f : EYELID_NEUTRAL_VALUE,
                _ => value < 0.5f ? 1.0f : 0.0f,
            };
            animator.SetFloat(GenerateFaceTrackingProcess.GetVRCFTParameterName(param), unusedValue);
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
            int maxMagnitude = (1 << bitCount) - 1;
            return Mathf.Clamp(Mathf.FloorToInt(Mathf.Clamp01(value) * maxMagnitude + 0.5f), 0, maxMagnitude);
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

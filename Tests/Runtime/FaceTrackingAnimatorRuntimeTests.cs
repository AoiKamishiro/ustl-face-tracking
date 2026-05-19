#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.TestTools;
using USTL.FaceTracking.Editor;
using Object = UnityEngine.Object;

namespace USTL.FaceTracking.Runtime.Tests
{
    public sealed class FaceTrackingAnimatorRuntimeTests
    {
        private const float WeightTolerance = 0.5f;
        private const string TempRoot = "Assets/__USTLFaceTrackingRuntimeTests__";
        private const string JawOpenBlendShape = "JawOpen";
        private const string JawRightBlendShape = "JawRight";
        private const string JawLeftBlendShape = "JawLeft";
        private const string RendererPath = "Face";
        private Animator _animator;

        private GameObject _avatarRoot;
        private Mesh _faceMesh;
        private SkinnedMeshRenderer _faceMeshRenderer;

        [SetUp]
        public void SetUp()
        {
            AssetDatabase.DeleteAsset(TempRoot);
            AssetDatabase.CreateFolder("Assets", "__USTLFaceTrackingRuntimeTests__");

            _avatarRoot = new GameObject("AvatarRoot");
            _animator = _avatarRoot.AddComponent<Animator>();
            _animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            GameObject face = new(RendererPath);
            face.transform.SetParent(_avatarRoot.transform, false);
            _faceMeshRenderer = face.AddComponent<SkinnedMeshRenderer>();
            _faceMesh = CreateFaceMesh(JawOpenBlendShape, JawRightBlendShape, JawLeftBlendShape);
            _faceMeshRenderer.sharedMesh = _faceMesh;
        }

        [TearDown]
        public void TearDown()
        {
            if (_avatarRoot)
            {
                Object.DestroyImmediate(_avatarRoot);
            }

            if (_faceMesh)
            {
                Object.DestroyImmediate(_faceMesh);
            }

            AssetDatabase.DeleteAsset(TempRoot);
        }

        [UnityTest]
        public IEnumerator LocalFloatParameter_DrivesBlendShapeWeight()
        {
            AnimatorController controller = CreateAssetController("LocalFloat.controller");
            FaceTrackingAnimatorControllerGenerator.CreateFloatParameterLayer(controller, VRCFTParameter.JawOpen, VRCFTParameterDefinition.All[VRCFTParameter.JawOpen], new[]
            {
                new BlendShapeTarget(JawOpenBlendShape, 100f, WeightCurveType.Linear),
            }, RendererPath, new List<Object>());

            StartAnimator(controller);
            yield return null;

            AssertBlendShapeWeight(JawOpenBlendShape, 0f);

            _animator.SetFloat(FaceTrackingGeneratedParameterNames.FormatFloat(VRCFTParameter.JawOpen), 0.25f);
            EvaluateAnimator();
            AssertBlendShapeWeight(JawOpenBlendShape, 25f);

            _animator.SetFloat(FaceTrackingGeneratedParameterNames.FormatFloat(VRCFTParameter.JawOpen), 0.8f);
            EvaluateAnimator();
            AssertBlendShapeWeight(JawOpenBlendShape, 80f);
        }

        [UnityTest]
        public IEnumerator RemoteBinaryParameters_ReconstructQuantizedSignedBlendShapeWeights()
        {
            const int bitCount = 4;
            AnimatorController controller = CreateAssetController("RemoteBinary.controller");
            FaceTrackingAnimatorControllerGenerator.CreateBinaryParameterLayer(controller, new SelectedParameterSetting(VRCFTParameter.JawX, ParameterSyncMode.Binary4Bit), VRCFTParameterDefinition.All[VRCFTParameter.JawX], new[]
            {
                new BlendShapeTarget(JawRightBlendShape, 100f, WeightCurveType.PositiveSigned),
                new BlendShapeTarget(JawLeftBlendShape, 100f, WeightCurveType.NegativeSigned),
            }, RendererPath, new List<Object>());

            StartAnimator(controller);
            yield return null;

            SetQuantizedBinaryParameters(VRCFTParameter.JawX, 0.47f, bitCount);
            EvaluateAnimatorTransition();
            float positiveExpected = GetQuantizedMagnitude(0.47f, bitCount) * 100f;
            AssertBlendShapeWeight(JawRightBlendShape, positiveExpected);
            AssertBlendShapeWeight(JawLeftBlendShape, 0f);

            SetQuantizedBinaryParameters(VRCFTParameter.JawX, -0.47f, bitCount);
            EvaluateAnimatorTransition();
            float negativeExpected = GetQuantizedMagnitude(-0.47f, bitCount) * 100f;
            AssertBlendShapeWeight(JawRightBlendShape, 0f);
            AssertBlendShapeWeight(JawLeftBlendShape, negativeExpected);
        }

        private static Mesh CreateFaceMesh(params string[] blendShapeNames)
        {
            Mesh mesh = new()
            {
                name = "FaceTrackingRuntimeTestFace",
                vertices = new[]
                {
                    new Vector3(-0.5f, -0.5f, 0f),
                    new Vector3(0.5f, -0.5f, 0f),
                    new Vector3(0f, 0.5f, 0f),
                },
                triangles = new[] { 0, 1, 2, },
            };

            Vector3[] deltaVertices =
            {
                new(0f, 0.01f, 0f),
                new(0f, 0.01f, 0f),
                new(0f, 0.01f, 0f),
            };

            foreach (string blendShapeName in blendShapeNames)
            {
                mesh.AddBlendShapeFrame(blendShapeName, 100f, deltaVertices, null, null);
            }

            mesh.RecalculateBounds();
            return mesh;
        }

        private static AnimatorController CreateAssetController(string fileName)
        {
            AnimatorController controller = AnimatorControllerNoUndoUtility.CreateAnimatorController(fileName);
            AssetDatabase.CreateAsset(controller, $"{TempRoot}/{fileName}");
            return controller;
        }

        private void StartAnimator(RuntimeAnimatorController controller)
        {
            _animator.runtimeAnimatorController = controller;
            _animator.Rebind();
            _animator.Update(0f);
        }

        private void EvaluateAnimator()
        {
            _animator.Update(1f / 60f);
        }

        private void EvaluateAnimatorTransition()
        {
            for (int i = 0; i < 30; i++)
            {
                _animator.Update(1f / 60f);
            }
        }

        private void SetQuantizedBinaryParameters(VRCFTParameter parameter, float value, int bitCount)
        {
            int magnitude = BinaryParameterEncoding.GetMagnitude(value, bitCount);
            for (int i = 0; i < bitCount; i++)
            {
                _animator.SetBool(FaceTrackingGeneratedParameterNames.FormatBinaryBit(parameter, 1 << i), (magnitude & (1 << i)) != 0);
            }

            _animator.SetBool(FaceTrackingGeneratedParameterNames.FormatBinaryNegative(parameter), magnitude > 0 && value < 0f);
        }

        private static float GetQuantizedMagnitude(float value, int bitCount)
        {
            int maxMagnitude = (1 << bitCount) - 1;
            return BinaryParameterEncoding.GetMagnitude(value, bitCount) / (float)maxMagnitude;
        }

        private void AssertBlendShapeWeight(string blendShapeName, float expected)
        {
            int index = _faceMesh.GetBlendShapeIndex(blendShapeName);
            Assert.That(index, Is.GreaterThanOrEqualTo(0), $"Missing blend shape '{blendShapeName}'.");
            Assert.That(_faceMeshRenderer.GetBlendShapeWeight(index), Is.EqualTo(expected).Within(WeightTolerance), blendShapeName);
        }
    }
}
#endif

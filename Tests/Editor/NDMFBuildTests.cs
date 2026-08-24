using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using nadena.dev.ndmf;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace USTL.FaceTracking.Editor.Tests
{
    public sealed class NDMFBuildTests
    {
        private GameObject _buildAvatar;
        private Mesh _originalMesh;
        private GameObject _sourceAvatar;

        [TearDown]
        public void TearDown()
        {
            if (_buildAvatar)
            {
                Object.DestroyImmediate(_buildAvatar);
            }

            if (_sourceAvatar)
            {
                Object.DestroyImmediate(_sourceAvatar);
            }

            if (_originalMesh)
            {
                Object.DestroyImmediate(_originalMesh, true);
            }
        }

        [Test]
        public void ProcessAvatar_SavesAndAssignsGeneratedMesh_WithoutModifyingSourceAvatar()
        {
            _sourceAvatar = CreateAvatar(out SkinnedMeshRenderer sourceRenderer);
            _buildAvatar = Object.Instantiate(_sourceAvatar);
            _buildAvatar.name = "Build Avatar";
            SkinnedMeshRenderer buildRenderer = _buildAvatar.GetComponentInChildren<SkinnedMeshRenderer>();

            AvatarProcessor.ProcessAvatar(_buildAvatar);

            Mesh generatedMesh = buildRenderer.sharedMesh;
            Assert.That(generatedMesh, Is.Not.Null.And.Not.SameAs(_originalMesh));
            Assert.That(generatedMesh.name, Does.Contain("USTL FT Generated BlendShapes"));
            Assert.That(generatedMesh.GetBlendShapeIndex("USTL_JawOpen"), Is.GreaterThanOrEqualTo(0));
            Assert.That(AssetDatabase.Contains(generatedMesh), Is.True, "Generated mesh was not saved as an NDMF asset.");

            Assert.That(sourceRenderer.sharedMesh, Is.SameAs(_originalMesh));
            Assert.That(_originalMesh.GetBlendShapeIndex("USTL_JawOpen"), Is.EqualTo(-1));
            Assert.That(_sourceAvatar.GetComponentInChildren<USTLFaceTracking>(), Is.Not.Null);
            Assert.That(_buildAvatar.GetComponentInChildren<USTLFaceTracking>(), Is.Null);
        }

        [Test]
        public void ProcessAvatar_SavesGeneratedAnimatorSubAssetsAndPreservesReferencesAfterImport()
        {
            _sourceAvatar = CreateAvatar(out _);
            USTLFaceTracking faceTracking = _sourceAvatar.GetComponentInChildren<USTLFaceTracking>();
            faceTracking.featureSettings = new[]
            {
                new FeatureSetting
                {
                    feature = FaceTrackingFeature.JawOpen,
                    outputFormatId = VRCFTParameterSetId.SingleJawOpen,
                    syncMode = ParameterSyncMode.Float8,
                },
            };
            _buildAvatar = Object.Instantiate(_sourceAvatar);
            _buildAvatar.name = "Build Avatar";
            HashSet<int> existingControllerIds = Resources.FindObjectsOfTypeAll<AnimatorController>().Select(asset => asset.GetInstanceID()).ToHashSet();

            AvatarProcessor.ProcessAvatar(_buildAvatar);

            AnimatorController controller = Resources.FindObjectsOfTypeAll<AnimatorController>()
                .FirstOrDefault(asset => !existingControllerIds.Contains(asset.GetInstanceID()) && asset.name == "USTL FaceTracking Generated FX");
            Assert.That(controller, Is.Not.Null);

            List<Object> allAssets = GenerateFaceTrackingPass.CollectAllAsset(controller);
            Assert.That(allAssets, Has.Some.InstanceOf<AnimatorStateMachine>());
            Assert.That(allAssets, Has.Some.InstanceOf<AnimatorState>());
            Assert.That(allAssets, Has.Some.InstanceOf<AnimatorStateTransition>());
            Assert.That(allAssets, Has.Some.InstanceOf<BlendTree>());
            Assert.That(allAssets, Has.Some.InstanceOf<AnimationClip>());
            Assert.That(allAssets.All(AssetDatabase.Contains), Is.True, "Every generated Animator object must be saved by NDMF.");

            string controllerPath = AssetDatabase.GetAssetPath(controller);
            Assert.That(controllerPath, Is.Not.Empty);
            AssetDatabase.ImportAsset(controllerPath, ImportAssetOptions.ForceUpdate);

            AnimatorController reloadedController = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            Assert.That(reloadedController, Is.Not.Null);
            Assert.That(reloadedController.layers, Is.Not.Empty);
            Assert.That(reloadedController.layers.All(layer => layer.stateMachine), Is.True);
            Assert.That(reloadedController.layers.SelectMany(layer => layer.stateMachine.states).Any(child => child.state && child.state.motion), Is.True,
                "Generated state Motion references must survive serialization and reimport.");
        }

        [Test]
        public void CollectAllAsset_ReturnsEveryGeneratedAnimatorObject()
        {
            AnimatorController controller = AnimatorControllerUtility.CreateAnimatorController("Collection Test");
            AnimatorControllerLayer layer = AnimatorControllerUtility.AddLayer(controller, "Layer");
            AnimatorState firstState = AnimatorControllerUtility.AddState(layer.stateMachine, "First");
            AnimatorState secondState = AnimatorControllerUtility.AddState(layer.stateMachine, "Second");
            AnimatorStateTransition transition = AnimatorControllerUtility.AddTransition(firstState, secondState);
            BlendTree blendTree = new() { name = "Blend Tree" };
            AnimationClip clip = new() { name = "Clip" };
            AnimatorControllerUtility.AddBlendTreeChild(blendTree, clip, 0.0f);
            firstState.motion = blendTree;

            List<Object> allAssets = GenerateFaceTrackingPass.CollectAllAsset(controller);

            Assert.That(allAssets, Does.Contain(controller));
            Assert.That(allAssets, Does.Contain(layer.stateMachine));
            Assert.That(allAssets, Does.Contain(firstState));
            Assert.That(allAssets, Does.Contain(secondState));
            Assert.That(allAssets, Does.Contain(transition));
            Assert.That(allAssets, Does.Contain(blendTree));
            Assert.That(allAssets, Does.Contain(clip));
            Assert.That(allAssets, Is.Unique);

            foreach (Object asset in allAssets)
            {
                Object.DestroyImmediate(asset);
            }

            Assert.That(allAssets.All(asset => !asset), Is.True, "Collected Animator objects must all be destroyable by the PlayMode cleanup path.");
        }

        private GameObject CreateAvatar(out SkinnedMeshRenderer renderer)
        {
            GameObject avatar = new("Source Avatar");
            avatar.AddComponent<Animator>();
            Type avatarDescriptorType = Assembly.Load("VRCSDK3A").GetType("VRC.SDK3.Avatars.Components.VRCAvatarDescriptor");
            Assert.That(avatarDescriptorType, Is.Not.Null);
            avatar.AddComponent(avatarDescriptorType);

            GameObject meshObject = new("Face");
            meshObject.transform.SetParent(avatar.transform, false);
            renderer = meshObject.AddComponent<SkinnedMeshRenderer>();
            _originalMesh = CreateMesh();
            renderer.sharedMesh = _originalMesh;

            GameObject settingsObject = new("Face Tracking");
            settingsObject.transform.SetParent(avatar.transform, false);
            USTLFaceTracking faceTracking = settingsObject.AddComponent<USTLFaceTracking>();
            faceTracking.faceMeshRenderer = renderer;
            faceTracking.featureSettings = new FeatureSetting[0];
            faceTracking.blendShapeSettings = new[]
            {
                new BlendShapeSetting
                {
                    expression = UnifiedExpression.JawOpen,
                    blendShapeName = "JawOpen",
                    maxValue = 50.0f,
                },
            };

            return avatar;
        }

        private static Mesh CreateMesh()
        {
            Mesh mesh = new()
            {
                name = "Original Face Mesh",
                vertices = new[] { Vector3.zero, Vector3.right, Vector3.up, },
                triangles = new[] { 0, 1, 2, },
            };
            Vector3[] deltas = { Vector3.right, Vector3.up, Vector3.forward, };
            mesh.AddBlendShapeFrame("JawOpen", 100.0f, deltas, deltas, deltas);
            return mesh;
        }
    }
}

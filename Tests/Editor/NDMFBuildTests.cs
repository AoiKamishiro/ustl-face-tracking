using System;
using System.Reflection;
using nadena.dev.ndmf;
using NUnit.Framework;
using UnityEditor;
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

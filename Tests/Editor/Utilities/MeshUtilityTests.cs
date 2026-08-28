using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace USTL.FaceTracking.Editor.Tests
{
    public sealed class MeshUtilityTests
    {
        private readonly List<Mesh> _meshes = new();

        [TearDown]
        public void TearDown()
        {
            foreach (Mesh mesh in _meshes)
            {
                if (mesh)
                {
                    Object.DestroyImmediate(mesh);
                }
            }

            _meshes.Clear();
        }

        [Test]
        public void GetBlendShapeNames_ReturnsEmpty_WhenMeshIsNull()
        {
            IReadOnlyList<string> names = MeshUtility.GetBlendShapeNames(null);

            Assert.That(names, Is.Empty);
        }

        [Test]
        public void GetBlendShapeNames_ReturnsEmpty_WhenMeshIsDestroyed()
        {
            Mesh mesh = CreateMeshWithBlendShapes("Smile");
            Object.DestroyImmediate(mesh);

            IReadOnlyList<string> names = MeshUtility.GetBlendShapeNames(mesh);

            Assert.That(names, Is.Empty);
        }

        [Test]
        public void GetBlendShapeNames_ReturnsBlendShapeNamesInIndexOrder()
        {
            Mesh mesh = CreateMeshWithBlendShapes("Smile", "BlinkLeft", "BlinkRight");

            IReadOnlyList<string> names = MeshUtility.GetBlendShapeNames(mesh);

            Assert.That(names, Is.EqualTo(new[] { "Smile", "BlinkLeft", "BlinkRight", }));
        }

        [Test]
        public void GetBlendShapeNames_ReturnsCachedInstance_WhenBlendShapeCountUnchanged()
        {
            Mesh mesh = CreateMeshWithBlendShapes("Smile", "BlinkLeft");

            IReadOnlyList<string> first = MeshUtility.GetBlendShapeNames(mesh);
            IReadOnlyList<string> second = MeshUtility.GetBlendShapeNames(mesh);

            Assert.That(second, Is.SameAs(first));
        }

        [Test]
        public void GetBlendShapeNames_RebuildsCache_WhenBlendShapeCountChanges()
        {
            Mesh mesh = CreateMeshWithBlendShapes("Smile");
            IReadOnlyList<string> before = MeshUtility.GetBlendShapeNames(mesh);

            AddBlendShape(mesh, "BlinkLeft");
            IReadOnlyList<string> after = MeshUtility.GetBlendShapeNames(mesh);

            Assert.That(after, Is.Not.SameAs(before));
            Assert.That(after, Is.EqualTo(new[] { "Smile", "BlinkLeft", }));
        }

        private Mesh CreateMeshWithBlendShapes(params string[] blendShapeNames)
        {
            Mesh mesh = new()
            {
                vertices = new[]
                {
                    Vector3.zero,
                    Vector3.right,
                    Vector3.up,
                },
                triangles = new[] { 0, 1, 2, },
            };
            _meshes.Add(mesh);

            foreach (string blendShapeName in blendShapeNames)
            {
                AddBlendShape(mesh, blendShapeName);
            }

            return mesh;
        }

        private static void AddBlendShape(Mesh mesh, string blendShapeName)
        {
            Vector3[] deltaVertices =
            {
                Vector3.right,
                Vector3.zero,
                Vector3.zero,
            };
            Vector3[] deltaNormals = new Vector3[mesh.vertexCount];
            Vector3[] deltaTangents = new Vector3[mesh.vertexCount];

            mesh.AddBlendShapeFrame(blendShapeName, 100.0f, deltaVertices, deltaNormals, deltaTangents);
        }
    }
}

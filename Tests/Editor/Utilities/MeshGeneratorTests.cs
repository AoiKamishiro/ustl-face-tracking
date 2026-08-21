using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace USTL.FaceTracking.Editor.Tests
{
    public sealed class MeshGeneratorTests
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
        public void Generate_AddsScaledGeneratedBlendShape_ForSingleFrame()
        {
            Mesh originalMesh = new()
            {
                name = "Original",
                vertices = new[]
                {
                    Vector3.zero,
                    Vector3.right,
                    Vector3.up,
                },
                triangles = new[] { 0, 1, 2, },
            };
            _meshes.Add(originalMesh);

            Vector3[] sourceDeltaVertices =
            {
                new(1.0f, 2.0f, 3.0f),
                new(4.0f, 5.0f, 6.0f),
                new(7.0f, 8.0f, 9.0f),
            };
            Vector3[] sourceDeltaNormals =
            {
                new(0.1f, 0.2f, 0.3f),
                new(0.4f, 0.5f, 0.6f),
                new(0.7f, 0.8f, 0.9f),
            };
            Vector3[] sourceDeltaTangents =
            {
                new(0.01f, 0.02f, 0.03f),
                new(0.04f, 0.05f, 0.06f),
                new(0.07f, 0.08f, 0.09f),
            };
            originalMesh.AddBlendShapeFrame("Smile", 75.0f, sourceDeltaVertices, sourceDeltaNormals, sourceDeltaTangents);

            FTBuildContext.BlendShapeBinding binding = new(UnifiedExpression.JawOpen, "Smile", 50.0f);
            Mesh generatedMesh = MeshGenerator.Generate(originalMesh, new[] { binding, });
            _meshes.Add(generatedMesh);

            int generatedBlendShapeIndex = generatedMesh.GetBlendShapeIndex("USTL_JawOpen");
            Assert.That(generatedBlendShapeIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(generatedMesh.GetBlendShapeFrameCount(generatedBlendShapeIndex), Is.EqualTo(1));
            Assert.That(generatedMesh.GetBlendShapeFrameWeight(generatedBlendShapeIndex, 0), Is.EqualTo(37.5f));

            Vector3[] actualDeltaVertices = new Vector3[generatedMesh.vertexCount];
            Vector3[] actualDeltaNormals = new Vector3[generatedMesh.vertexCount];
            Vector3[] actualDeltaTangents = new Vector3[generatedMesh.vertexCount];
            generatedMesh.GetBlendShapeFrameVertices(generatedBlendShapeIndex, 0, actualDeltaVertices, actualDeltaNormals, actualDeltaTangents);

            Assert.That(actualDeltaVertices, Is.EqualTo(sourceDeltaVertices));
            Assert.That(actualDeltaNormals, Is.EqualTo(sourceDeltaNormals));
            Assert.That(actualDeltaTangents, Is.EqualTo(sourceDeltaTangents));
        }

        [TestCase(1.0f, 100.0f)]
        [TestCase(100.0f, 1.0f)]
        [TestCase(1000.0f, 0.1f)]
        [TestCase(1001.0f, 0.1f)]
        [TestCase(float.NaN, 1.0f)]
        [TestCase(float.PositiveInfinity, 1.0f)]
        [TestCase(float.NegativeInfinity, 1.0f)]
        public void Generate_ScalesFrameWeight_UsingNormalizedMaxValue(float maxValue, float expectedScale)
        {
            Mesh originalMesh = new()
            {
                vertices = new[]
                {
                    Vector3.zero,
                    Vector3.right,
                    Vector3.up,
                },
                triangles = new[] { 0, 1, 2, },
            };
            _meshes.Add(originalMesh);

            Vector3[] sourceDeltas =
            {
                new(1.0f, 2.0f, 3.0f),
                new(4.0f, 5.0f, 6.0f),
                new(7.0f, 8.0f, 9.0f),
            };
            originalMesh.AddBlendShapeFrame("Smile", 100.0f, sourceDeltas, sourceDeltas, sourceDeltas);

            FTBuildContext.BlendShapeBinding binding = new(UnifiedExpression.JawOpen, "Smile", maxValue);
            Mesh generatedMesh = MeshGenerator.Generate(originalMesh, new[] { binding, });
            _meshes.Add(generatedMesh);

            int generatedBlendShapeIndex = generatedMesh.GetBlendShapeIndex("USTL_JawOpen");
            Assert.That(generatedMesh.GetBlendShapeFrameWeight(generatedBlendShapeIndex, 0), Is.EqualTo(100.0f / expectedScale));
            Vector3[] actualDeltaVertices = new Vector3[generatedMesh.vertexCount];
            Vector3[] actualDeltaNormals = new Vector3[generatedMesh.vertexCount];
            Vector3[] actualDeltaTangents = new Vector3[generatedMesh.vertexCount];
            generatedMesh.GetBlendShapeFrameVertices(generatedBlendShapeIndex, 0, actualDeltaVertices, actualDeltaNormals, actualDeltaTangents);

            Assert.That(actualDeltaVertices, Is.EqualTo(sourceDeltas));
            Assert.That(actualDeltaNormals, Is.EqualTo(sourceDeltas));
            Assert.That(actualDeltaTangents, Is.EqualTo(sourceDeltas));
        }

        [TestCase(0.0f)]
        [TestCase(-1.0f)]
        public void Generate_SkipsDisabledBinding(float maxValue)
        {
            Mesh originalMesh = CreateTriangleMesh();
            Vector3[] sourceDeltas = { Vector3.right, Vector3.up, Vector3.forward, };
            originalMesh.AddBlendShapeFrame("Smile", 100.0f, sourceDeltas, sourceDeltas, sourceDeltas);

            FTBuildContext.BlendShapeBinding binding = new(UnifiedExpression.JawOpen, "Smile", maxValue);
            Mesh generatedMesh = MeshGenerator.Generate(originalMesh, new[] { binding, });
            _meshes.Add(generatedMesh);

            Assert.That(generatedMesh.GetBlendShapeIndex(binding.GeneratedBlendShapeName), Is.EqualTo(-1));
        }

        [Test]
        public void Generate_LastDisabledBinding_DisablesDuplicateExpression()
        {
            Mesh originalMesh = CreateTriangleMesh();
            Vector3[] sourceDeltas = { Vector3.right, Vector3.up, Vector3.forward, };
            originalMesh.AddBlendShapeFrame("Smile", 100.0f, sourceDeltas, sourceDeltas, sourceDeltas);

            FTBuildContext.BlendShapeBinding[] bindings =
            {
                new(UnifiedExpression.JawOpen, "Smile", 100.0f),
                new(UnifiedExpression.JawOpen, "Smile", 0.0f),
            };
            Mesh generatedMesh = MeshGenerator.Generate(originalMesh, bindings);
            _meshes.Add(generatedMesh);

            Assert.That(generatedMesh.GetBlendShapeIndex(bindings[0].GeneratedBlendShapeName), Is.EqualTo(-1));
        }

        [Test]
        public void Generate_ScalesEveryFrameWeight_AndPreservesDeltas_ForMultipleFrames()
        {
            Mesh originalMesh = CreateTriangleMesh();
            Vector3[] firstDeltas = { Vector3.right, Vector3.up, Vector3.forward, };
            Vector3[] secondDeltas = { Vector3.one, Vector3.one * 2.0f, Vector3.one * 3.0f, };
            originalMesh.AddBlendShapeFrame("Smile", 40.0f, firstDeltas, firstDeltas, firstDeltas);
            originalMesh.AddBlendShapeFrame("Smile", 100.0f, secondDeltas, secondDeltas, secondDeltas);

            Mesh generatedMesh = MeshGenerator.Generate(originalMesh, new[] { new FTBuildContext.BlendShapeBinding(UnifiedExpression.JawOpen, "Smile", 25.0f), });
            _meshes.Add(generatedMesh);

            int index = generatedMesh.GetBlendShapeIndex("USTL_JawOpen");
            Assert.That(generatedMesh.GetBlendShapeFrameWeight(index, 0), Is.EqualTo(10.0f));
            Assert.That(generatedMesh.GetBlendShapeFrameWeight(index, 1), Is.EqualTo(25.0f));
            AssertFrameDeltas(generatedMesh, index, 0, firstDeltas);
            AssertFrameDeltas(generatedMesh, index, 1, secondDeltas);
        }

        [Test]
        public void Generate_UsesLastBinding_ForDuplicateExpression()
        {
            Mesh originalMesh = CreateTriangleMesh();
            Vector3[] firstDeltas = { Vector3.right, Vector3.right, Vector3.right, };
            Vector3[] lastDeltas = { Vector3.up, Vector3.up, Vector3.up, };
            originalMesh.AddBlendShapeFrame("First", 100.0f, firstDeltas, firstDeltas, firstDeltas);
            originalMesh.AddBlendShapeFrame("Last", 100.0f, lastDeltas, lastDeltas, lastDeltas);

            FTBuildContext.BlendShapeBinding[] bindings =
            {
                new(UnifiedExpression.JawOpen, "First", 100.0f),
                new(UnifiedExpression.JawOpen, "Last", 50.0f),
            };
            Mesh generatedMesh = MeshGenerator.Generate(originalMesh, bindings);
            _meshes.Add(generatedMesh);

            int index = generatedMesh.GetBlendShapeIndex("USTL_JawOpen");
            Assert.That(generatedMesh.GetBlendShapeFrameCount(index), Is.EqualTo(1));
            Assert.That(generatedMesh.GetBlendShapeFrameWeight(index, 0), Is.EqualTo(50.0f));
            AssertFrameDeltas(generatedMesh, index, 0, lastDeltas);
        }

        [Test]
        public void Generate_SkipsNoneExpression()
        {
            Mesh originalMesh = CreateTriangleMesh();
            Vector3[] deltas = { Vector3.right, Vector3.up, Vector3.forward, };
            originalMesh.AddBlendShapeFrame("Smile", 100.0f, deltas, deltas, deltas);

            Mesh generatedMesh = MeshGenerator.Generate(originalMesh, new[] { new FTBuildContext.BlendShapeBinding(UnifiedExpression.None, "Smile", 100.0f), });
            _meshes.Add(generatedMesh);

            Assert.That(generatedMesh.GetBlendShapeIndex("USTL_None"), Is.EqualTo(-1));
        }

        [Test]
        public void Generate_ThrowsWhenGeneratedNameAlreadyExists()
        {
            Mesh originalMesh = CreateTriangleMesh();
            Vector3[] deltas = { Vector3.right, Vector3.up, Vector3.forward, };
            originalMesh.AddBlendShapeFrame("Smile", 100.0f, deltas, deltas, deltas);
            originalMesh.AddBlendShapeFrame("USTL_JawOpen", 100.0f, deltas, deltas, deltas);

            Assert.That(() => MeshGenerator.Generate(originalMesh, new[] { new FTBuildContext.BlendShapeBinding(UnifiedExpression.JawOpen, "Smile", 100.0f), }), Throws.InvalidOperationException.With.Message.Contains("USTL_JawOpen"));
        }

        [Test]
        public void Generate_SilentlySkipsMissingSourceBlendShape_AndContinues()
        {
            Mesh originalMesh = new()
            {
                vertices = new[]
                {
                    Vector3.zero,
                    Vector3.right,
                    Vector3.up,
                },
                triangles = new[] { 0, 1, 2, },
            };
            _meshes.Add(originalMesh);

            Vector3[] sourceDeltas =
            {
                Vector3.right,
                Vector3.up,
                Vector3.forward,
            };
            originalMesh.AddBlendShapeFrame("Smile", 100.0f, sourceDeltas, sourceDeltas, sourceDeltas);

            FTBuildContext.BlendShapeBinding missingBinding = new(UnifiedExpression.JawOpen, "Missing", 100.0f);
            FTBuildContext.BlendShapeBinding validBinding = new(UnifiedExpression.MouthCornerPullLeft, "Smile", 100.0f);
            Mesh generatedMesh = MeshGenerator.Generate(originalMesh, new[] { missingBinding, validBinding, });
            _meshes.Add(generatedMesh);

            Assert.That(generatedMesh.GetBlendShapeIndex(missingBinding.GeneratedBlendShapeName), Is.EqualTo(-1));
            Assert.That(generatedMesh.GetBlendShapeIndex(validBinding.GeneratedBlendShapeName), Is.GreaterThanOrEqualTo(0));
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void Generate_DoesNotModifyOriginalMesh()
        {
            Mesh originalMesh = new()
            {
                name = "Original",
                vertices = new[]
                {
                    Vector3.zero,
                    Vector3.right,
                    Vector3.up,
                },
                triangles = new[] { 0, 1, 2, },
                bounds = new Bounds(new Vector3(1.0f, 2.0f, 3.0f), new Vector3(4.0f, 5.0f, 6.0f)),
            };
            _meshes.Add(originalMesh);

            Vector3[] sourceDeltaVertices =
            {
                new(1.0f, 2.0f, 3.0f),
                new(4.0f, 5.0f, 6.0f),
                new(7.0f, 8.0f, 9.0f),
            };
            Vector3[] sourceDeltaNormals =
            {
                new(0.1f, 0.2f, 0.3f),
                new(0.4f, 0.5f, 0.6f),
                new(0.7f, 0.8f, 0.9f),
            };
            Vector3[] sourceDeltaTangents =
            {
                new(0.01f, 0.02f, 0.03f),
                new(0.04f, 0.05f, 0.06f),
                new(0.07f, 0.08f, 0.09f),
            };
            originalMesh.AddBlendShapeFrame("Smile", 75.0f, sourceDeltaVertices, sourceDeltaNormals, sourceDeltaTangents);

            Vector3[] originalVertices = originalMesh.vertices;
            Bounds originalBounds = originalMesh.bounds;
            FTBuildContext.BlendShapeBinding binding = new(UnifiedExpression.JawOpen, "Smile", 50.0f);

            Mesh generatedMesh = MeshGenerator.Generate(originalMesh, new[] { binding, });
            _meshes.Add(generatedMesh);

            Assert.That(generatedMesh, Is.Not.SameAs(originalMesh));
            Assert.That(originalMesh.name, Is.EqualTo("Original"));
            Assert.That(originalMesh.vertices, Is.EqualTo(originalVertices));
            Assert.That(originalMesh.bounds, Is.EqualTo(originalBounds));
            Assert.That(originalMesh.blendShapeCount, Is.EqualTo(1));
            Assert.That(originalMesh.GetBlendShapeName(0), Is.EqualTo("Smile"));
            Assert.That(originalMesh.GetBlendShapeIndex(binding.GeneratedBlendShapeName), Is.EqualTo(-1));
            Assert.That(originalMesh.GetBlendShapeFrameCount(0), Is.EqualTo(1));
            Assert.That(originalMesh.GetBlendShapeFrameWeight(0, 0), Is.EqualTo(75.0f));

            Vector3[] actualDeltaVertices = new Vector3[originalMesh.vertexCount];
            Vector3[] actualDeltaNormals = new Vector3[originalMesh.vertexCount];
            Vector3[] actualDeltaTangents = new Vector3[originalMesh.vertexCount];
            originalMesh.GetBlendShapeFrameVertices(0, 0, actualDeltaVertices, actualDeltaNormals, actualDeltaTangents);

            Assert.That(actualDeltaVertices, Is.EqualTo(sourceDeltaVertices));
            Assert.That(actualDeltaNormals, Is.EqualTo(sourceDeltaNormals));
            Assert.That(actualDeltaTangents, Is.EqualTo(sourceDeltaTangents));
        }

        private Mesh CreateTriangleMesh()
        {
            Mesh mesh = new()
            {
                vertices = new[] { Vector3.zero, Vector3.right, Vector3.up, },
                triangles = new[] { 0, 1, 2, },
            };
            _meshes.Add(mesh);
            return mesh;
        }

        private static void AssertFrameDeltas(Mesh mesh, int blendShapeIndex, int frameIndex, Vector3[] expected)
        {
            Vector3[] vertices = new Vector3[mesh.vertexCount];
            Vector3[] normals = new Vector3[mesh.vertexCount];
            Vector3[] tangents = new Vector3[mesh.vertexCount];
            mesh.GetBlendShapeFrameVertices(blendShapeIndex, frameIndex, vertices, normals, tangents);
            Assert.That(vertices, Is.EqualTo(expected));
            Assert.That(normals, Is.EqualTo(expected));
            Assert.That(tangents, Is.EqualTo(expected));
        }
    }
}

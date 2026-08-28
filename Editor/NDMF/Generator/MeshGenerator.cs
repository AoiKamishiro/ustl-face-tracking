using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace USTL.FaceTracking.Editor
{
    internal static class MeshGenerator
    {
        private const string GeneratedMeshNameSuffix = " (USTL FT Generated BlendShapes)";

        internal static void Generate(FTBuildContext context)
        {
            Mesh originalMesh = context.OriginalMesh;
            IReadOnlyList<FTBuildContext.BlendShapeBinding> bindings = context.BlendShapeBindings;
            Mesh generatedMesh = Generate(originalMesh, bindings);
            context.GeneratedMesh = generatedMesh;
        }

        internal static Mesh Generate(Mesh originalMesh, IReadOnlyList<FTBuildContext.BlendShapeBinding> bindings)
        {
            HashSet<UnifiedExpression> validatedExpressions = new();
            for (int bindingIndex = bindings.Count - 1; bindingIndex >= 0; bindingIndex--)
            {
                FTBuildContext.BlendShapeBinding binding = bindings[bindingIndex];
                if (binding.Expression == UnifiedExpression.None || !validatedExpressions.Add(binding.Expression) || binding.MaxValue <= 0.0f || binding.GetBlendshapeIndex(originalMesh) < 0)
                {
                    continue;
                }

                if (originalMesh.GetBlendShapeIndex(binding.GeneratedBlendShapeName) >= 0)
                {
                    throw new InvalidOperationException($"The generated BlendShape name '{binding.GeneratedBlendShapeName}' is already used by the source mesh.");
                }
            }

            Mesh generatedMesh = Object.Instantiate(originalMesh);
            generatedMesh.name = $"{originalMesh.name}{GeneratedMeshNameSuffix}";

            int vertexCount = originalMesh.vertexCount;
            Vector3[] deltaVertices = new Vector3[vertexCount];
            Vector3[] deltaNormals = new Vector3[vertexCount];
            Vector3[] deltaTangents = new Vector3[vertexCount];

            HashSet<UnifiedExpression> generatedExpressions = new();
            for (int bindingIndex = bindings.Count - 1; bindingIndex >= 0; bindingIndex--)
            {
                FTBuildContext.BlendShapeBinding binding = bindings[bindingIndex];
                if (binding.Expression == UnifiedExpression.None || !generatedExpressions.Add(binding.Expression) || binding.MaxValue <= 0.0f)
                {
                    continue;
                }

                int blendShapeIndex = binding.GetBlendshapeIndex(originalMesh);
                if (blendShapeIndex < 0)
                {
                    continue;
                }

                int frameCount = originalMesh.GetBlendShapeFrameCount(blendShapeIndex);
                for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
                {
                    originalMesh.GetBlendShapeFrameVertices(blendShapeIndex, frameIndex, deltaVertices, deltaNormals, deltaTangents);
                    float sourceFrameWeight = originalMesh.GetBlendShapeFrameWeight(blendShapeIndex, frameIndex);
                    float generatedFrameWeight = sourceFrameWeight * binding.MaxValue / 100.0f;
                    generatedMesh.AddBlendShapeFrame(binding.GeneratedBlendShapeName, generatedFrameWeight, deltaVertices, deltaNormals, deltaTangents);
                }
            }

            generatedMesh.bounds = originalMesh.bounds;
            return generatedMesh;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace USTL.FaceTracking.Editor
{
    public static class MeshUtility
    {
        private static readonly Dictionary<Mesh, IReadOnlyList<string>> BlendShapeNameCache = new();

        internal static IReadOnlyList<string> GetBlendShapeNames(Mesh mesh)
        {
            if (!mesh)
            {
                return Array.Empty<string>();
            }

            if (BlendShapeNameCache.TryGetValue(mesh, out IReadOnlyList<string> blendShapeNames) && blendShapeNames.Count == mesh.blendShapeCount)
            {
                return blendShapeNames;
            }

            blendShapeNames = BuildBlendShapeNames(mesh);
            BlendShapeNameCache[mesh] = blendShapeNames;
            return blendShapeNames;
        }

        private static IReadOnlyList<string> BuildBlendShapeNames(Mesh mesh)
        {
            string[] blendShapeNames = new string[mesh.blendShapeCount];

            for (int i = 0; i < mesh.blendShapeCount; i++)
            {
                blendShapeNames[i] = mesh.GetBlendShapeName(i);
            }

            return blendShapeNames;
        }
    }
}

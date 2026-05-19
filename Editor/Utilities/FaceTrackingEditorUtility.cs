using System;
using System.Collections.Generic;
using UnityEngine;

namespace USTL.FaceTracking.Editor
{
    internal static class FaceTrackingEditorUtility
    {
        private static List<UnifiedExpression> _allExpressions;
        private static List<FaceTrackingFeatureDefinition> _allFeatureDefinitions;

        private static readonly Dictionary<Mesh, IReadOnlyList<string>> BlendShapeNameCache = new();

        internal static IReadOnlyList<UnifiedExpression> AllExpressions
        {
            get
            {
                if (_allExpressions != null)
                {
                    return _allExpressions;
                }

                Array all = Enum.GetValues(typeof(UnifiedExpression));
                _allExpressions = new List<UnifiedExpression>(all.Length - 1);

                foreach (UnifiedExpression expression in all)
                {
                    if (expression == UnifiedExpression.None)
                    {
                        continue;
                    }

                    _allExpressions.Add(expression);
                }

                return _allExpressions;
            }
        }

        internal static IReadOnlyList<FaceTrackingFeatureDefinition> AllFeatureDefinitions
        {
            get
            {
                if (_allFeatureDefinitions != null)
                {
                    return _allFeatureDefinitions;
                }

                Array all = Enum.GetValues(typeof(FaceTrackingFeature));
                _allFeatureDefinitions = new List<FaceTrackingFeatureDefinition>(all.Length);

                foreach (FaceTrackingFeature feature in all)
                {
                    if (FaceTrackingFeatureDefinition.All.TryGetValue(feature, out FaceTrackingFeatureDefinition featureDefinition))
                    {
                        _allFeatureDefinitions.Add(featureDefinition);
                    }
                }

                return _allFeatureDefinitions;
            }
        }

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

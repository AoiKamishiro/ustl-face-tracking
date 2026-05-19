using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace USTL.FaceTracking.Editor
{
    internal static class BlendShapeAnimationClipFactory
    {
        internal static AnimationClip Create(AnimatorController controller, string name, string rendererPath, IReadOnlyList<BlendShapeTarget> targets, float parameterValue)
        {
            AnimationClip clip = new()
            {
                name = name,
                hideFlags = HideFlags.HideInHierarchy,
            };

            AssetDatabase.AddObjectToAsset(clip, controller);

            Dictionary<string, float> values = BuildBlendShapeValues(targets, parameterValue);
            foreach (KeyValuePair<string, float> value in values)
            {
                AnimationUtility.SetEditorCurve(clip, new EditorCurveBinding
                {
                    path = rendererPath,
                    type = typeof(SkinnedMeshRenderer),
                    propertyName = $"blendShape.{value.Key}",
                }, AnimationCurve.Constant(0f, 1f / 60f, value.Value));
            }

            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static Dictionary<string, float> BuildBlendShapeValues(IReadOnlyList<BlendShapeTarget> targets, float parameterValue)
        {
            Dictionary<string, float> values = new();
            foreach (BlendShapeTarget target in targets)
            {
                float value = FaceTrackingWeightEvaluator.Evaluate(target.CurveType, parameterValue) * target.Value;

                if (!values.TryGetValue(target.Name, out float existing) || Math.Abs(value) > Math.Abs(existing))
                {
                    values[target.Name] = value;
                }
            }

            return values;
        }
    }
}

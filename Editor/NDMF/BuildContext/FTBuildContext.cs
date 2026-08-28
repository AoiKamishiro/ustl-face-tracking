using System;
using System.Collections.Generic;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using UnityEditor.Animations;
using UnityEngine;

namespace USTL.FaceTracking.Editor
{
    internal class FTBuildContext
    {
        internal FTBuildContext(BuildContext buildContext, USTLFaceTracking source)
        {
            BuildContext = buildContext;
            Source = source;
            OriginalMesh = source.faceMeshRenderer.sharedMesh;
            GeneratedMesh = OriginalMesh;

            List<BlendShapeBinding> bindings = new(EnumUtility.GetAllElements<UnifiedExpression>().Count);
            BlendShapeSetting[] blendShapeSettings = source.blendShapeSettings ?? Array.Empty<BlendShapeSetting>();
            foreach (BlendShapeSetting setting in blendShapeSettings)
            {
                if (setting == null || setting.expression == UnifiedExpression.None || string.IsNullOrWhiteSpace(setting.blendShapeName))
                {
                    continue;
                }

                BlendShapeBinding binding = new(setting.expression, setting.blendShapeName, setting.maxValue);
                bindings.Add(binding);
            }

            BlendShapeBindings = bindings.AsReadOnly();
        }

        internal BuildContext BuildContext { get; }
        internal USTLFaceTracking Source { get; }
        internal Transform AvatarRootTransform => BuildContext.AvatarRootTransform;
        internal IReadOnlyList<BlendShapeBinding> BlendShapeBindings { get; }
        internal Mesh OriginalMesh { get; }
        internal Mesh GeneratedMesh { get; set; }
        internal GameObject GeneratedObject { get; set; }
        internal ModularAvatarMergeAnimator ModularAvatarMergeAnimator { get; set; }
        internal ModularAvatarParameters ModularAvatarParameters { get; set; }
        internal List<AnimatorGenerator.ParameterAnimation> ParameterAnimations { get; set; } = new();
        internal AnimatorController AnimatorController { get; set; }

        internal readonly struct BlendShapeBinding
        {
            private const string GeneratedBlendShapePrefix = "USTL_";

            public BlendShapeBinding(UnifiedExpression expression, string blendShapeName, float maxValue)
            {
                Expression = expression;
                BlendShapeName = blendShapeName;
                MaxValue = BlendshapeUtility.ClampMaxValue(maxValue);
            }

            public readonly UnifiedExpression Expression;
            public readonly float MaxValue;
            public readonly string BlendShapeName;
            public string GeneratedBlendShapeName => GeneratedBlendShapePrefix + Expression;

            public int GetBlendshapeIndex(Mesh mesh)
            {
                return mesh.GetBlendShapeIndex(BlendShapeName);
            }
        }
    }
}

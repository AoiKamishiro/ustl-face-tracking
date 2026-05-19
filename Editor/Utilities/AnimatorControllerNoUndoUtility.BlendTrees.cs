using UnityEditor.Animations;
using UnityEngine;

namespace USTL.FaceTracking.Editor
{
    internal static partial class AnimatorControllerNoUndoUtility
    {
        internal static void AddBlendTreeChild(BlendTree tree, Motion motion)
        {
            AddBlendTreeChild(tree, motion, Vector2.zero, 0f);
        }

        internal static void AddBlendTreeChild(BlendTree tree, Motion motion, Vector2 position)
        {
            AddBlendTreeChild(tree, motion, position, 0f);
        }

        internal static void AddBlendTreeChild(BlendTree tree, Motion motion, float threshold)
        {
            AddBlendTreeChild(tree, motion, Vector2.zero, threshold);
        }

        internal static void AddBlendTreeChild(BlendTree tree, Motion motion, Vector2 position, float threshold)
        {
            ThrowIfNull(tree, nameof(tree));

            ChildMotion child = new()
            {
                timeScale = 1f,
                motion = motion,
                position = position,
                threshold = threshold,
                directBlendParameter = DefaultBlendParameterName,
            };

            tree.children = AddToArray(tree.children, child);
            MarkDirty(tree);
        }

        internal static BlendTree CreateBlendTreeChild(BlendTree parent, float threshold)
        {
            return CreateBlendTreeChild(parent, Vector2.zero, threshold);
        }

        internal static BlendTree CreateBlendTreeChild(BlendTree parent, Vector2 position)
        {
            return CreateBlendTreeChild(parent, position, 0f);
        }

        internal static BlendTree CreateBlendTreeChild(BlendTree parent, Vector2 position, float threshold)
        {
            ThrowIfNull(parent, nameof(parent));

            BlendTree child = new()
            {
                name = DefaultBlendTreeName,
                hideFlags = HideFlags.HideInHierarchy,
            };

            AddObjectToAssetIfPossible(child, parent);
            AddBlendTreeChild(parent, child, position, threshold);
            MarkDirty(child);
            return child;
        }
    }
}

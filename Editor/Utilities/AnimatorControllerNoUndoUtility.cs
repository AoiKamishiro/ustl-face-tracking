using System;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace USTL.FaceTracking.Editor
{
    /// <summary>
    ///     Creates AnimatorController assets without using Unity's public mutation helpers that register Undo operations.
    /// </summary>
    internal static partial class AnimatorControllerNoUndoUtility
    {
        private const string DefaultLayerName = "Base Layer";
        private const string DefaultBlendParameterName = "Blend";
        private const string DefaultBlendTreeName = "BlendTree";
        private const float DefaultTransitionDuration = 0.25f;
        private const float DefaultTransitionExitTime = 0.75f;

        private static readonly Vector3 FirstStatePosition = new(200f, 0f, 0f);
        private static readonly Vector3 NextStateOffset = new(35f, 65f, 0f);

        internal static AnimatorController CreateAnimatorController(string name)
        {
            AnimatorController controller = new()
            {
                name = name,
                layers = Array.Empty<AnimatorControllerLayer>(),
                parameters = Array.Empty<AnimatorControllerParameter>(),
            };

            MarkDirty(controller);
            return controller;
        }

        internal static AnimatorController CreateAnimatorControllerAtPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("AnimatorController asset path must not be null or empty.", nameof(path));
            }

            AnimatorController controller = CreateAnimatorController(Path.GetFileName(path));
            AssetDatabase.CreateAsset(controller, path);
            AddLayer(controller, DefaultLayerName);
            MarkDirty(controller);
            return controller;
        }

        internal static AnimatorController CreateAnimatorControllerAtPathWithClip(string path, AnimationClip clip)
        {
            ThrowIfNull(clip, nameof(clip));

            AnimatorController controller = CreateAnimatorControllerAtPath(path);
            AddMotion(controller, clip);
            return controller;
        }

        internal static AnimatorControllerLayer AddLayer(AnimatorController controller, string name)
        {
            ThrowIfNull(controller, nameof(controller));

            AnimatorStateMachine stateMachine = new()
            {
                name = controller.MakeUniqueLayerName(name),
                hideFlags = HideFlags.HideInHierarchy,
            };

            AddObjectToAssetIfPossible(stateMachine, controller);

            AnimatorControllerLayer layer = new()
            {
                name = stateMachine.name,
                stateMachine = stateMachine,
            };

            AddLayer(controller, layer);
            MarkDirty(stateMachine);
            return layer;
        }

        internal static void AddLayer(AnimatorController controller, AnimatorControllerLayer layer)
        {
            ThrowIfNull(controller, nameof(controller));
            if (layer == null)
            {
                throw new ArgumentNullException(nameof(layer));
            }

            AnimatorControllerLayer[] layers = AddToArray(controller.layers, layer);
            controller.layers = layers;
            MarkDirty(controller);
        }

        internal static AnimatorControllerParameter AddParameter(AnimatorController controller, string name, AnimatorControllerParameterType type)
        {
            ThrowIfNull(controller, nameof(controller));

            AnimatorControllerParameter parameter = new()
            {
                name = controller.MakeUniqueParameterName(name),
                type = type,
            };

            AddParameter(controller, parameter);
            return parameter;
        }

        internal static void AddParameter(AnimatorController controller, AnimatorControllerParameter parameter)
        {
            ThrowIfNull(controller, nameof(controller));
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            AnimatorControllerParameter[] parameters = AddToArray(controller.parameters, parameter);
            controller.parameters = parameters;
            MarkDirty(controller);
        }

        private static string GetDefaultBlendTreeParameter(AnimatorController controller)
        {
            AnimatorControllerParameter[] parameters = controller.parameters ?? Array.Empty<AnimatorControllerParameter>();
            foreach (AnimatorControllerParameter parameter in parameters)
            {
                if (parameter.type == AnimatorControllerParameterType.Float)
                {
                    return parameter.name;
                }
            }

            AddParameter(controller, DefaultBlendParameterName, AnimatorControllerParameterType.Float);
            return DefaultBlendParameterName;
        }

        private static void AddObjectToAssetIfPossible(Object obj, Object assetOwner)
        {
            string assetPath = AssetDatabase.GetAssetPath(assetOwner);
            if (string.IsNullOrEmpty(assetPath) || EditorUtility.IsPersistent(obj))
            {
                return;
            }

            AssetDatabase.AddObjectToAsset(obj, assetPath);
        }

        private static T[] AddToArray<T>(T[] values, T value)
        {
            if (values == null || values.Length == 0)
            {
                return new[] { value, };
            }

            T[] result = new T[values.Length + 1];
            Array.Copy(values, result, values.Length);
            result[values.Length] = value;
            return result;
        }

        private static void MarkDirty(Object obj)
        {
            if (obj)
            {
                EditorUtility.SetDirty(obj);
            }
        }

        private static void ThrowIfNull(Object obj, string paramName)
        {
            if (!obj)
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}

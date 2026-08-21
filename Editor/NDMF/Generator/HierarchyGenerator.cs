using nadena.dev.modular_avatar.core;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace USTL.FaceTracking.Editor
{
    internal static class HierarchyGenerator
    {
        private const string GeneratedObjectName = "USTL FaceTracking Generated";

        internal static void Generate(FTBuildContext context)
        {
            GameObject generatedObject = new(GeneratedObjectName);
            generatedObject.transform.SetParent(context.Source.transform, false);

            ModularAvatarParameters parameters = generatedObject.AddComponent<ModularAvatarParameters>();
            ModularAvatarMergeAnimator mergeAnimator = generatedObject.AddComponent<ModularAvatarMergeAnimator>();
            mergeAnimator.deleteAttachedAnimator = true;
            mergeAnimator.pathMode = MergeAnimatorPathMode.Absolute;
            mergeAnimator.matchAvatarWriteDefaults = false;
            mergeAnimator.mergeAnimatorMode = MergeAnimatorMode.Append;
            mergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.FX;

            context.GeneratedObject = generatedObject;
            context.ModularAvatarMergeAnimator = mergeAnimator;
            context.ModularAvatarParameters = parameters;
        }
    }
}

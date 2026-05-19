using System.Collections.Generic;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using Object = UnityEngine.Object;

namespace USTL.FaceTracking.Editor
{
    internal sealed class GenerateFaceTrackingPass : Pass<GenerateFaceTrackingPass>
    {
        public override string DisplayName => "Generate U-Stella FaceTracking";

        protected override void Execute(BuildContext context)
        {
            USTLFaceTracking[] components = context.AvatarRootTransform.GetComponentsInChildren<USTLFaceTracking>(true);

            foreach (USTLFaceTracking component in components)
            {
                if (!component)
                {
                    continue;
                }

                Generate(context, component);
                Object.DestroyImmediate(component);
            }
        }

        private static void Generate(BuildContext context, USTLFaceTracking source)
        {
            GameObject generatedObject = new(FaceTrackingGenerationConstants.GeneratedObjectName);
            generatedObject.transform.SetParent(source.transform, false);

            AnimatorController controller = CreateController(context, source);
            ModularAvatarParameters parameters = generatedObject.AddComponent<ModularAvatarParameters>();
            ModularAvatarMergeAnimator mergeAnimator = generatedObject.AddComponent<ModularAvatarMergeAnimator>();
            List<Object> generatedAssets = new();
            List<SelectedParameterSetting> selectedParameters = SelectedParameterCollector.Collect(source);
            List<SelectedParameterSetting> targetBackedParameters = SyncParameterUsageCalculator.GetTargetBackedParameters(source, selectedParameters);

            FaceTrackingModularAvatarParameterGenerator.Populate(parameters, targetBackedParameters);
            FaceTrackingAnimatorControllerGenerator.Populate(context, source, controller, targetBackedParameters, generatedAssets);

            ConfigureMergeAnimator(mergeAnimator, controller);
            RegisterGeneratedObjects(context, generatedObject, parameters, mergeAnimator, controller, generatedAssets);
        }

        private static AnimatorController CreateController(BuildContext context, USTLFaceTracking source)
        {
            AnimatorController controller = AnimatorControllerNoUndoUtility.CreateAnimatorController($"{FaceTrackingGenerationConstants.GeneratedControllerName} ({source.gameObject.name})");

            context.AssetSaver.SaveAsset(controller);
            return controller;
        }

        private static void ConfigureMergeAnimator(ModularAvatarMergeAnimator mergeAnimator, AnimatorController controller)
        {
            mergeAnimator.animator = controller;
            mergeAnimator.deleteAttachedAnimator = true;
            mergeAnimator.pathMode = MergeAnimatorPathMode.Absolute;
            mergeAnimator.matchAvatarWriteDefaults = true;
            mergeAnimator.mergeAnimatorMode = MergeAnimatorMode.Append;
            mergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
        }

        private static void RegisterGeneratedObjects(BuildContext context, GameObject generatedObject, ModularAvatarParameters parameters, ModularAvatarMergeAnimator mergeAnimator, AnimatorController controller, IReadOnlyList<Object> generatedAssets)
        {
            using (new ObjectRegistryScope(context.ObjectRegistry))
            {
                ObjectRegistry.GetReference(generatedObject);
                ObjectRegistry.GetReference(generatedObject.transform);
                ObjectRegistry.GetReference(parameters);
                ObjectRegistry.GetReference(mergeAnimator);
                ObjectRegistry.GetReference(controller);

                foreach (Object generatedAsset in generatedAssets)
                {
                    if (generatedAsset)
                    {
                        ObjectRegistry.GetReference(generatedAsset);
                    }
                }
            }

            EditorUtility.SetDirty(generatedObject);
            EditorUtility.SetDirty(parameters);
            EditorUtility.SetDirty(mergeAnimator);
            EditorUtility.SetDirty(controller);

            foreach (Object generatedAsset in generatedAssets)
            {
                if (generatedAsset)
                {
                    EditorUtility.SetDirty(generatedAsset);
                }
            }
        }
    }
}

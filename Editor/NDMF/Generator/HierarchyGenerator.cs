using nadena.dev.modular_avatar.core;
using System;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

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

        internal static void GenerateEyelidResponseMenu(FTBuildContext context, bool synced)
        {
            GameObject menuRoot = context.GeneratedObject;
            menuRoot.AddComponent<ModularAvatarMenuInstaller>();

            ModularAvatarMenuItem subMenuItem = menuRoot.AddComponent<ModularAvatarMenuItem>();
            subMenuItem.Control = new VRCExpressionsMenu.Control
            {
                name = "Face Tracking",
                type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                parameter = new VRCExpressionsMenu.Control.Parameter { name = string.Empty, },
                subParameters = Array.Empty<VRCExpressionsMenu.Control.Parameter>(),
                labels = Array.Empty<VRCExpressionsMenu.Control.Label>(),
            };
            subMenuItem.label = "Face Tracking";
            subMenuItem.MenuSource = SubmenuSource.Children;

            GameObject radialObject = new("Eyelid Sensitivity");
            radialObject.transform.SetParent(menuRoot.transform, false);

            ModularAvatarMenuItem radialItem = radialObject.AddComponent<ModularAvatarMenuItem>();
            radialItem.Control = new VRCExpressionsMenu.Control
            {
                name = radialObject.name,
                type = VRCExpressionsMenu.Control.ControlType.RadialPuppet,
                parameter = new VRCExpressionsMenu.Control.Parameter { name = string.Empty, },
                subParameters = new[]
                {
                    new VRCExpressionsMenu.Control.Parameter { name = AnimatorGenerator.EyelidResponseParameterName, },
                },
                labels = Array.Empty<VRCExpressionsMenu.Control.Label>(),
            };
            radialItem.automaticValue = false;
            radialItem.isSaved = true;
            radialItem.isSynced = synced;
        }
    }
}

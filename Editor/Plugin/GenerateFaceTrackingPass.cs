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
        private const string GeneratedObjectName = "USTL FaceTracking Generated";
        private const string GeneratedControllerName = "USTL FaceTracking Generated FX";

        public override string DisplayName => "Generate U-Stella FaceTracking";

        protected override void Execute(BuildContext context)
        {
            USTLFaceTracking[] components = context.AvatarRootTransform.GetComponentsInChildren<USTLFaceTracking>(true);

            bool proceeded = false;
            foreach (USTLFaceTracking component in components)
            {
                if (!component)
                {
                    continue;
                }

                if (!proceeded)
                {
                    Generate(context, component);
                    proceeded = true;
                }

                Object.DestroyImmediate(component);
            }
        }

        private static void Generate(BuildContext context, USTLFaceTracking source)
        {
            GameObject generatedObject = new(GeneratedObjectName);
            generatedObject.transform.SetParent(source.transform, false);

            List<ParameterConfig> parameterConfigs = new();
            AnimatorController controller = GenerateFaceTrackingProcess.GenerateAnimatorController($"{GeneratedControllerName} ({source.gameObject.name})", source, context.AvatarRootTransform, out List<GenerateFaceTrackingProcess.ParameterAnimation> parameterAnimation);

            foreach (GenerateFaceTrackingProcess.ParameterAnimation anims in parameterAnimation)
            {
                AddParameterConfigs(anims, ref parameterConfigs);
            }

            ModularAvatarParameters parameters = generatedObject.AddComponent<ModularAvatarParameters>();
            parameters.parameters = parameterConfigs;
            ModularAvatarMergeAnimator mergeAnimator = generatedObject.AddComponent<ModularAvatarMergeAnimator>();
            mergeAnimator.animator = controller;
            mergeAnimator.deleteAttachedAnimator = true;
            mergeAnimator.pathMode = MergeAnimatorPathMode.Absolute;
            mergeAnimator.matchAvatarWriteDefaults = false;
            mergeAnimator.mergeAnimatorMode = MergeAnimatorMode.Append;
            mergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.FX;

            RegisterGeneratedObjects(context, controller);
            RegisterGeneratedObjects(context, generatedObject);
        }


        private static ParameterConfig CreateParameterConfig(string name, ParameterSyncType syncType, bool localOnly, float defaultValue)
        {
            return new ParameterConfig
            {
                nameOrPrefix = name,
                remapTo = string.Empty,
                syncType = syncType,
                localOnly = localOnly,
                defaultValue = defaultValue,
                hasExplicitDefaultValue = !Mathf.Approximately(defaultValue, 0.0f),
            };
        }

        internal static List<Object> CollectAllAsset(AnimatorController controller)
        {
            List<Object> allAssets = new();
            HashSet<Object> visitedObjects = new();
            HashSet<AnimatorStateMachine> visitedStateMachines = new();
            HashSet<AnimatorState> visitedStates = new();
            HashSet<Motion> visitedMotions = new();

            AddGeneratedObject(controller);

            AnimatorControllerLayer[] layers = controller.layers;
            if (layers != null)
            {
                foreach (AnimatorControllerLayer layer in layers)
                {
                    AddGeneratedObject(layer.avatarMask);
                    AddStateMachine(layer.stateMachine);

                    if (layer.syncedLayerIndex < 0 || layer.syncedLayerIndex >= layers.Length)
                    {
                        continue;
                    }

                    AnimatorStateMachine syncedStateMachine = layers[layer.syncedLayerIndex].stateMachine;
                    foreach (ChildAnimatorState childState in syncedStateMachine.states)
                    {
                        if (childState.state)
                        {
                            AddMotion(layer.GetOverrideMotion(childState.state));
                        }
                    }
                }
            }

            return allAssets;

            void AddGeneratedObject(Object asset)
            {
                if (!asset || !visitedObjects.Add(asset))
                {
                    return;
                }

                if (asset == controller)
                {
                    allAssets.Add(asset);
                }
            }

            void AddStateMachine(AnimatorStateMachine stateMachine)
            {
                if (!stateMachine || !visitedStateMachines.Add(stateMachine))
                {
                    return;
                }

                AddGeneratedObject(stateMachine);
                AddState(stateMachine.defaultState);
                AddAnimatorStateTransitions(stateMachine.anyStateTransitions);
                AddAnimatorTransitions(stateMachine.entryTransitions);

                foreach (ChildAnimatorState childState in stateMachine.states)
                {
                    AddState(childState.state);
                }

                foreach (ChildAnimatorStateMachine childStateMachine in stateMachine.stateMachines)
                {
                    AddStateMachine(childStateMachine.stateMachine);
                    AddAnimatorTransitions(stateMachine.GetStateMachineTransitions(childStateMachine.stateMachine));
                }
            }

            void AddState(AnimatorState state)
            {
                if (!state || !visitedStates.Add(state))
                {
                    return;
                }

                AddGeneratedObject(state);
                AddMotion(state.motion);
                AddAnimatorStateTransitions(state.transitions);

                foreach (StateMachineBehaviour behaviour in state.behaviours)
                {
                    AddGeneratedObject(behaviour);
                }
            }

            void AddMotion(Motion motion)
            {
                if (!motion || !visitedMotions.Add(motion))
                {
                    return;
                }

                AddGeneratedObject(motion);
                if (motion is not BlendTree blendTree)
                {
                    return;
                }

                foreach (ChildMotion childMotion in blendTree.children)
                {
                    AddMotion(childMotion.motion);
                }
            }

            void AddAnimatorStateTransitions(AnimatorStateTransition[] transitions)
            {
                if (transitions == null)
                {
                    return;
                }

                foreach (AnimatorStateTransition transition in transitions)
                {
                    AddTransition(transition);
                }
            }

            void AddAnimatorTransitions(AnimatorTransition[] transitions)
            {
                if (transitions == null)
                {
                    return;
                }

                foreach (AnimatorTransition transition in transitions)
                {
                    AddTransition(transition);
                }
            }

            void AddTransition(AnimatorTransitionBase transition)
            {
                if (!transition)
                {
                    return;
                }

                AddGeneratedObject(transition);
                AddState(transition.destinationState);
                AddStateMachine(transition.destinationStateMachine);
            }
        }

        private static void RegisterGeneratedObjects(BuildContext context, AnimatorController controller)
        {
            List<Object> allAssets = CollectAllAsset(controller);

            using (SerializationScope scope = context.OpenSerializationScope())
            {
                foreach (Object generatedAsset in allAssets)
                {
                    scope.SaveAsset(generatedAsset);
                }
            }

            using (new ObjectRegistryScope(context.ObjectRegistry))
            {
                foreach (Object generatedAsset in allAssets)
                {
                    if (generatedAsset)
                    {
                        ObjectRegistry.GetReference(generatedAsset);
                    }
                }
            }

            foreach (Object generatedAsset in allAssets)
            {
                if (generatedAsset)
                {
                    EditorUtility.SetDirty(generatedAsset);
                }
            }
        }

        private static void RegisterGeneratedObjects(BuildContext context, GameObject gameObject)
        {
            List<Object> allAssets = new() { gameObject, };
            allAssets.AddRange(gameObject.GetComponents<MonoBehaviour>());

            using (new ObjectRegistryScope(context.ObjectRegistry))
            {
                foreach (Object generatedAsset in allAssets)
                {
                    if (generatedAsset)
                    {
                        ObjectRegistry.GetReference(generatedAsset);
                    }
                }
            }

            foreach (Object generatedAsset in allAssets)
            {
                if (generatedAsset)
                {
                    EditorUtility.SetDirty(generatedAsset);
                }
            }
        }


        private static void AddParameterConfigs(GenerateFaceTrackingProcess.ParameterAnimation parameterAnimation, ref List<ParameterConfig> parameterConfigs)
        {
            bool syncFloatParameter = parameterAnimation.SyncMode == ParameterSyncMode.Float8;
            parameterConfigs.Add(CreateParameterConfig(parameterAnimation.ParameterName, ParameterSyncType.Float, !syncFloatParameter, parameterAnimation.DefaultValue));

            int bitCount = GenerateFaceTrackingProcess.GetBinaryBitCount(parameterAnimation.SyncMode);
            if (bitCount <= 0)
            {
                return;
            }

            for (int bitIndex = 0; bitIndex < bitCount; bitIndex++)
            {
                string binaryParameterName = GenerateFaceTrackingProcess.GetBinaryParameterName(parameterAnimation.Parameter, 1 << bitIndex);
                parameterConfigs.Add(CreateParameterConfig(binaryParameterName, ParameterSyncType.Bool, false, 0.0f));
            }

            if (parameterAnimation.Range != ParameterRangeKind.Signed)
            {
                return;
            }

            string negativeParameterName = GenerateFaceTrackingProcess.GetBinaryNegativeParameterName(parameterAnimation.Parameter);
            parameterConfigs.Add(CreateParameterConfig(negativeParameterName, ParameterSyncType.Bool, false, 0.0f));
        }
    }
}

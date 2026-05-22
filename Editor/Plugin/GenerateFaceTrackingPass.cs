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
            GameObject generatedObject = new(GeneratedObjectName);
            generatedObject.transform.SetParent(source.transform, false);

            AnimatorController controller = GenerateAnimatorController($"{GeneratedControllerName} ({source.gameObject.name})", source);
            ModularAvatarParameters parameters = generatedObject.AddComponent<ModularAvatarParameters>();
            parameters.parameters = new List<ParameterConfig>();
            ModularAvatarMergeAnimator mergeAnimator = generatedObject.AddComponent<ModularAvatarMergeAnimator>();
            mergeAnimator.animator = controller;
            mergeAnimator.deleteAttachedAnimator = true;
            mergeAnimator.pathMode = MergeAnimatorPathMode.Absolute;
            mergeAnimator.matchAvatarWriteDefaults = true;
            mergeAnimator.mergeAnimatorMode = MergeAnimatorMode.Append;
            mergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.FX;

            RegisterGeneratedObjects(context, controller);
            RegisterGeneratedObjects(context, generatedObject);
        }

        private static AnimatorController GenerateAnimatorController(string controllerName, USTLFaceTracking source)
        {
            return AnimatorControllerUtility.CreateAnimatorController(controllerName);

            // TODO: アニメーター生成処理の追加
        }

        private static void RegisterGeneratedObjects(BuildContext context, AnimatorController controller)
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

            void AddGeneratedObject(Object asset)
            {
                if (!asset || !visitedObjects.Add(asset))
                {
                    return;
                }

                if (asset == controller || context.IsTemporaryAsset(asset))
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
    }
}

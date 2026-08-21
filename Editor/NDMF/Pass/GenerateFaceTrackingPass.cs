using System.Collections.Generic;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace USTL.FaceTracking.Editor
{
    internal sealed class GenerateFaceTrackingPass : Pass<GenerateFaceTrackingPass>
    {
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

                if (!proceeded && component.gameObject.activeInHierarchy && component.enabled && component.faceMeshRenderer && component.faceMeshRenderer.sharedMesh)
                {
                    Generate(context, component);
                    proceeded = true;
                }

                Object.DestroyImmediate(component);
            }
        }

        private static void Generate(BuildContext context, USTLFaceTracking source)
        {
            FTBuildContext ftContext = new(context, source);
            HierarchyGenerator.Generate(ftContext);
            MeshGenerator.Generate(ftContext);
            AnimatorGenerator.Generate(ftContext);
            source.faceMeshRenderer.sharedMesh = ftContext.GeneratedMesh;

            List<ParameterConfig> parameterConfigs = new();
            foreach (AnimatorGenerator.ParameterAnimation parameterAnimation in ftContext.ParameterAnimations)
            {
                AddParameterConfigs(parameterAnimation, ref parameterConfigs);
            }

            ftContext.ModularAvatarParameters.parameters = parameterConfigs;
            ftContext.ModularAvatarMergeAnimator.animator = ftContext.AnimatorController;

            RegisterGeneratedObject(ftContext);
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

        private static void RegisterGeneratedObject(FTBuildContext ftContext)
        {
            RegisterGeneratedObject(ftContext.BuildContext, ftContext.GeneratedObject);
            RegisterGeneratedObject(ftContext.BuildContext, ftContext.GeneratedMesh);
            RegisterGeneratedObject(ftContext.BuildContext, ftContext.AnimatorController);
        }

        private static void RegisterGeneratedObject(BuildContext context, AnimatorController controller)
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

        private static void RegisterGeneratedObject(BuildContext context, Object generatedAsset)
        {
            if (!generatedAsset)
            {
                return;
            }

            using (SerializationScope scope = context.OpenSerializationScope())
            {
                scope.SaveAsset(generatedAsset);
            }

            using (new ObjectRegistryScope(context.ObjectRegistry))
            {
                ObjectRegistry.GetReference(generatedAsset);
            }

            EditorUtility.SetDirty(generatedAsset);
        }

        private static void RegisterGeneratedObject(BuildContext context, GameObject gameObject)
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


        private static void AddParameterConfigs(AnimatorGenerator.ParameterAnimation parameterAnimation, ref List<ParameterConfig> parameterConfigs)
        {
            bool syncFloatParameter = parameterAnimation.SyncMode == ParameterSyncMode.Float8;
            parameterConfigs.Add(CreateParameterConfig(parameterAnimation.ParameterName, ParameterSyncType.Float, !syncFloatParameter, parameterAnimation.DefaultValue));

            int bitCount = AnimatorGenerator.GetBinaryBitCount(parameterAnimation.SyncMode);
            if (bitCount <= 0)
            {
                return;
            }

            for (int bitIndex = 0; bitIndex < bitCount; bitIndex++)
            {
                string binaryParameterName = AnimatorGenerator.GetBinaryParameterName(parameterAnimation.Parameter, 1 << bitIndex);
                parameterConfigs.Add(CreateParameterConfig(binaryParameterName, ParameterSyncType.Bool, false, 0.0f));
            }

            if (parameterAnimation.Range != ParameterRangeKind.Signed)
            {
                return;
            }

            string negativeParameterName = AnimatorGenerator.GetBinaryNegativeParameterName(parameterAnimation.Parameter);
            parameterConfigs.Add(CreateParameterConfig(negativeParameterName, ParameterSyncType.Bool, false, 0.0f));
        }
    }
}

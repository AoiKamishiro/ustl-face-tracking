using System;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace USTL.FaceTracking.Editor
{
    /// <summary>
    ///     Creates AnimatorController assets without using Unity's public mutation helpers that register Undo operations.
    /// </summary>
    internal static class AnimatorControllerUtility
    {
        private const string DefaultBlendParameterName = "Blend";
        private const string DefaultBlendTreeName = "BlendTree";
        private const float DefaultTransitionDuration = 0.25f;
        private const float DefaultTransitionExitTime = 0.75f;

        private static readonly Vector3 FirstStatePosition = new(200f, 0f, 0f);
        private static readonly Vector3 NextStateOffset = new(35f, 65f, 0f);

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

        #region Controller/Layer/Parameter

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

        internal static AnimatorControllerLayer AddLayer(AnimatorController controller, string name)
        {
            ThrowIfNull(controller, nameof(controller));

            AnimatorStateMachine stateMachine = new()
            {
                name = controller.MakeUniqueLayerName(name),
                hideFlags = HideFlags.HideInHierarchy,
            };

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

        #endregion

        #region BlendTrees

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

            AddBlendTreeChild(parent, child, position, threshold);
            MarkDirty(child);
            return child;
        }

        #endregion

        #region States

        internal static AnimatorState AddMotion(AnimatorController controller, Motion motion)
        {
            return AddMotion(controller, motion, 0);
        }

        internal static AnimatorState AddMotion(AnimatorController controller, Motion motion, int layerIndex)
        {
            ThrowIfNull(controller, nameof(controller));
            ThrowIfNull(motion, nameof(motion));

            AnimatorStateMachine stateMachine = controller.layers[layerIndex].stateMachine;
            AnimatorState state = AddState(stateMachine, motion.name);
            state.motion = motion;
            MarkDirty(state);
            return state;
        }

        internal static AnimatorState CreateBlendTreeInController(AnimatorController controller, string name, out BlendTree tree)
        {
            return CreateBlendTreeInController(controller, name, out tree, 0);
        }

        internal static AnimatorState CreateBlendTreeInController(AnimatorController controller, string name, out BlendTree tree, int layerIndex)
        {
            ThrowIfNull(controller, nameof(controller));

            tree = new BlendTree
            {
                name = name,
                blendParameter = GetDefaultBlendTreeParameter(controller),
                blendParameterY = GetDefaultBlendTreeParameter(controller),
            };

            AnimatorStateMachine stateMachine = controller.layers[layerIndex].stateMachine;
            AnimatorState state = AddState(stateMachine, tree.name);
            state.motion = tree;

            MarkDirty(tree);
            MarkDirty(state);
            return state;

            string GetDefaultBlendTreeParameter(AnimatorController controllerInner)
            {
                AnimatorControllerParameter[] parameters = controllerInner.parameters ?? Array.Empty<AnimatorControllerParameter>();
                foreach (AnimatorControllerParameter parameter in parameters)
                {
                    if (parameter.type == AnimatorControllerParameterType.Float)
                    {
                        return parameter.name;
                    }
                }

                AddParameter(controllerInner, DefaultBlendParameterName, AnimatorControllerParameterType.Float);
                return DefaultBlendParameterName;
            }
        }

        internal static AnimatorState AddState(AnimatorStateMachine stateMachine, string name)
        {
            ThrowIfNull(stateMachine, nameof(stateMachine));

            ChildAnimatorState[] states = stateMachine.states;
            Vector3 position = states != null && states.Length > 0 ? states[states.Length - 1].position + NextStateOffset : FirstStatePosition;

            return AddState(stateMachine, name, position);
        }

        internal static AnimatorState AddState(AnimatorStateMachine stateMachine, string name, Vector3 position)
        {
            ThrowIfNull(stateMachine, nameof(stateMachine));

            AnimatorState state = new()
            {
                hideFlags = HideFlags.HideInHierarchy,
                name = stateMachine.MakeUniqueStateName(name),
            };

            AddState(stateMachine, state, position);
            MarkDirty(state);
            return state;
        }

        internal static bool AddState(AnimatorStateMachine stateMachine, AnimatorState state, Vector3 position)
        {
            ThrowIfNull(stateMachine, nameof(stateMachine));
            ThrowIfNull(state, nameof(state));

            ChildAnimatorState[] states = stateMachine.states ?? Array.Empty<ChildAnimatorState>();
            if (Array.Exists(states, childState => childState.state == state))
            {
                Debug.LogWarning($"State '{state.name}' already exists in state machine '{stateMachine.name}', discarding new state.");
                return false;
            }

            ChildAnimatorState child = new()
            {
                state = state,
                position = position,
            };

            stateMachine.states = AddToArray(states, child);
            MarkDirty(stateMachine);
            return true;
        }

        internal static AnimatorStateMachine AddStateMachine(AnimatorStateMachine parent, string name)
        {
            return AddStateMachine(parent, name, Vector3.zero);
        }

        internal static AnimatorStateMachine AddStateMachine(AnimatorStateMachine parent, string name, Vector3 position)
        {
            ThrowIfNull(parent, nameof(parent));

            AnimatorStateMachine stateMachine = new()
            {
                hideFlags = HideFlags.HideInHierarchy,
                name = parent.MakeUniqueStateMachineName(name),
            };

            AddStateMachine(parent, stateMachine, position);
            MarkDirty(stateMachine);
            return stateMachine;
        }

        internal static bool AddStateMachine(AnimatorStateMachine parent, AnimatorStateMachine stateMachine, Vector3 position)
        {
            ThrowIfNull(parent, nameof(parent));
            ThrowIfNull(stateMachine, nameof(stateMachine));

            ChildAnimatorStateMachine[] stateMachines = parent.stateMachines ?? Array.Empty<ChildAnimatorStateMachine>();
            if (Array.Exists(stateMachines, childStateMachine => childStateMachine.stateMachine == stateMachine))
            {
                Debug.LogWarning($"Sub state machine '{stateMachine.name}' already exists in state machine '{parent.name}', discarding new state machine.");
                return false;
            }

            ChildAnimatorStateMachine child = new()
            {
                stateMachine = stateMachine,
                position = position,
            };

            parent.stateMachines = AddToArray(stateMachines, child);
            MarkDirty(parent);
            return true;
        }

        #endregion

        #region Transitions

        internal static AnimatorStateTransition AddTransition(AnimatorState sourceState, AnimatorState destinationState)
        {
            return AddTransition(sourceState, destinationState, false);
        }

        internal static AnimatorStateTransition AddTransition(AnimatorState sourceState, AnimatorState destinationState, bool defaultExitTime)
        {
            ThrowIfNull(destinationState, nameof(destinationState));

            AnimatorStateTransition transition = CreateStateTransition(sourceState, defaultExitTime);
            transition.destinationState = destinationState;
            AddTransition(sourceState, transition);
            return transition;
        }

        internal static AnimatorStateTransition AddTransition(AnimatorState sourceState, AnimatorStateMachine destinationStateMachine)
        {
            return AddTransition(sourceState, destinationStateMachine, false);
        }

        internal static AnimatorStateTransition AddTransition(AnimatorState sourceState, AnimatorStateMachine destinationStateMachine, bool defaultExitTime)
        {
            ThrowIfNull(destinationStateMachine, nameof(destinationStateMachine));

            AnimatorStateTransition transition = CreateStateTransition(sourceState, defaultExitTime);
            transition.destinationStateMachine = destinationStateMachine;
            AddTransition(sourceState, transition);
            return transition;
        }

        internal static AnimatorStateTransition AddExitTransition(AnimatorState sourceState)
        {
            return AddExitTransition(sourceState, false);
        }

        internal static AnimatorStateTransition AddExitTransition(AnimatorState sourceState, bool defaultExitTime)
        {
            AnimatorStateTransition transition = CreateStateTransition(sourceState, defaultExitTime);
            transition.isExit = true;
            AddTransition(sourceState, transition);
            return transition;
        }

        internal static void AddTransition(AnimatorState sourceState, AnimatorStateTransition transition)
        {
            ThrowIfNull(sourceState, nameof(sourceState));
            ThrowIfNull(transition, nameof(transition));

            sourceState.transitions = AddToArray(sourceState.transitions, transition);
            MarkDirty(sourceState);
        }

        internal static AnimatorStateTransition AddAnyStateTransition(AnimatorStateMachine stateMachine, AnimatorState destinationState)
        {
            ThrowIfNull(destinationState, nameof(destinationState));

            AnimatorStateTransition transition = AddAnyStateTransition(stateMachine);
            transition.destinationState = destinationState;
            MarkDirty(transition);
            return transition;
        }

        internal static AnimatorStateTransition AddAnyStateTransition(AnimatorStateMachine stateMachine, AnimatorStateMachine destinationStateMachine)
        {
            ThrowIfNull(destinationStateMachine, nameof(destinationStateMachine));

            AnimatorStateTransition transition = AddAnyStateTransition(stateMachine);
            transition.destinationStateMachine = destinationStateMachine;
            MarkDirty(transition);
            return transition;
        }

        internal static AnimatorTransition AddEntryTransition(AnimatorStateMachine stateMachine, AnimatorState destinationState)
        {
            ThrowIfNull(destinationState, nameof(destinationState));

            AnimatorTransition transition = AddEntryTransition(stateMachine);
            transition.destinationState = destinationState;
            MarkDirty(transition);
            return transition;
        }

        internal static AnimatorTransition AddEntryTransition(AnimatorStateMachine stateMachine, AnimatorStateMachine destinationStateMachine)
        {
            ThrowIfNull(destinationStateMachine, nameof(destinationStateMachine));

            AnimatorTransition transition = AddEntryTransition(stateMachine);
            transition.destinationStateMachine = destinationStateMachine;
            MarkDirty(transition);
            return transition;
        }

        internal static AnimatorTransition AddStateMachineTransition(AnimatorStateMachine stateMachine, AnimatorStateMachine sourceStateMachine)
        {
            AnimatorTransition transition = new();
            AddStateMachineTransition(stateMachine, sourceStateMachine, transition);
            return transition;
        }

        internal static AnimatorTransition AddStateMachineTransition(AnimatorStateMachine stateMachine, AnimatorStateMachine sourceStateMachine, AnimatorState destinationState)
        {
            ThrowIfNull(destinationState, nameof(destinationState));

            AnimatorTransition transition = new()
            {
                destinationState = destinationState,
            };

            AddStateMachineTransition(stateMachine, sourceStateMachine, transition);
            return transition;
        }

        internal static AnimatorTransition AddStateMachineTransition(AnimatorStateMachine stateMachine, AnimatorStateMachine sourceStateMachine, AnimatorStateMachine destinationStateMachine)
        {
            ThrowIfNull(destinationStateMachine, nameof(destinationStateMachine));

            AnimatorTransition transition = new()
            {
                destinationStateMachine = destinationStateMachine,
            };

            AddStateMachineTransition(stateMachine, sourceStateMachine, transition);
            return transition;
        }

        internal static AnimatorTransition AddStateMachineExitTransition(AnimatorStateMachine stateMachine, AnimatorStateMachine sourceStateMachine)
        {
            AnimatorTransition transition = new()
            {
                isExit = true,
            };

            AddStateMachineTransition(stateMachine, sourceStateMachine, transition);
            return transition;
        }

        internal static void AddCondition(AnimatorTransitionBase transition, AnimatorConditionMode mode, float threshold, string parameter)
        {
            ThrowIfNull(transition, nameof(transition));

            AnimatorCondition condition = new()
            {
                mode = mode,
                parameter = parameter,
                threshold = threshold,
            };

            transition.conditions = AddToArray(transition.conditions, condition);
            MarkDirty(transition);
        }

        private static AnimatorStateTransition AddAnyStateTransition(AnimatorStateMachine stateMachine)
        {
            ThrowIfNull(stateMachine, nameof(stateMachine));

            AnimatorStateTransition transition = new()
            {
                hasExitTime = false,
                hasFixedDuration = true,
                duration = DefaultTransitionDuration,
                exitTime = DefaultTransitionExitTime,
                hideFlags = HideFlags.HideInHierarchy,
            };

            stateMachine.anyStateTransitions = AddToArray(stateMachine.anyStateTransitions, transition);

            MarkDirty(stateMachine);
            MarkDirty(transition);
            return transition;
        }

        private static AnimatorTransition AddEntryTransition(AnimatorStateMachine stateMachine)
        {
            ThrowIfNull(stateMachine, nameof(stateMachine));

            AnimatorTransition transition = new()
            {
                hideFlags = HideFlags.HideInHierarchy,
            };

            stateMachine.entryTransitions = AddToArray(stateMachine.entryTransitions, transition);

            MarkDirty(stateMachine);
            MarkDirty(transition);
            return transition;
        }

        private static void AddStateMachineTransition(AnimatorStateMachine stateMachine, AnimatorStateMachine sourceStateMachine, AnimatorTransition transition)
        {
            ThrowIfNull(stateMachine, nameof(stateMachine));
            ThrowIfNull(sourceStateMachine, nameof(sourceStateMachine));
            ThrowIfNull(transition, nameof(transition));

            transition.hideFlags = HideFlags.HideInHierarchy;

            AnimatorTransition[] transitions = stateMachine.GetStateMachineTransitions(sourceStateMachine);
            stateMachine.SetStateMachineTransitions(sourceStateMachine, AddToArray(transitions, transition));

            MarkDirty(stateMachine);
            MarkDirty(transition);
        }

        private static AnimatorStateTransition CreateStateTransition(AnimatorState sourceState, bool defaultExitTime)
        {
            ThrowIfNull(sourceState, nameof(sourceState));

            AnimatorStateTransition transition = new()
            {
                hasExitTime = false,
                hasFixedDuration = true,
                hideFlags = HideFlags.HideInHierarchy,
            };

            if (defaultExitTime)
            {
                SetDefaultExitTime(sourceState, transition);
            }

            MarkDirty(transition);
            return transition;
        }

        private static void SetDefaultExitTime(AnimatorState sourceState, AnimatorStateTransition transition)
        {
            transition.hasExitTime = true;

            if (sourceState.motion && sourceState.motion.averageDuration > 0f)
            {
                float durationNormalized = DefaultTransitionDuration / sourceState.motion.averageDuration;
                transition.duration = DefaultTransitionDuration;
                transition.exitTime = 1f - durationNormalized;
                return;
            }

            transition.duration = DefaultTransitionDuration;
            transition.exitTime = DefaultTransitionExitTime;
        }

        #endregion
    }
}

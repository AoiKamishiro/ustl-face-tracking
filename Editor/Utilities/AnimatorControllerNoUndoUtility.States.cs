using System;
using UnityEditor.Animations;
using UnityEngine;

namespace USTL.FaceTracking.Editor
{
    internal static partial class AnimatorControllerNoUndoUtility
    {
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

            AddObjectToAssetIfPossible(tree, controller);

            AnimatorStateMachine stateMachine = controller.layers[layerIndex].stateMachine;
            AnimatorState state = AddState(stateMachine, tree.name);
            state.motion = tree;

            MarkDirty(tree);
            MarkDirty(state);
            return state;
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

            AddObjectToAssetIfPossible(state, stateMachine);
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
            AddObjectToAssetIfPossible(stateMachine, parent);
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
    }
}

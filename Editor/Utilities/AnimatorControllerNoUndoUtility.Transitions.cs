using UnityEditor.Animations;
using UnityEngine;

namespace USTL.FaceTracking.Editor
{
    internal static partial class AnimatorControllerNoUndoUtility
    {
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

            AddObjectToAssetIfPossible(transition, stateMachine);
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

            AddObjectToAssetIfPossible(transition, stateMachine);
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
            AddObjectToAssetIfPossible(transition, stateMachine);

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

            AddObjectToAssetIfPossible(transition, sourceState);

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
    }
}

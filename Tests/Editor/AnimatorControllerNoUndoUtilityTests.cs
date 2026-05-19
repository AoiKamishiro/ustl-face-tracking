using System.Collections.Generic;
using System.Globalization;
using System.Text;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace USTL.FaceTracking.Editor.Tests
{
    public sealed class AnimatorControllerNoUndoUtilityTests
    {
        private const string TempRoot = "Assets/__USTLFaceTrackingEditorTests__";

        private readonly List<Object> transientObjects = new();

        [SetUp]
        public void SetUp()
        {
            AssetDatabase.DeleteAsset(TempRoot);
            AssetDatabase.CreateFolder("Assets", "__USTLFaceTrackingEditorTests__");
        }

        [TearDown]
        public void TearDown()
        {
            foreach (Object obj in transientObjects)
            {
                if (obj)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            transientObjects.Clear();
            AssetDatabase.DeleteAsset(TempRoot);
        }

        [Test]
        public void CreateAnimatorController_InitializesEmptyController()
        {
            AnimatorController controller = Track(AnimatorControllerNoUndoUtility.CreateAnimatorController("Controller"));

            Assert.That(controller.name, Is.EqualTo("Controller"));
            Assert.That(controller.layers, Is.Empty);
            Assert.That(controller.parameters, Is.Empty);
        }

        [Test]
        public void CreateAnimatorControllerAtPath_CreatesAssetWithBaseLayerSubAsset()
        {
            string path = $"{TempRoot}/Generated.controller";

            AnimatorController controller = AnimatorControllerNoUndoUtility.CreateAnimatorControllerAtPath(path);
            AnimatorController loaded = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);

            Assert.That(loaded, Is.SameAs(controller));
            Assert.That(controller.layers, Has.Length.EqualTo(1));

            AnimatorControllerLayer layer = controller.layers[0];
            Assert.That(layer.name, Is.EqualTo("Base Layer"));
            Assert.That(layer.stateMachine, Is.Not.Null);
            Assert.That(layer.stateMachine.name, Is.EqualTo("Base Layer"));
            Assert.That(layer.stateMachine.hideFlags, Is.EqualTo(HideFlags.HideInHierarchy));
            Assert.That(AssetDatabase.GetAssetPath(layer.stateMachine), Is.EqualTo(path));
            CollectionAssert.Contains(AssetDatabase.LoadAllAssetsAtPath(path), layer.stateMachine);
        }

        [Test]
        public void AddLayerAndParameter_AppendsUniqueEntries()
        {
            AnimatorController controller = Track(AnimatorControllerNoUndoUtility.CreateAnimatorController("Controller"));

            AnimatorControllerLayer firstLayer = AnimatorControllerNoUndoUtility.AddLayer(controller, "Layer");
            AnimatorControllerLayer secondLayer = AnimatorControllerNoUndoUtility.AddLayer(controller, "Layer");
            Track(firstLayer.stateMachine);
            Track(secondLayer.stateMachine);

            AnimatorControllerParameter firstParameter = AnimatorControllerNoUndoUtility.AddParameter(controller, "Blend", AnimatorControllerParameterType.Float);
            AnimatorControllerParameter secondParameter = AnimatorControllerNoUndoUtility.AddParameter(controller, "Blend", AnimatorControllerParameterType.Int);

            Assert.That(controller.layers, Has.Length.EqualTo(2));
            Assert.That(controller.layers[0].name, Is.EqualTo(firstLayer.name));
            Assert.That(controller.layers[0].stateMachine, Is.SameAs(firstLayer.stateMachine));
            Assert.That(controller.layers[1].name, Is.EqualTo(secondLayer.name));
            Assert.That(controller.layers[1].stateMachine, Is.SameAs(secondLayer.stateMachine));
            Assert.That(firstLayer.name, Is.EqualTo("Layer"));
            Assert.That(secondLayer.name, Is.Not.EqualTo(firstLayer.name));
            Assert.That(secondLayer.stateMachine.name, Is.EqualTo(secondLayer.name));

            Assert.That(controller.parameters, Has.Length.EqualTo(2));
            Assert.That(controller.parameters[0].name, Is.EqualTo(firstParameter.name));
            Assert.That(controller.parameters[0].type, Is.EqualTo(firstParameter.type));
            Assert.That(controller.parameters[1].name, Is.EqualTo(secondParameter.name));
            Assert.That(controller.parameters[1].type, Is.EqualTo(secondParameter.type));
            Assert.That(firstParameter.name, Is.EqualTo("Blend"));
            Assert.That(secondParameter.name, Is.Not.EqualTo(firstParameter.name));
            Assert.That(firstParameter.type, Is.EqualTo(AnimatorControllerParameterType.Float));
            Assert.That(secondParameter.type, Is.EqualTo(AnimatorControllerParameterType.Int));
        }

        [Test]
        public void AddMotion_AddsStatesWithMotionAndIncrementalPositions()
        {
            AnimatorController controller = CreateAssetController("Motion.controller");
            AnimationClip firstClip = Track(new AnimationClip { name = "Clip", });
            AnimationClip secondClip = Track(new AnimationClip { name = "Clip", });

            AnimatorState firstState = AnimatorControllerNoUndoUtility.AddMotion(controller, firstClip);
            AnimatorState secondState = AnimatorControllerNoUndoUtility.AddMotion(controller, secondClip);

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            Assert.That(stateMachine.states, Has.Length.EqualTo(2));
            Assert.That(stateMachine.states[0].state, Is.SameAs(firstState));
            Assert.That(stateMachine.states[0].position, Is.EqualTo(new Vector3(200f, 0f, 0f)));
            Assert.That(stateMachine.states[1].state, Is.SameAs(secondState));
            Assert.That(stateMachine.states[1].position, Is.EqualTo(new Vector3(235f, 65f, 0f)));

            Assert.That(firstState.motion, Is.SameAs(firstClip));
            Assert.That(secondState.motion, Is.SameAs(secondClip));
            Assert.That(firstState.name, Is.EqualTo("Clip"));
            Assert.That(secondState.name, Is.Not.EqualTo(firstState.name));
            Assert.That(AssetDatabase.GetAssetPath(firstState), Is.EqualTo(AssetDatabase.GetAssetPath(controller)));
            Assert.That(AssetDatabase.GetAssetPath(secondState), Is.EqualTo(AssetDatabase.GetAssetPath(controller)));
        }

        [Test]
        public void CreateBlendTreeInController_CreatesStateTreeAndDefaultFloatParameter()
        {
            AnimatorController controller = CreateAssetController("BlendTree.controller");

            AnimatorState state = AnimatorControllerNoUndoUtility.CreateBlendTreeInController(controller, "Blend Tree", out BlendTree tree);

            Assert.That(controller.parameters, Has.Length.EqualTo(1));
            Assert.That(controller.parameters[0].name, Is.EqualTo("Blend"));
            Assert.That(controller.parameters[0].type, Is.EqualTo(AnimatorControllerParameterType.Float));

            Assert.That(tree.name, Is.EqualTo("Blend Tree"));
            Assert.That(tree.blendParameter, Is.EqualTo("Blend"));
            Assert.That(tree.blendParameterY, Is.EqualTo("Blend"));
            Assert.That(tree.hideFlags, Is.EqualTo(HideFlags.None));
            Assert.That(state.motion, Is.SameAs(tree));
            Assert.That(controller.layers[0].stateMachine.states[0].state, Is.SameAs(state));
            Assert.That(AssetDatabase.GetAssetPath(tree), Is.EqualTo(AssetDatabase.GetAssetPath(controller)));
        }

        [Test]
        public void AddTransitions_AppendsTargetsDefaultsAndConditions()
        {
            AnimatorState sourceState = Track(new AnimatorState { name = "Source", });
            AnimatorState destinationState = Track(new AnimatorState { name = "Destination", });

            AnimatorStateTransition transition = Track(AnimatorControllerNoUndoUtility.AddTransition(sourceState, destinationState, true));
            AnimatorControllerNoUndoUtility.AddCondition(transition, AnimatorConditionMode.Greater, 0.5f, "Blend");

            Assert.That(sourceState.transitions, Has.Length.EqualTo(1));
            Assert.That(sourceState.transitions[0], Is.SameAs(transition));
            Assert.That(transition.destinationState, Is.SameAs(destinationState));
            Assert.That(transition.hasExitTime, Is.True);
            Assert.That(transition.hasFixedDuration, Is.True);
            Assert.That(transition.duration, Is.EqualTo(0.25f).Within(0.0001f));
            Assert.That(transition.exitTime, Is.EqualTo(0.75f).Within(0.0001f));
            Assert.That(transition.hideFlags, Is.EqualTo(HideFlags.HideInHierarchy));

            Assert.That(transition.conditions, Has.Length.EqualTo(1));
            Assert.That(transition.conditions[0].mode, Is.EqualTo(AnimatorConditionMode.Greater));
            Assert.That(transition.conditions[0].threshold, Is.EqualTo(0.5f));
            Assert.That(transition.conditions[0].parameter, Is.EqualTo("Blend"));
        }

        [Test]
        public void AddStateMachineTransitionsAndBlendTreeChildren_AppendsConfiguredEntries()
        {
            AnimatorController controller = CreateAssetController("StateMachine.controller");
            AnimatorStateMachine root = controller.layers[0].stateMachine;
            AnimatorState state = AnimatorControllerNoUndoUtility.AddState(root, "State");
            AnimatorStateMachine childStateMachine = AnimatorControllerNoUndoUtility.AddStateMachine(root, "Child", new Vector3(10f, 20f, 0f));
            BlendTree tree = Track(new BlendTree { name = "Parent Tree", useAutomaticThresholds = false, });
            AnimationClip clip = Track(new AnimationClip { name = "Clip", });

            AnimatorStateTransition anyStateTransition = AnimatorControllerNoUndoUtility.AddAnyStateTransition(root, state);
            AnimatorTransition entryTransition = AnimatorControllerNoUndoUtility.AddEntryTransition(root, childStateMachine);
            AnimatorTransition stateMachineTransition = AnimatorControllerNoUndoUtility.AddStateMachineExitTransition(root, childStateMachine);
            AnimatorControllerNoUndoUtility.AddBlendTreeChild(tree, clip, new Vector2(1f, 2f), 0.5f);
            BlendTree childTree = Track(AnimatorControllerNoUndoUtility.CreateBlendTreeChild(tree, new Vector2(3f, 4f), 0.75f));

            Assert.That(root.stateMachines, Has.Length.EqualTo(1));
            Assert.That(root.stateMachines[0].stateMachine, Is.SameAs(childStateMachine));
            Assert.That(root.stateMachines[0].position, Is.EqualTo(new Vector3(10f, 20f, 0f)));
            Assert.That(AssetDatabase.GetAssetPath(childStateMachine), Is.EqualTo(AssetDatabase.GetAssetPath(controller)));

            Assert.That(root.anyStateTransitions, Has.Length.EqualTo(1));
            Assert.That(root.anyStateTransitions[0], Is.SameAs(anyStateTransition));
            Assert.That(anyStateTransition.destinationState, Is.SameAs(state));
            Assert.That(anyStateTransition.duration, Is.EqualTo(0.25f).Within(0.0001f));
            Assert.That(anyStateTransition.exitTime, Is.EqualTo(0.75f).Within(0.0001f));

            Assert.That(root.entryTransitions, Has.Length.EqualTo(1));
            Assert.That(root.entryTransitions[0], Is.SameAs(entryTransition));
            Assert.That(entryTransition.destinationStateMachine, Is.SameAs(childStateMachine));

            AnimatorTransition[] stateMachineTransitions = root.GetStateMachineTransitions(childStateMachine);
            Assert.That(stateMachineTransitions, Has.Length.EqualTo(1));
            Assert.That(stateMachineTransitions[0], Is.SameAs(stateMachineTransition));
            Assert.That(stateMachineTransition.isExit, Is.True);

            Assert.That(tree.children, Has.Length.EqualTo(2));
            Assert.That(tree.children[0].motion, Is.SameAs(clip));
            Assert.That(tree.children[0].position, Is.EqualTo(new Vector2(1f, 2f)));
            Assert.That(tree.children[0].threshold, Is.EqualTo(0.5f));
            Assert.That(tree.children[0].timeScale, Is.EqualTo(1f));
            Assert.That(tree.children[0].directBlendParameter, Is.EqualTo("Blend"));

            Assert.That(tree.children[1].motion, Is.SameAs(childTree));
            Assert.That(tree.children[1].position, Is.EqualTo(new Vector2(3f, 4f)));
            Assert.That(tree.children[1].threshold, Is.EqualTo(0.75f));
            Assert.That(childTree.name, Is.EqualTo("BlendTree"));
            Assert.That(childTree.hideFlags, Is.EqualTo(HideFlags.HideInHierarchy));
        }

        [Test]
        public void BuildControllerGraph_MatchesUnityPublicApiExceptUndo()
        {
            AssetDatabase.CreateFolder(TempRoot, "NoUndo");
            AssetDatabase.CreateFolder(TempRoot, "UnityApi");

            string noUndoPath = $"{TempRoot}/NoUndo/Generated.controller";
            string unityApiPath = $"{TempRoot}/UnityApi/Generated.controller";
            AnimationClip mainClip = Track(new AnimationClip { name = "Main Clip", });
            AnimationClip blendClip = Track(new AnimationClip { name = "Blend Clip", });

            AnimatorController noUndoController = BuildControllerGraphWithNoUndo(noUndoPath, mainClip, blendClip);
            AnimatorController unityApiController = BuildControllerGraphWithUnityApi(unityApiPath, mainClip, blendClip);

            Assert.That(DumpController(noUndoController), Is.EqualTo(DumpController(unityApiController)));
            Assert.That(DumpAssetObjects(noUndoPath), Is.EqualTo(DumpAssetObjects(unityApiPath)));
        }

        private AnimatorController CreateAssetController(string fileName)
        {
            return AnimatorControllerNoUndoUtility.CreateAnimatorControllerAtPath($"{TempRoot}/{fileName}");
        }

        private T Track<T>(T obj) where T : Object
        {
            transientObjects.Add(obj);
            return obj;
        }

        private static AnimatorController BuildControllerGraphWithNoUndo(string path, AnimationClip mainClip, AnimationClip blendClip)
        {
            AnimatorController controller = AnimatorControllerNoUndoUtility.CreateAnimatorControllerAtPath(path);
            AnimatorControllerNoUndoUtility.CreateBlendTreeInController(controller, "Blend Tree", out BlendTree tree);
            AnimatorControllerNoUndoUtility.AddBlendTreeChild(tree, blendClip, 0.25f);
            AnimatorControllerNoUndoUtility.CreateBlendTreeChild(tree, 0.75f);
            AnimatorControllerNoUndoUtility.AddParameter(controller, "Gate", AnimatorControllerParameterType.Bool);
            AnimatorControllerNoUndoUtility.AddLayer(controller, "Extra");

            AnimatorStateMachine root = controller.layers[0].stateMachine;
            AnimatorState mainState = AnimatorControllerNoUndoUtility.AddMotion(controller, mainClip);
            AnimatorState manualState = AnimatorControllerNoUndoUtility.AddState(root, "Manual", new Vector3(10f, 20f, 0f));
            AnimatorStateMachine childStateMachine = AnimatorControllerNoUndoUtility.AddStateMachine(root, "Child", new Vector3(30f, 40f, 0f));

            AnimatorStateTransition transition = AnimatorControllerNoUndoUtility.AddTransition(mainState, manualState, true);
            AnimatorControllerNoUndoUtility.AddCondition(transition, AnimatorConditionMode.Greater, 0.5f, "Blend");
            AnimatorControllerNoUndoUtility.AddExitTransition(manualState);
            AnimatorControllerNoUndoUtility.AddAnyStateTransition(root, mainState);
            AnimatorControllerNoUndoUtility.AddEntryTransition(root, childStateMachine);
            AnimatorControllerNoUndoUtility.AddStateMachineExitTransition(root, childStateMachine);

            return controller;
        }

        private static AnimatorController BuildControllerGraphWithUnityApi(string path, AnimationClip mainClip, AnimationClip blendClip)
        {
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.CreateBlendTreeInController("Blend Tree", out BlendTree tree);
            tree.AddChild(blendClip, 0.25f);
            tree.CreateBlendTreeChild(0.75f);
            controller.AddParameter("Gate", AnimatorControllerParameterType.Bool);
            controller.AddLayer("Extra");

            AnimatorStateMachine root = controller.layers[0].stateMachine;
            AnimatorState mainState = controller.AddMotion(mainClip);
            AnimatorState manualState = root.AddState("Manual", new Vector3(10f, 20f, 0f));
            AnimatorStateMachine childStateMachine = root.AddStateMachine("Child", new Vector3(30f, 40f, 0f));

            AnimatorStateTransition transition = mainState.AddTransition(manualState, true);
            transition.AddCondition(AnimatorConditionMode.Greater, 0.5f, "Blend");
            manualState.AddExitTransition();
            root.AddAnyStateTransition(mainState);
            root.AddEntryTransition(childStateMachine);
            root.AddStateMachineExitTransition(childStateMachine);

            return controller;
        }

        private static string DumpController(AnimatorController controller)
        {
            StringBuilder builder = new();
            builder.AppendLine($"controller:{controller.name}");
            DumpParameters(builder, controller.parameters);

            AnimatorControllerLayer[] layers = controller.layers;
            builder.AppendLine($"layers:{layers.Length}");
            for (int i = 0; i < layers.Length; i++)
            {
                DumpLayer(builder, layers[i], i);
            }

            return builder.ToString();
        }

        private static string DumpAssetObjects(string path)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            List<string> lines = new();
            foreach (Object asset in assets)
            {
                if (asset is AnimatorController)
                {
                    continue;
                }

                lines.Add($"{asset.GetType().FullName}:{asset.name}:{asset.hideFlags}");
            }

            lines.Sort();

            StringBuilder builder = new();
            foreach (string line in lines)
            {
                builder.AppendLine(line);
            }

            return builder.ToString();
        }

        private static void DumpParameters(StringBuilder builder, AnimatorControllerParameter[] parameters)
        {
            builder.AppendLine($"parameters:{parameters.Length}");
            foreach (AnimatorControllerParameter parameter in parameters)
            {
                builder.AppendLine($"parameter:{parameter.name}:{parameter.type}:{parameter.defaultBool}:{Float(parameter.defaultFloat)}:{parameter.defaultInt}");
            }
        }

        private static void DumpLayer(StringBuilder builder, AnimatorControllerLayer layer, int index)
        {
            builder.AppendLine("layer:" + $"{index}:{layer.name}:{Float(layer.defaultWeight)}:{layer.blendingMode}:{layer.iKPass}:" + $"{layer.syncedLayerIndex}:{layer.syncedLayerAffectsTiming}:{ObjectName(layer.avatarMask)}");
            DumpStateMachine(builder, layer.stateMachine, "  ");
        }

        private static void DumpStateMachine(StringBuilder builder, AnimatorStateMachine stateMachine, string indent)
        {
            builder.AppendLine($"{indent}stateMachine:{stateMachine.name}:{stateMachine.hideFlags}:{ObjectName(stateMachine.defaultState)}");

            ChildAnimatorState[] states = stateMachine.states;
            builder.AppendLine($"{indent}states:{states.Length}");
            foreach (ChildAnimatorState childState in states)
            {
                builder.AppendLine($"{indent}statePosition:{childState.state.name}:{Vector(childState.position)}");
                DumpState(builder, childState.state, indent + "  ");
            }

            ChildAnimatorStateMachine[] stateMachines = stateMachine.stateMachines;
            builder.AppendLine($"{indent}stateMachines:{stateMachines.Length}");
            foreach (ChildAnimatorStateMachine childStateMachine in stateMachines)
            {
                builder.AppendLine($"{indent}childStateMachine:{childStateMachine.stateMachine.name}:{Vector(childStateMachine.position)}");
                AnimatorTransition[] transitions = stateMachine.GetStateMachineTransitions(childStateMachine.stateMachine);
                DumpTransitions(builder, transitions, indent + "  ", $"stateMachineTransitions:{childStateMachine.stateMachine.name}");
            }

            DumpTransitions(builder, stateMachine.anyStateTransitions, indent, "anyStateTransitions");
            DumpTransitions(builder, stateMachine.entryTransitions, indent, "entryTransitions");
        }

        private static void DumpState(StringBuilder builder, AnimatorState state, string indent)
        {
            builder.AppendLine($"{indent}state:{state.name}:{state.hideFlags}:{ObjectName(state.motion)}:{state.writeDefaultValues}:" + $"{Float(state.speed)}:{Float(state.cycleOffset)}:{state.mirror}:{state.iKOnFeet}:{state.tag}");
            DumpMotion(builder, state.motion, indent + "  ");
            DumpTransitions(builder, state.transitions, indent, "stateTransitions");
        }

        private static void DumpMotion(StringBuilder builder, Motion motion, string indent)
        {
            if (!motion)
            {
                builder.AppendLine($"{indent}motion:null");
                return;
            }

            if (motion is not BlendTree tree)
            {
                builder.AppendLine($"{indent}motion:{motion.GetType().Name}:{motion.name}");
                return;
            }

            builder.AppendLine($"{indent}blendTree:{tree.name}:{tree.hideFlags}:{tree.blendType}:{tree.blendParameter}:" + $"{tree.blendParameterY}:{Float(tree.minThreshold)}:{Float(tree.maxThreshold)}:{tree.useAutomaticThresholds}");

            ChildMotion[] children = tree.children;
            builder.AppendLine($"{indent}blendTreeChildren:{children.Length}");
            for (int i = 0; i < children.Length; i++)
            {
                ChildMotion child = children[i];
                builder.AppendLine($"{indent}child:{i}:{Vector(child.position)}:{Float(child.threshold)}:{Float(child.timeScale)}:" + $"{Float(child.cycleOffset)}:{child.mirror}:{child.directBlendParameter}:{ObjectName(child.motion)}");
                DumpMotion(builder, child.motion, indent + "  ");
            }
        }

        private static void DumpTransitions(StringBuilder builder, AnimatorTransitionBase[] transitions, string indent, string label)
        {
            builder.AppendLine($"{indent}{label}:{transitions.Length}");
            foreach (AnimatorTransitionBase transition in transitions)
            {
                builder.AppendLine($"{indent}transition:{transition.GetType().Name}:{transition.name}:{transition.hideFlags}:" + $"{ObjectName(transition.destinationState)}:{ObjectName(transition.destinationStateMachine)}:" + $"{transition.isExit}:{transition.mute}:{transition.solo}");

                if (transition is AnimatorStateTransition stateTransition)
                {
                    builder.AppendLine($"{indent}stateTransition:{stateTransition.hasExitTime}:{stateTransition.hasFixedDuration}:" + $"{Float(stateTransition.duration)}:{Float(stateTransition.exitTime)}:{Float(stateTransition.offset)}:" + $"{stateTransition.interruptionSource}:{stateTransition.orderedInterruption}:" + $"{stateTransition.canTransitionToSelf}");
                }

                AnimatorCondition[] conditions = transition.conditions;
                builder.AppendLine($"{indent}conditions:{conditions.Length}");
                foreach (AnimatorCondition condition in conditions)
                {
                    builder.AppendLine($"{indent}condition:{condition.mode}:{Float(condition.threshold)}:{condition.parameter}");
                }
            }
        }

        private static string ObjectName(Object obj)
        {
            return obj ? $"{obj.GetType().Name}:{obj.name}" : "null";
        }

        private static string Vector(Vector2 value)
        {
            return $"{Float(value.x)},{Float(value.y)}";
        }

        private static string Vector(Vector3 value)
        {
            return $"{Float(value.x)},{Float(value.y)},{Float(value.z)}";
        }

        private static string Float(float value)
        {
            return value.ToString("R", CultureInfo.InvariantCulture);
        }
    }
}

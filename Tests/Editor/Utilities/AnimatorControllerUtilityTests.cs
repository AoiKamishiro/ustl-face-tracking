using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace USTL.FaceTracking.Editor.Tests
{
    public sealed class AnimatorControllerUtilityTests
    {
        public enum BuilderMode
        {
            Utility,
            UnityApi,
        }

        private const float FloatTolerance = 0.00001f;

        private readonly HashSet<Object> _objects = new();

        [TearDown]
        public void TearDown()
        {
            foreach (Object obj in _objects)
            {
                if (obj)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _objects.Clear();
        }

        [Test]
        public void UtilityGeneratedAnimator_MatchesUnityApiGeneratedAnimator()
        {
            AnimatorController utilityController = BuildController(BuilderMode.Utility);
            AnimatorController unityApiController = BuildController(BuilderMode.UnityApi);
            CollectObjects(utilityController);
            CollectObjects(unityApiController);

            AssertControllersEquivalent(utilityController, unityApiController);
        }

        [TestCaseSource(nameof(ApiEquivalenceTestCases))]
        public void InternalApi_MatchesUnityApiStructure(ApiEquivalenceCase testCase)
        {
            ApiEquivalenceFixture utilityFixture = new(BuilderMode.Utility);
            ApiEquivalenceFixture unityApiFixture = new(BuilderMode.UnityApi);

            testCase.Exercise(utilityFixture);
            testCase.Exercise(unityApiFixture);

            CollectObjects(utilityFixture.Controller);
            CollectObjects(unityApiFixture.Controller);

            AssertControllersEquivalent(utilityFixture.Controller, unityApiFixture.Controller);
        }

        [Test]
        public void ApiEquivalenceCases_CoverAllInternalApis()
        {
            string[] exposedApis = typeof(AnimatorControllerUtility).GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).Where(method => method.IsAssembly && !method.Name.Contains("<")).Select(GetApiSignature).OrderBy(signature => signature).ToArray();

            string[] coveredApis = ApiEquivalenceCases().Select(testCase => testCase.ApiSignature).OrderBy(signature => signature).ToArray();

            Assert.That(coveredApis, Is.EquivalentTo(exposedApis));
        }

        private static IEnumerable<TestCaseData> ApiEquivalenceTestCases()
        {
            return ApiEquivalenceCases().Select(testCase => new TestCaseData(testCase).SetName($"AnimatorControllerUtility.{testCase.Name}_MatchesUnityApiStructure"));
        }

        private static IEnumerable<ApiEquivalenceCase> ApiEquivalenceCases()
        {
            yield return Case("CreateAnimatorController", nameof(AnimatorControllerUtility.CreateAnimatorController), new[] { typeof(string), }, fixture => fixture.CreateControllerWithApi("Equivalent Controller"));

            yield return Case("AddLayer_ByName", nameof(AnimatorControllerUtility.AddLayer), new[] { typeof(AnimatorController), typeof(string), }, fixture =>
            {
                fixture.AddRawLayer("Layer");
                AddLayer(fixture.Mode, fixture.Controller, "Layer");
            });

            yield return Case("AddLayer_ByLayer", nameof(AnimatorControllerUtility.AddLayer), new[] { typeof(AnimatorController), typeof(AnimatorControllerLayer), }, fixture => AddLayer(fixture.Mode, fixture.Controller, fixture.CreateRawLayer("Supplied Layer")));

            yield return Case("AddParameter_ByName", nameof(AnimatorControllerUtility.AddParameter), new[] { typeof(AnimatorController), typeof(string), typeof(AnimatorControllerParameterType), }, fixture =>
            {
                fixture.AddRawParameter(new AnimatorControllerParameter
                {
                    name = "Parameter",
                    type = AnimatorControllerParameterType.Float,
                });
                AddParameter(fixture.Mode, fixture.Controller, "Parameter", AnimatorControllerParameterType.Bool);
            });

            yield return Case("AddParameter_ByParameter", nameof(AnimatorControllerUtility.AddParameter), new[] { typeof(AnimatorController), typeof(AnimatorControllerParameter), }, fixture => AddParameter(fixture.Mode, fixture.Controller, new AnimatorControllerParameter
            {
                name = "Supplied Parameter",
                type = AnimatorControllerParameterType.Float,
                defaultFloat = 0.5f,
            }));

            yield return Case("AddBlendTreeChild_ByMotion", nameof(AnimatorControllerUtility.AddBlendTreeChild), new[] { typeof(BlendTree), typeof(Motion), }, fixture => AddBlendTreeChild(fixture.Mode, fixture.AttachRawBlendTree(), CreateAnimatedClip("Child Motion", 1.0f)));

            yield return Case("AddBlendTreeChild_ByPosition", nameof(AnimatorControllerUtility.AddBlendTreeChild), new[] { typeof(BlendTree), typeof(Motion), typeof(Vector2), }, fixture => AddBlendTreeChild(fixture.Mode, fixture.AttachRawBlendTree(), CreateAnimatedClip("Child Motion", 1.0f), new Vector2(0.25f, 0.75f)));

            yield return Case("AddBlendTreeChild_ByThreshold", nameof(AnimatorControllerUtility.AddBlendTreeChild), new[] { typeof(BlendTree), typeof(Motion), typeof(float), }, fixture => AddBlendTreeChild(fixture.Mode, fixture.AttachRawBlendTree(), CreateAnimatedClip("Child Motion", 1.0f), 0.5f));

            yield return Case("AddBlendTreeChild_ByPositionAndThreshold", nameof(AnimatorControllerUtility.AddBlendTreeChild), new[] { typeof(BlendTree), typeof(Motion), typeof(Vector2), typeof(float), }, fixture => AddBlendTreeChild(fixture.Mode, fixture.AttachRawBlendTree(), CreateAnimatedClip("Child Motion", 1.0f), new Vector2(-0.25f, 0.5f), 0.75f));

            yield return Case("CreateBlendTreeChild_ByThreshold", nameof(AnimatorControllerUtility.CreateBlendTreeChild), new[] { typeof(BlendTree), typeof(float), }, fixture => CreateBlendTreeChild(fixture.Mode, fixture.AttachRawBlendTree(), 0.5f));

            yield return Case("CreateBlendTreeChild_ByPosition", nameof(AnimatorControllerUtility.CreateBlendTreeChild), new[] { typeof(BlendTree), typeof(Vector2), }, fixture => CreateBlendTreeChild(fixture.Mode, fixture.AttachRawBlendTree(), new Vector2(0.25f, -0.75f)));

            yield return Case("CreateBlendTreeChild_ByPositionAndThreshold", nameof(AnimatorControllerUtility.CreateBlendTreeChild), new[] { typeof(BlendTree), typeof(Vector2), typeof(float), }, fixture => CreateBlendTreeChild(fixture.Mode, fixture.AttachRawBlendTree(), new Vector2(0.75f, 0.25f), -0.5f));

            yield return Case("AddMotion_DefaultLayer", nameof(AnimatorControllerUtility.AddMotion), new[] { typeof(AnimatorController), typeof(Motion), }, fixture =>
            {
                fixture.AddRawLayer("Base Layer");
                AddMotion(fixture.Mode, fixture.Controller, CreateAnimatedClip("Motion Clip", 2.0f));
            });

            yield return Case("AddMotion_ByLayerIndex", nameof(AnimatorControllerUtility.AddMotion), new[] { typeof(AnimatorController), typeof(Motion), typeof(int), }, fixture =>
            {
                fixture.AddRawLayer("First Layer");
                fixture.AddRawLayer("Target Layer");
                AddMotion(fixture.Mode, fixture.Controller, CreateAnimatedClip("Motion Clip", 2.0f), 1);
            });

            yield return Case("CreateBlendTreeInController_DefaultLayer", nameof(AnimatorControllerUtility.CreateBlendTreeInController), new[] { typeof(AnimatorController), typeof(string), typeof(BlendTree).MakeByRefType(), }, fixture =>
            {
                fixture.AddRawLayer("Base Layer");
                CreateBlendTreeInController(fixture.Mode, fixture.Controller, "Controller BlendTree", out _);
            });

            yield return Case("CreateBlendTreeInController_ByLayerIndex", nameof(AnimatorControllerUtility.CreateBlendTreeInController), new[] { typeof(AnimatorController), typeof(string), typeof(BlendTree).MakeByRefType(), typeof(int), }, fixture =>
            {
                fixture.AddRawParameter(new AnimatorControllerParameter
                {
                    name = "Speed",
                    type = AnimatorControllerParameterType.Float,
                });
                fixture.AddRawLayer("First Layer");
                fixture.AddRawLayer("Target Layer");
                CreateBlendTreeInController(fixture.Mode, fixture.Controller, "Controller BlendTree", out _, 1);
            });

            yield return Case("AddState_ByName", nameof(AnimatorControllerUtility.AddState), new[] { typeof(AnimatorStateMachine), typeof(string), }, fixture =>
            {
                AnimatorStateMachine root = fixture.AddRawLayer("Base Layer");
                fixture.AddRawState(root, "Seed State", new Vector3(100.0f, 200.0f, 0.0f));
                AddState(fixture.Mode, root, "State");
            });

            yield return Case("AddState_ByNameAndPosition", nameof(AnimatorControllerUtility.AddState), new[] { typeof(AnimatorStateMachine), typeof(string), typeof(Vector3), }, fixture => AddState(fixture.Mode, fixture.AddRawLayer("Base Layer"), "State", new Vector3(125.0f, -25.0f, 0.0f)));

            yield return Case("AddState_ByStateAndPosition", nameof(AnimatorControllerUtility.AddState), new[] { typeof(AnimatorStateMachine), typeof(AnimatorState), typeof(Vector3), }, fixture =>
            {
                AnimatorState state = fixture.CreateRawState("Supplied State");
                Assert.That(AddState(fixture.Mode, fixture.AddRawLayer("Base Layer"), state, new Vector3(300.0f, 100.0f, 0.0f)), Is.True);
            });

            yield return Case("AddStateMachine_ByName", nameof(AnimatorControllerUtility.AddStateMachine), new[] { typeof(AnimatorStateMachine), typeof(string), }, fixture =>
            {
                AnimatorStateMachine root = fixture.AddRawLayer("Base Layer");
                fixture.AddRawStateMachine(root, "Seed StateMachine", new Vector3(50.0f, 100.0f, 0.0f));
                AddStateMachine(fixture.Mode, root, "Sub StateMachine");
            });

            yield return Case("AddStateMachine_ByNameAndPosition", nameof(AnimatorControllerUtility.AddStateMachine), new[] { typeof(AnimatorStateMachine), typeof(string), typeof(Vector3), }, fixture => AddStateMachine(fixture.Mode, fixture.AddRawLayer("Base Layer"), "Sub StateMachine", new Vector3(400.0f, 125.0f, 0.0f)));

            yield return Case("AddStateMachine_ByStateMachineAndPosition", nameof(AnimatorControllerUtility.AddStateMachine), new[] { typeof(AnimatorStateMachine), typeof(AnimatorStateMachine), typeof(Vector3), }, fixture =>
            {
                AnimatorStateMachine stateMachine = fixture.CreateRawStateMachine("Supplied StateMachine");
                Assert.That(AddStateMachine(fixture.Mode, fixture.AddRawLayer("Base Layer"), stateMachine, new Vector3(400.0f, 125.0f, 0.0f)), Is.True);
            });

            yield return Case("AddTransition_ToState", nameof(AnimatorControllerUtility.AddTransition), new[] { typeof(AnimatorState), typeof(AnimatorState), }, fixture =>
            {
                (AnimatorState source, AnimatorState destination) = fixture.AddRawStatePair();
                AddTransition(fixture.Mode, source, destination);
            });

            yield return Case("AddTransition_ToStateWithDefaultExitTime", nameof(AnimatorControllerUtility.AddTransition), new[] { typeof(AnimatorState), typeof(AnimatorState), typeof(bool), }, fixture =>
            {
                (AnimatorState source, AnimatorState destination) = fixture.AddRawStatePair();
                source.motion = CreateAnimatedClip("Source Motion", 2.0f);
                AddTransition(fixture.Mode, source, destination, true);
            });

            yield return Case("AddTransition_ToStateMachine", nameof(AnimatorControllerUtility.AddTransition), new[] { typeof(AnimatorState), typeof(AnimatorStateMachine), }, fixture =>
            {
                AnimatorStateMachine root = fixture.AddRawLayer("Base Layer");
                AnimatorState source = fixture.AddRawState(root, "Source State", Vector3.zero);
                AnimatorStateMachine destination = fixture.AddRawStateMachine(root, "Destination StateMachine", new Vector3(200.0f, 0.0f, 0.0f));
                AddTransition(fixture.Mode, source, destination);
            });

            yield return Case("AddTransition_ToStateMachineWithDefaultExitTime", nameof(AnimatorControllerUtility.AddTransition), new[] { typeof(AnimatorState), typeof(AnimatorStateMachine), typeof(bool), }, fixture =>
            {
                AnimatorStateMachine root = fixture.AddRawLayer("Base Layer");
                AnimatorState source = fixture.AddRawState(root, "Source State", Vector3.zero);
                source.motion = CreateAnimatedClip("Source Motion", 2.0f);
                AnimatorStateMachine destination = fixture.AddRawStateMachine(root, "Destination StateMachine", new Vector3(200.0f, 0.0f, 0.0f));
                AddTransition(fixture.Mode, source, destination, true);
            });

            yield return Case("AddExitTransition", nameof(AnimatorControllerUtility.AddExitTransition), new[] { typeof(AnimatorState), }, fixture => AddExitTransition(fixture.Mode, fixture.AddRawState(fixture.AddRawLayer("Base Layer"), "Source State", Vector3.zero)));

            yield return Case("AddExitTransition_WithDefaultExitTime", nameof(AnimatorControllerUtility.AddExitTransition), new[] { typeof(AnimatorState), typeof(bool), }, fixture =>
            {
                AnimatorState source = fixture.AddRawState(fixture.AddRawLayer("Base Layer"), "Source State", Vector3.zero);
                source.motion = CreateAnimatedClip("Source Motion", 2.0f);
                AddExitTransition(fixture.Mode, source, true);
            });

            yield return Case("AddTransition_ByTransition", nameof(AnimatorControllerUtility.AddTransition), new[] { typeof(AnimatorState), typeof(AnimatorStateTransition), }, fixture =>
            {
                (AnimatorState source, AnimatorState destination) = fixture.AddRawStatePair();
                AddTransition(fixture.Mode, source, new AnimatorStateTransition
                {
                    destinationState = destination,
                    duration = 0.125f,
                    hasFixedDuration = true,
                    hideFlags = HideFlags.HideInHierarchy,
                });
            });

            yield return Case("AddAnyStateTransition_ToState", nameof(AnimatorControllerUtility.AddAnyStateTransition), new[] { typeof(AnimatorStateMachine), typeof(AnimatorState), }, fixture =>
            {
                AnimatorStateMachine root = fixture.AddRawLayer("Base Layer");
                AnimatorState destination = fixture.AddRawState(root, "Destination State", Vector3.zero);
                AddAnyStateTransition(fixture.Mode, root, destination);
            });

            yield return Case("AddAnyStateTransition_ToStateMachine", nameof(AnimatorControllerUtility.AddAnyStateTransition), new[] { typeof(AnimatorStateMachine), typeof(AnimatorStateMachine), }, fixture =>
            {
                AnimatorStateMachine root = fixture.AddRawLayer("Base Layer");
                AnimatorStateMachine destination = fixture.AddRawStateMachine(root, "Destination StateMachine", Vector3.zero);
                AddAnyStateTransition(fixture.Mode, root, destination);
            });

            yield return Case("AddEntryTransition_ToState", nameof(AnimatorControllerUtility.AddEntryTransition), new[] { typeof(AnimatorStateMachine), typeof(AnimatorState), }, fixture =>
            {
                AnimatorStateMachine root = fixture.AddRawLayer("Base Layer");
                AnimatorState destination = fixture.AddRawState(root, "Destination State", Vector3.zero);
                AddEntryTransition(fixture.Mode, root, destination);
            });

            yield return Case("AddEntryTransition_ToStateMachine", nameof(AnimatorControllerUtility.AddEntryTransition), new[] { typeof(AnimatorStateMachine), typeof(AnimatorStateMachine), }, fixture =>
            {
                AnimatorStateMachine root = fixture.AddRawLayer("Base Layer");
                AnimatorStateMachine destination = fixture.AddRawStateMachine(root, "Destination StateMachine", Vector3.zero);
                AddEntryTransition(fixture.Mode, root, destination);
            });

            yield return Case("AddStateMachineTransition", nameof(AnimatorControllerUtility.AddStateMachineTransition), new[] { typeof(AnimatorStateMachine), typeof(AnimatorStateMachine), }, fixture =>
            {
                AnimatorStateMachine root = fixture.AddRawLayer("Base Layer");
                AnimatorStateMachine source = fixture.AddRawStateMachine(root, "Source StateMachine", Vector3.zero);
                AddStateMachineTransition(fixture.Mode, root, source);
            });

            yield return Case("AddStateMachineTransition_ToState", nameof(AnimatorControllerUtility.AddStateMachineTransition), new[] { typeof(AnimatorStateMachine), typeof(AnimatorStateMachine), typeof(AnimatorState), }, fixture =>
            {
                AnimatorStateMachine root = fixture.AddRawLayer("Base Layer");
                AnimatorStateMachine source = fixture.AddRawStateMachine(root, "Source StateMachine", Vector3.zero);
                AnimatorState destination = fixture.AddRawState(root, "Destination State", new Vector3(200.0f, 0.0f, 0.0f));
                AddStateMachineTransition(fixture.Mode, root, source, destination);
            });

            yield return Case("AddStateMachineTransition_ToStateMachine", nameof(AnimatorControllerUtility.AddStateMachineTransition), new[] { typeof(AnimatorStateMachine), typeof(AnimatorStateMachine), typeof(AnimatorStateMachine), }, fixture =>
            {
                AnimatorStateMachine root = fixture.AddRawLayer("Base Layer");
                AnimatorStateMachine source = fixture.AddRawStateMachine(root, "Source StateMachine", Vector3.zero);
                AnimatorStateMachine destination = fixture.AddRawStateMachine(root, "Destination StateMachine", new Vector3(200.0f, 0.0f, 0.0f));
                AddStateMachineTransition(fixture.Mode, root, source, destination);
            });

            yield return Case("AddStateMachineExitTransition", nameof(AnimatorControllerUtility.AddStateMachineExitTransition), new[] { typeof(AnimatorStateMachine), typeof(AnimatorStateMachine), }, fixture =>
            {
                AnimatorStateMachine root = fixture.AddRawLayer("Base Layer");
                AnimatorStateMachine source = fixture.AddRawStateMachine(root, "Source StateMachine", Vector3.zero);
                AddStateMachineExitTransition(fixture.Mode, root, source);
            });

            yield return Case("AddCondition", nameof(AnimatorControllerUtility.AddCondition), new[] { typeof(AnimatorTransitionBase), typeof(AnimatorConditionMode), typeof(float), typeof(string), }, fixture =>
            {
                AnimatorStateTransition transition = fixture.AddRawTransition();
                AddCondition(fixture.Mode, transition, AnimatorConditionMode.Greater, 0.5f, "Speed");
            });
        }

        private static ApiEquivalenceCase Case(string name, string methodName, Type[] parameterTypes, Action<ApiEquivalenceFixture> exercise)
        {
            return ApiEquivalenceCase.Create(name, GetApiSignature(methodName, parameterTypes), exercise);
        }

        private static string GetApiSignature(MethodInfo method)
        {
            return GetApiSignature(method.Name, method.GetParameters().Select(parameter => parameter.ParameterType));
        }

        private static string GetApiSignature(string methodName, IEnumerable<Type> parameterTypes)
        {
            return $"{methodName}({string.Join(", ", parameterTypes.Select(GetTypeName))})";
        }

        private static string GetTypeName(Type type)
        {
            if (type.IsByRef)
            {
                return $"{GetTypeName(type.GetElementType())}&";
            }

            return type.FullName ?? type.Name;
        }

        private static AnimatorController BuildController(BuilderMode mode)
        {
            AnimatorController controller = CreateController(mode, "Equivalent Controller");

            AddParameter(mode, controller, "Blend", AnimatorControllerParameterType.Bool);
            AddLayer(mode, controller, "String Layer");

            AnimationClip motionClip = CreateAnimatedClip("Motion Clip", 2.0f);
            AnimatorState motionState = AddMotion(mode, controller, motionClip, 0);
            motionState.writeDefaultValues = true;
            controller.layers[0].stateMachine.defaultState = motionState;

            AnimatorStateMachine configuredStateMachine = new()
            {
                name = controller.MakeUniqueLayerName("Configured Layer"),
                hideFlags = HideFlags.HideInHierarchy,
            };
            AnimatorControllerLayer configuredLayer = new()
            {
                name = configuredStateMachine.name,
                stateMachine = configuredStateMachine,
                defaultWeight = 1.0f,
                blendingMode = AnimatorLayerBlendingMode.Additive,
                iKPass = true,
                syncedLayerAffectsTiming = true,
            };
            AddLayer(mode, controller, configuredLayer);
            int configuredLayerIndex = controller.layers.Length - 1;

            AnimatorState controllerTreeState = CreateBlendTreeInController(mode, controller, "Controller BlendTree", out BlendTree controllerTree, configuredLayerIndex);
            controllerTreeState.writeDefaultValues = true;
            controllerTree.blendType = BlendTreeType.Simple1D;
            controllerTree.useAutomaticThresholds = false;
            AddBlendTreeChild(mode, controllerTree, CreateAnimatedClip("Controller Tree Child", 1.0f), 0.5f);

            AddParameter(mode, controller, "Speed", AnimatorControllerParameterType.Float);
            AddParameter(mode, controller, "Offset", AnimatorControllerParameterType.Float);
            AddParameter(mode, controller, new AnimatorControllerParameter
            {
                name = "Count",
                type = AnimatorControllerParameterType.Int,
                defaultInt = 2,
            });
            AddParameter(mode, controller, new AnimatorControllerParameter
            {
                name = "Enabled",
                type = AnimatorControllerParameterType.Bool,
                defaultBool = true,
            });
            AddParameter(mode, controller, new AnimatorControllerParameter
            {
                name = "Trigger",
                type = AnimatorControllerParameterType.Trigger,
            });

            AnimatorStateMachine root = configuredStateMachine;
            AnimatorState stateA = AddState(mode, root, "State A", new Vector3(100.0f, 200.0f, 0.0f));
            stateA.motion = CreateAnimatedClip("State A Motion", 2.0f);
            stateA.speed = 1.25f;
            stateA.cycleOffset = 0.125f;
            stateA.mirror = true;
            stateA.iKOnFeet = true;
            stateA.writeDefaultValues = true;
            stateA.tag = "Primary";
            stateA.speedParameter = "Speed";
            stateA.speedParameterActive = true;
            stateA.cycleOffsetParameter = "Offset";
            stateA.cycleOffsetParameterActive = true;
            stateA.mirrorParameter = "Enabled";
            stateA.mirrorParameterActive = true;
            stateA.timeParameter = "Speed";
            stateA.timeParameterActive = true;

            AnimatorState stateB = AddState(mode, root, "State B");
            stateB.writeDefaultValues = false;
            BlendTree stateBTree = new()
            {
                name = "State B Tree",
                hideFlags = HideFlags.HideInHierarchy,
                blendType = BlendTreeType.Simple1D,
                blendParameter = "Speed",
                useAutomaticThresholds = false,
                minThreshold = -1.0f,
                maxThreshold = 1.0f,
            };
            stateB.motion = stateBTree;
            AddBlendTreeChild(mode, stateBTree, CreateAnimatedClip("Threshold Child", 1.0f), -1.0f);
            AddBlendTreeChild(mode, stateBTree, CreateAnimatedClip("Position Child", 1.0f), new Vector2(0.25f, 0.75f));
            BlendTree thresholdChildTree = CreateBlendTreeChild(mode, stateBTree, 0.25f);
            thresholdChildTree.blendType = BlendTreeType.Direct;
            thresholdChildTree.blendParameter = "Speed";
            AddBlendTreeChild(mode, thresholdChildTree, CreateAnimatedClip("Nested Threshold Child", 1.0f), 0.0f);
            BlendTree positionChildTree = CreateBlendTreeChild(mode, stateBTree, new Vector2(-0.5f, 0.5f));
            positionChildTree.blendType = BlendTreeType.Simple1D;
            positionChildTree.blendParameter = "Offset";

            AnimatorState externalState = new()
            {
                name = root.MakeUniqueStateName("External State"),
                hideFlags = HideFlags.HideInHierarchy,
                motion = CreateAnimatedClip("External Motion", 1.0f),
                writeDefaultValues = true,
            };
            AddState(mode, root, externalState, new Vector3(400.0f, 200.0f, 0.0f));
            root.defaultState = stateA;

            AnimatorStateMachine subStateMachine = AddStateMachine(mode, root, "Sub StateMachine", new Vector3(600.0f, 100.0f, 0.0f));
            AnimatorState subState = AddState(mode, subStateMachine, "Sub State", new Vector3(200.0f, 50.0f, 0.0f));
            subState.motion = CreateAnimatedClip("Sub Motion", 1.5f);
            subState.writeDefaultValues = true;
            subStateMachine.defaultState = subState;

            AnimatorStateMachine secondSubStateMachine = AddStateMachine(mode, root, "Second Sub StateMachine", new Vector3(600.0f, 250.0f, 0.0f));
            AnimatorState secondSubState = AddState(mode, secondSubStateMachine, "Second Sub State");
            secondSubState.motion = CreateAnimatedClip("Second Sub Motion", 1.0f);
            secondSubStateMachine.defaultState = secondSubState;

            AnimatorStateTransition toStateB = AddTransition(mode, stateA, stateB);
            toStateB.offset = 0.1f;
            toStateB.interruptionSource = TransitionInterruptionSource.Source;
            toStateB.orderedInterruption = true;
            AddCondition(mode, toStateB, AnimatorConditionMode.If, 0.0f, "Enabled");
            AddCondition(mode, toStateB, AnimatorConditionMode.Greater, 0.25f, "Speed");

            AnimatorStateTransition toSubStateMachine = AddTransition(mode, stateA, subStateMachine, true);
            AddCondition(mode, toSubStateMachine, AnimatorConditionMode.Less, 0.75f, "Offset");

            AnimatorStateTransition exitTransition = AddExitTransition(mode, stateB, true);
            AddCondition(mode, exitTransition, AnimatorConditionMode.If, 0.0f, "Trigger");

            AnimatorStateTransition subToRoot = AddTransition(mode, subState, stateA);
            AddCondition(mode, subToRoot, AnimatorConditionMode.IfNot, 0.0f, "Enabled");

            AnimatorStateTransition anyToState = AddAnyStateTransition(mode, root, stateB);
            anyToState.canTransitionToSelf = false;
            AddCondition(mode, anyToState, AnimatorConditionMode.Equals, 2.0f, "Count");

            AnimatorStateTransition anyToStateMachine = AddAnyStateTransition(mode, root, subStateMachine);
            AddCondition(mode, anyToStateMachine, AnimatorConditionMode.NotEqual, 0.0f, "Count");

            AnimatorTransition entryToState = AddEntryTransition(mode, root, externalState);
            AddCondition(mode, entryToState, AnimatorConditionMode.If, 0.0f, "Enabled");
            AddEntryTransition(mode, root, subStateMachine);

            AnimatorTransition stateMachineToState = AddStateMachineTransition(mode, root, subStateMachine, stateB);
            AddCondition(mode, stateMachineToState, AnimatorConditionMode.Greater, 0.5f, "Speed");
            AnimatorTransition stateMachineToStateMachine = AddStateMachineTransition(mode, root, subStateMachine, secondSubStateMachine);
            AddCondition(mode, stateMachineToStateMachine, AnimatorConditionMode.Less, 0.25f, "Offset");
            AnimatorTransition stateMachineExit = AddStateMachineExitTransition(mode, root, secondSubStateMachine);
            AddCondition(mode, stateMachineExit, AnimatorConditionMode.IfNot, 0.0f, "Enabled");

            return controller;
        }

        private static AnimatorController CreateController(BuilderMode mode, string name)
        {
            if (mode == BuilderMode.Utility)
            {
                return AnimatorControllerUtility.CreateAnimatorController(name);
            }

            return CreateRawController(name);
        }

        private static AnimatorControllerParameter AddParameter(BuilderMode mode, AnimatorController controller, string name, AnimatorControllerParameterType type)
        {
            if (mode == BuilderMode.Utility)
            {
                return AnimatorControllerUtility.AddParameter(controller, name, type);
            }

            controller.AddParameter(name, type);
            AnimatorControllerParameter[] parameters = controller.parameters;
            return parameters[parameters.Length - 1];
        }

        private static void AddParameter(BuilderMode mode, AnimatorController controller, AnimatorControllerParameter parameter)
        {
            if (mode == BuilderMode.Utility)
            {
                AnimatorControllerUtility.AddParameter(controller, parameter);
                return;
            }

            controller.AddParameter(parameter);
        }

        private static AnimatorControllerLayer AddLayer(BuilderMode mode, AnimatorController controller, string name)
        {
            if (mode == BuilderMode.Utility)
            {
                AnimatorControllerUtility.AddLayer(controller, name);
            }
            else
            {
                controller.AddLayer(name);
            }

            AnimatorControllerLayer[] layers = controller.layers;
            return layers[layers.Length - 1];
        }

        private static void AddLayer(BuilderMode mode, AnimatorController controller, AnimatorControllerLayer layer)
        {
            if (mode == BuilderMode.Utility)
            {
                AnimatorControllerUtility.AddLayer(controller, layer);
                return;
            }

            controller.AddLayer(layer);
        }

        private static AnimatorState AddMotion(BuilderMode mode, AnimatorController controller, Motion motion, int layerIndex)
        {
            return mode == BuilderMode.Utility ? AnimatorControllerUtility.AddMotion(controller, motion, layerIndex) : controller.AddMotion(motion, layerIndex);
        }

        private static AnimatorState AddMotion(BuilderMode mode, AnimatorController controller, Motion motion)
        {
            return mode == BuilderMode.Utility ? AnimatorControllerUtility.AddMotion(controller, motion) : controller.AddMotion(motion);
        }

        private static AnimatorState CreateBlendTreeInController(BuilderMode mode, AnimatorController controller, string name, out BlendTree tree, int layerIndex)
        {
            return mode == BuilderMode.Utility ? AnimatorControllerUtility.CreateBlendTreeInController(controller, name, out tree, layerIndex) : controller.CreateBlendTreeInController(name, out tree, layerIndex);
        }

        private static AnimatorState CreateBlendTreeInController(BuilderMode mode, AnimatorController controller, string name, out BlendTree tree)
        {
            return mode == BuilderMode.Utility ? AnimatorControllerUtility.CreateBlendTreeInController(controller, name, out tree) : controller.CreateBlendTreeInController(name, out tree);
        }

        private static AnimatorState AddState(BuilderMode mode, AnimatorStateMachine stateMachine, string name)
        {
            return mode == BuilderMode.Utility ? AnimatorControllerUtility.AddState(stateMachine, name) : stateMachine.AddState(name);
        }

        private static AnimatorState AddState(BuilderMode mode, AnimatorStateMachine stateMachine, string name, Vector3 position)
        {
            return mode == BuilderMode.Utility ? AnimatorControllerUtility.AddState(stateMachine, name, position) : stateMachine.AddState(name, position);
        }

        private static bool AddState(BuilderMode mode, AnimatorStateMachine stateMachine, AnimatorState state, Vector3 position)
        {
            if (mode == BuilderMode.Utility)
            {
                return AnimatorControllerUtility.AddState(stateMachine, state, position);
            }

            stateMachine.AddState(state, position);
            return true;
        }

        private static AnimatorStateMachine AddStateMachine(BuilderMode mode, AnimatorStateMachine parent, string name)
        {
            return mode == BuilderMode.Utility ? AnimatorControllerUtility.AddStateMachine(parent, name) : parent.AddStateMachine(name);
        }

        private static AnimatorStateMachine AddStateMachine(BuilderMode mode, AnimatorStateMachine parent, string name, Vector3 position)
        {
            return mode == BuilderMode.Utility ? AnimatorControllerUtility.AddStateMachine(parent, name, position) : parent.AddStateMachine(name, position);
        }

        private static bool AddStateMachine(BuilderMode mode, AnimatorStateMachine parent, AnimatorStateMachine stateMachine, Vector3 position)
        {
            if (mode == BuilderMode.Utility)
            {
                return AnimatorControllerUtility.AddStateMachine(parent, stateMachine, position);
            }

            parent.AddStateMachine(stateMachine, position);
            return true;
        }

        private static AnimatorStateTransition AddTransition(BuilderMode mode, AnimatorState sourceState, AnimatorState destinationState)
        {
            return mode == BuilderMode.Utility ? AnimatorControllerUtility.AddTransition(sourceState, destinationState) : sourceState.AddTransition(destinationState);
        }

        private static AnimatorStateTransition AddTransition(BuilderMode mode, AnimatorState sourceState, AnimatorState destinationState, bool defaultExitTime)
        {
            return mode == BuilderMode.Utility ? AnimatorControllerUtility.AddTransition(sourceState, destinationState, defaultExitTime) : sourceState.AddTransition(destinationState, defaultExitTime);
        }

        private static AnimatorStateTransition AddTransition(BuilderMode mode, AnimatorState sourceState, AnimatorStateMachine destinationStateMachine)
        {
            return mode == BuilderMode.Utility ? AnimatorControllerUtility.AddTransition(sourceState, destinationStateMachine) : sourceState.AddTransition(destinationStateMachine);
        }

        private static AnimatorStateTransition AddTransition(BuilderMode mode, AnimatorState sourceState, AnimatorStateMachine destinationStateMachine, bool defaultExitTime)
        {
            return mode == BuilderMode.Utility ? AnimatorControllerUtility.AddTransition(sourceState, destinationStateMachine, defaultExitTime) : sourceState.AddTransition(destinationStateMachine, defaultExitTime);
        }

        private static AnimatorStateTransition AddExitTransition(BuilderMode mode, AnimatorState sourceState)
        {
            return mode == BuilderMode.Utility ? AnimatorControllerUtility.AddExitTransition(sourceState) : sourceState.AddExitTransition();
        }

        private static AnimatorStateTransition AddExitTransition(BuilderMode mode, AnimatorState sourceState, bool defaultExitTime)
        {
            return mode == BuilderMode.Utility ? AnimatorControllerUtility.AddExitTransition(sourceState, defaultExitTime) : sourceState.AddExitTransition(defaultExitTime);
        }

        private static void AddTransition(BuilderMode mode, AnimatorState sourceState, AnimatorStateTransition transition)
        {
            if (mode == BuilderMode.Utility)
            {
                AnimatorControllerUtility.AddTransition(sourceState, transition);
                return;
            }

            sourceState.AddTransition(transition);
        }

        private static AnimatorStateTransition AddAnyStateTransition(BuilderMode mode, AnimatorStateMachine stateMachine, AnimatorState destinationState)
        {
            return mode == BuilderMode.Utility ? AnimatorControllerUtility.AddAnyStateTransition(stateMachine, destinationState) : stateMachine.AddAnyStateTransition(destinationState);
        }

        private static AnimatorStateTransition AddAnyStateTransition(BuilderMode mode, AnimatorStateMachine stateMachine, AnimatorStateMachine destinationStateMachine)
        {
            return mode == BuilderMode.Utility ? AnimatorControllerUtility.AddAnyStateTransition(stateMachine, destinationStateMachine) : stateMachine.AddAnyStateTransition(destinationStateMachine);
        }

        private static AnimatorTransition AddEntryTransition(BuilderMode mode, AnimatorStateMachine stateMachine, AnimatorState destinationState)
        {
            return mode == BuilderMode.Utility ? AnimatorControllerUtility.AddEntryTransition(stateMachine, destinationState) : stateMachine.AddEntryTransition(destinationState);
        }

        private static AnimatorTransition AddEntryTransition(BuilderMode mode, AnimatorStateMachine stateMachine, AnimatorStateMachine destinationStateMachine)
        {
            return mode == BuilderMode.Utility ? AnimatorControllerUtility.AddEntryTransition(stateMachine, destinationStateMachine) : stateMachine.AddEntryTransition(destinationStateMachine);
        }

        private static AnimatorTransition AddStateMachineTransition(BuilderMode mode, AnimatorStateMachine stateMachine, AnimatorStateMachine sourceStateMachine)
        {
            return mode == BuilderMode.Utility ? AnimatorControllerUtility.AddStateMachineTransition(stateMachine, sourceStateMachine) : stateMachine.AddStateMachineTransition(sourceStateMachine);
        }

        private static AnimatorTransition AddStateMachineTransition(BuilderMode mode, AnimatorStateMachine stateMachine, AnimatorStateMachine sourceStateMachine, AnimatorState destinationState)
        {
            return mode == BuilderMode.Utility ? AnimatorControllerUtility.AddStateMachineTransition(stateMachine, sourceStateMachine, destinationState) : stateMachine.AddStateMachineTransition(sourceStateMachine, destinationState);
        }

        private static AnimatorTransition AddStateMachineTransition(BuilderMode mode, AnimatorStateMachine stateMachine, AnimatorStateMachine sourceStateMachine, AnimatorStateMachine destinationStateMachine)
        {
            return mode == BuilderMode.Utility ? AnimatorControllerUtility.AddStateMachineTransition(stateMachine, sourceStateMachine, destinationStateMachine) : stateMachine.AddStateMachineTransition(sourceStateMachine, destinationStateMachine);
        }

        private static AnimatorTransition AddStateMachineExitTransition(BuilderMode mode, AnimatorStateMachine stateMachine, AnimatorStateMachine sourceStateMachine)
        {
            return mode == BuilderMode.Utility ? AnimatorControllerUtility.AddStateMachineExitTransition(stateMachine, sourceStateMachine) : stateMachine.AddStateMachineExitTransition(sourceStateMachine);
        }

        private static void AddCondition(BuilderMode mode, AnimatorTransitionBase transition, AnimatorConditionMode conditionMode, float threshold, string parameter)
        {
            if (mode == BuilderMode.Utility)
            {
                AnimatorControllerUtility.AddCondition(transition, conditionMode, threshold, parameter);
                return;
            }

            transition.AddCondition(conditionMode, threshold, parameter);
        }

        private static void AddBlendTreeChild(BuilderMode mode, BlendTree tree, Motion motion, float threshold)
        {
            if (mode == BuilderMode.Utility)
            {
                AnimatorControllerUtility.AddBlendTreeChild(tree, motion, threshold);
                return;
            }

            tree.AddChild(motion, threshold);
        }

        private static void AddBlendTreeChild(BuilderMode mode, BlendTree tree, Motion motion)
        {
            if (mode == BuilderMode.Utility)
            {
                AnimatorControllerUtility.AddBlendTreeChild(tree, motion);
                return;
            }

            tree.AddChild(motion);
        }

        private static void AddBlendTreeChild(BuilderMode mode, BlendTree tree, Motion motion, Vector2 position)
        {
            if (mode == BuilderMode.Utility)
            {
                AnimatorControllerUtility.AddBlendTreeChild(tree, motion, position);
                return;
            }

            tree.AddChild(motion, position);
        }

        private static void AddBlendTreeChild(BuilderMode mode, BlendTree tree, Motion motion, Vector2 position, float threshold)
        {
            if (mode == BuilderMode.Utility)
            {
                AnimatorControllerUtility.AddBlendTreeChild(tree, motion, position, threshold);
                return;
            }

            tree.AddChild(motion, position);
            SetLastChildThreshold(tree, threshold);
        }

        private static BlendTree CreateBlendTreeChild(BuilderMode mode, BlendTree parent, float threshold)
        {
            return mode == BuilderMode.Utility ? AnimatorControllerUtility.CreateBlendTreeChild(parent, threshold) : parent.CreateBlendTreeChild(threshold);
        }

        private static BlendTree CreateBlendTreeChild(BuilderMode mode, BlendTree parent, Vector2 position)
        {
            return mode == BuilderMode.Utility ? AnimatorControllerUtility.CreateBlendTreeChild(parent, position) : parent.CreateBlendTreeChild(position);
        }

        private static BlendTree CreateBlendTreeChild(BuilderMode mode, BlendTree parent, Vector2 position, float threshold)
        {
            if (mode == BuilderMode.Utility)
            {
                return AnimatorControllerUtility.CreateBlendTreeChild(parent, position, threshold);
            }

            BlendTree child = parent.CreateBlendTreeChild(position);
            SetLastChildThreshold(parent, threshold);
            return child;
        }

        private static void SetLastChildThreshold(BlendTree tree, float threshold)
        {
            ChildMotion[] children = tree.children;
            ChildMotion child = children[children.Length - 1];
            child.threshold = threshold;
            children[children.Length - 1] = child;
            tree.children = children;
        }

        private static AnimationClip CreateAnimatedClip(string name, float duration)
        {
            AnimationClip clip = new()
            {
                name = name,
                hideFlags = HideFlags.HideInHierarchy,
            };
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(string.Empty, typeof(Transform), "m_LocalPosition.x"), new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(duration, 1.0f)));
            return clip;
        }

        private static AnimatorController CreateRawController(string name)
        {
            return new AnimatorController
            {
                name = name,
                layers = Array.Empty<AnimatorControllerLayer>(),
                parameters = Array.Empty<AnimatorControllerParameter>(),
            };
        }

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

        private void AssertControllersEquivalent(AnimatorController utilityController, AnimatorController unityApiController)
        {
            AnimatorComparisonContext context = new();
            AssertEqual(utilityController.name, unityApiController.name, "controller.name");
            CompareHideFlags(utilityController, unityApiController, "controller");
            CompareParameters(utilityController.parameters, unityApiController.parameters, "controller.parameters");
            CompareLayers(utilityController.layers, unityApiController.layers, context, "controller.layers");
        }

        private static void CompareParameters(AnimatorControllerParameter[] utilityParameters, AnimatorControllerParameter[] unityApiParameters, string path)
        {
            AssertEqual(utilityParameters.Length, unityApiParameters.Length, $"{path}.Length");
            for (int i = 0; i < utilityParameters.Length; i++)
            {
                AnimatorControllerParameter utilityParameter = utilityParameters[i];
                AnimatorControllerParameter unityApiParameter = unityApiParameters[i];
                string parameterPath = $"{path}[{i}]";
                AssertEqual(utilityParameter.name, unityApiParameter.name, $"{parameterPath}.name");
                AssertEqual(utilityParameter.type, unityApiParameter.type, $"{parameterPath}.type");
                AssertFloat(utilityParameter.defaultFloat, unityApiParameter.defaultFloat, $"{parameterPath}.defaultFloat");
                AssertEqual(utilityParameter.defaultInt, unityApiParameter.defaultInt, $"{parameterPath}.defaultInt");
                AssertEqual(utilityParameter.defaultBool, unityApiParameter.defaultBool, $"{parameterPath}.defaultBool");
            }
        }

        private void CompareLayers(AnimatorControllerLayer[] utilityLayers, AnimatorControllerLayer[] unityApiLayers, AnimatorComparisonContext context, string path)
        {
            AssertEqual(utilityLayers.Length, unityApiLayers.Length, $"{path}.Length");
            for (int i = 0; i < utilityLayers.Length; i++)
            {
                AnimatorControllerLayer utilityLayer = utilityLayers[i];
                AnimatorControllerLayer unityApiLayer = unityApiLayers[i];
                string layerPath = $"{path}[{i}]";
                AssertEqual(utilityLayer.name, unityApiLayer.name, $"{layerPath}.name");
                AssertFloat(utilityLayer.defaultWeight, unityApiLayer.defaultWeight, $"{layerPath}.defaultWeight");
                AssertEqual(utilityLayer.blendingMode, unityApiLayer.blendingMode, $"{layerPath}.blendingMode");
                AssertEqual(utilityLayer.syncedLayerIndex, unityApiLayer.syncedLayerIndex, $"{layerPath}.syncedLayerIndex");
                AssertEqual(utilityLayer.iKPass, unityApiLayer.iKPass, $"{layerPath}.iKPass");
                AssertEqual(utilityLayer.syncedLayerAffectsTiming, unityApiLayer.syncedLayerAffectsTiming, $"{layerPath}.syncedLayerAffectsTiming");
                AssertObjectReference(utilityLayer.avatarMask, unityApiLayer.avatarMask, $"{layerPath}.avatarMask");
                CompareStateMachine(utilityLayer.stateMachine, unityApiLayer.stateMachine, context, $"{layerPath}.stateMachine");
            }
        }

        private void CompareStateMachine(AnimatorStateMachine utilityStateMachine, AnimatorStateMachine unityApiStateMachine, AnimatorComparisonContext context, string path)
        {
            AssertObjectReference(utilityStateMachine, unityApiStateMachine, path);
            if (!utilityStateMachine || !unityApiStateMachine || !context.StateMachines.Add(utilityStateMachine, unityApiStateMachine, path))
            {
                return;
            }

            AssertEqual(utilityStateMachine.name, unityApiStateMachine.name, $"{path}.name");
            CompareHideFlags(utilityStateMachine, unityApiStateMachine, path);
            CompareVector3(utilityStateMachine.anyStatePosition, unityApiStateMachine.anyStatePosition, $"{path}.anyStatePosition");
            CompareVector3(utilityStateMachine.entryPosition, unityApiStateMachine.entryPosition, $"{path}.entryPosition");
            CompareVector3(utilityStateMachine.exitPosition, unityApiStateMachine.exitPosition, $"{path}.exitPosition");
            CompareVector3(utilityStateMachine.parentStateMachinePosition, unityApiStateMachine.parentStateMachinePosition, $"{path}.parentStateMachinePosition");
            AssertEqual(utilityStateMachine.behaviours.Length, unityApiStateMachine.behaviours.Length, $"{path}.behaviours.Length");

            ChildAnimatorState[] utilityStates = utilityStateMachine.states;
            ChildAnimatorState[] unityApiStates = unityApiStateMachine.states;
            AssertEqual(utilityStates.Length, unityApiStates.Length, $"{path}.states.Length");
            for (int i = 0; i < utilityStates.Length; i++)
            {
                CompareVector3(utilityStates[i].position, unityApiStates[i].position, $"{path}.states[{i}].position");
                CompareState(utilityStates[i].state, unityApiStates[i].state, context, $"{path}.states[{i}].state");
            }

            ChildAnimatorStateMachine[] utilityStateMachines = utilityStateMachine.stateMachines;
            ChildAnimatorStateMachine[] unityApiStateMachines = unityApiStateMachine.stateMachines;
            AssertEqual(utilityStateMachines.Length, unityApiStateMachines.Length, $"{path}.stateMachines.Length");
            for (int i = 0; i < utilityStateMachines.Length; i++)
            {
                CompareVector3(utilityStateMachines[i].position, unityApiStateMachines[i].position, $"{path}.stateMachines[{i}].position");
                CompareStateMachine(utilityStateMachines[i].stateMachine, unityApiStateMachines[i].stateMachine, context, $"{path}.stateMachines[{i}].stateMachine");
            }

            CompareMappedReference(utilityStateMachine.defaultState, unityApiStateMachine.defaultState, context.States, $"{path}.defaultState");

            for (int i = 0; i < utilityStates.Length; i++)
            {
                CompareAnimatorStateTransitions(utilityStates[i].state.transitions, unityApiStates[i].state.transitions, context, $"{path}.states[{i}].state.transitions");
            }

            CompareAnimatorStateTransitions(utilityStateMachine.anyStateTransitions, unityApiStateMachine.anyStateTransitions, context, $"{path}.anyStateTransitions");
            CompareAnimatorTransitions(utilityStateMachine.entryTransitions, unityApiStateMachine.entryTransitions, context, $"{path}.entryTransitions");
            for (int i = 0; i < utilityStateMachines.Length; i++)
            {
                CompareAnimatorTransitions(utilityStateMachine.GetStateMachineTransitions(utilityStateMachines[i].stateMachine), unityApiStateMachine.GetStateMachineTransitions(unityApiStateMachines[i].stateMachine), context, $"{path}.stateMachineTransitions[{i}]");
            }
        }

        private void CompareState(AnimatorState utilityState, AnimatorState unityApiState, AnimatorComparisonContext context, string path)
        {
            AssertObjectReference(utilityState, unityApiState, path);
            if (!utilityState || !unityApiState || !context.States.Add(utilityState, unityApiState, path))
            {
                return;
            }

            AssertEqual(utilityState.name, unityApiState.name, $"{path}.name");
            CompareHideFlags(utilityState, unityApiState, path);
            AssertFloat(utilityState.speed, unityApiState.speed, $"{path}.speed");
            AssertFloat(utilityState.cycleOffset, unityApiState.cycleOffset, $"{path}.cycleOffset");
            AssertEqual(utilityState.mirror, unityApiState.mirror, $"{path}.mirror");
            AssertEqual(utilityState.iKOnFeet, unityApiState.iKOnFeet, $"{path}.iKOnFeet");
            AssertEqual(utilityState.writeDefaultValues, unityApiState.writeDefaultValues, $"{path}.writeDefaultValues");
            AssertEqual(utilityState.tag, unityApiState.tag, $"{path}.tag");
            AssertEqual(utilityState.speedParameter, unityApiState.speedParameter, $"{path}.speedParameter");
            AssertEqual(utilityState.speedParameterActive, unityApiState.speedParameterActive, $"{path}.speedParameterActive");
            AssertEqual(utilityState.cycleOffsetParameter, unityApiState.cycleOffsetParameter, $"{path}.cycleOffsetParameter");
            AssertEqual(utilityState.cycleOffsetParameterActive, unityApiState.cycleOffsetParameterActive, $"{path}.cycleOffsetParameterActive");
            AssertEqual(utilityState.mirrorParameter, unityApiState.mirrorParameter, $"{path}.mirrorParameter");
            AssertEqual(utilityState.mirrorParameterActive, unityApiState.mirrorParameterActive, $"{path}.mirrorParameterActive");
            AssertEqual(utilityState.timeParameter, unityApiState.timeParameter, $"{path}.timeParameter");
            AssertEqual(utilityState.timeParameterActive, unityApiState.timeParameterActive, $"{path}.timeParameterActive");
            AssertEqual(utilityState.behaviours.Length, unityApiState.behaviours.Length, $"{path}.behaviours.Length");
            CompareMotion(utilityState.motion, unityApiState.motion, context, $"{path}.motion");
        }

        private void CompareMotion(Motion utilityMotion, Motion unityApiMotion, AnimatorComparisonContext context, string path)
        {
            AssertObjectReference(utilityMotion, unityApiMotion, path);
            if (!utilityMotion || !unityApiMotion || !context.Motions.Add(utilityMotion, unityApiMotion, path))
            {
                return;
            }

            AssertEqual(utilityMotion.GetType(), unityApiMotion.GetType(), $"{path}.type");
            AssertEqual(utilityMotion.name, unityApiMotion.name, $"{path}.name");
            CompareHideFlags(utilityMotion, unityApiMotion, path);

            if (utilityMotion is BlendTree utilityTree && unityApiMotion is BlendTree unityApiTree)
            {
                CompareBlendTree(utilityTree, unityApiTree, context, path);
                return;
            }

            if (utilityMotion is AnimationClip utilityClip && unityApiMotion is AnimationClip unityApiClip)
            {
                CompareAnimationClip(utilityClip, unityApiClip, path);
            }
        }

        private void CompareBlendTree(BlendTree utilityTree, BlendTree unityApiTree, AnimatorComparisonContext context, string path)
        {
            AssertEqual(utilityTree.blendParameter, unityApiTree.blendParameter, $"{path}.blendParameter");
            AssertEqual(utilityTree.blendParameterY, unityApiTree.blendParameterY, $"{path}.blendParameterY");
            AssertEqual(utilityTree.blendType, unityApiTree.blendType, $"{path}.blendType");
            AssertEqual(utilityTree.useAutomaticThresholds, unityApiTree.useAutomaticThresholds, $"{path}.useAutomaticThresholds");
            AssertFloat(utilityTree.minThreshold, unityApiTree.minThreshold, $"{path}.minThreshold");
            AssertFloat(utilityTree.maxThreshold, unityApiTree.maxThreshold, $"{path}.maxThreshold");

            ChildMotion[] utilityChildren = utilityTree.children;
            ChildMotion[] unityApiChildren = unityApiTree.children;
            AssertEqual(utilityChildren.Length, unityApiChildren.Length, $"{path}.children.Length");
            for (int i = 0; i < utilityChildren.Length; i++)
            {
                ChildMotion utilityChild = utilityChildren[i];
                ChildMotion unityApiChild = unityApiChildren[i];
                string childPath = $"{path}.children[{i}]";
                AssertFloat(utilityChild.threshold, unityApiChild.threshold, $"{childPath}.threshold");
                CompareVector2(utilityChild.position, unityApiChild.position, $"{childPath}.position");
                AssertFloat(utilityChild.timeScale, unityApiChild.timeScale, $"{childPath}.timeScale");
                AssertFloat(utilityChild.cycleOffset, unityApiChild.cycleOffset, $"{childPath}.cycleOffset");
                AssertEqual(utilityChild.directBlendParameter, unityApiChild.directBlendParameter, $"{childPath}.directBlendParameter");
                AssertEqual(utilityChild.mirror, unityApiChild.mirror, $"{childPath}.mirror");
                CompareMotion(utilityChild.motion, unityApiChild.motion, context, $"{childPath}.motion");
            }
        }

        private static void CompareAnimationClip(AnimationClip utilityClip, AnimationClip unityApiClip, string path)
        {
            AssertFloat(utilityClip.frameRate, unityApiClip.frameRate, $"{path}.frameRate");
            AssertFloat(utilityClip.length, unityApiClip.length, $"{path}.length");
            AssertEqual(utilityClip.wrapMode, unityApiClip.wrapMode, $"{path}.wrapMode");

            EditorCurveBinding[] utilityBindings = SortBindings(AnimationUtility.GetCurveBindings(utilityClip));
            EditorCurveBinding[] unityApiBindings = SortBindings(AnimationUtility.GetCurveBindings(unityApiClip));
            AssertEqual(utilityBindings.Length, unityApiBindings.Length, $"{path}.curveBindings.Length");
            for (int i = 0; i < utilityBindings.Length; i++)
            {
                CompareBinding(utilityBindings[i], unityApiBindings[i], $"{path}.curveBindings[{i}]");
                CompareCurve(AnimationUtility.GetEditorCurve(utilityClip, utilityBindings[i]), AnimationUtility.GetEditorCurve(unityApiClip, unityApiBindings[i]), $"{path}.curves[{i}]");
            }

            EditorCurveBinding[] utilityObjectBindings = SortBindings(AnimationUtility.GetObjectReferenceCurveBindings(utilityClip));
            EditorCurveBinding[] unityApiObjectBindings = SortBindings(AnimationUtility.GetObjectReferenceCurveBindings(unityApiClip));
            AssertEqual(utilityObjectBindings.Length, unityApiObjectBindings.Length, $"{path}.objectCurveBindings.Length");
        }

        private static void CompareAnimatorStateTransitions(AnimatorStateTransition[] utilityTransitions, AnimatorStateTransition[] unityApiTransitions, AnimatorComparisonContext context, string path)
        {
            AssertEqual(utilityTransitions.Length, unityApiTransitions.Length, $"{path}.Length");
            for (int i = 0; i < utilityTransitions.Length; i++)
            {
                CompareAnimatorStateTransition(utilityTransitions[i], unityApiTransitions[i], context, $"{path}[{i}]");
            }
        }

        private static void CompareAnimatorTransitions(AnimatorTransition[] utilityTransitions, AnimatorTransition[] unityApiTransitions, AnimatorComparisonContext context, string path)
        {
            AssertEqual(utilityTransitions.Length, unityApiTransitions.Length, $"{path}.Length");
            for (int i = 0; i < utilityTransitions.Length; i++)
            {
                CompareAnimatorTransition(utilityTransitions[i], unityApiTransitions[i], context, $"{path}[{i}]");
            }
        }

        private static void CompareAnimatorStateTransition(AnimatorStateTransition utilityTransition, AnimatorStateTransition unityApiTransition, AnimatorComparisonContext context, string path)
        {
            CompareTransitionBase(utilityTransition, unityApiTransition, context, path);
            AssertFloat(utilityTransition.duration, unityApiTransition.duration, $"{path}.duration");
            AssertFloat(utilityTransition.offset, unityApiTransition.offset, $"{path}.offset");
            AssertEqual(utilityTransition.interruptionSource, unityApiTransition.interruptionSource, $"{path}.interruptionSource");
            AssertEqual(utilityTransition.orderedInterruption, unityApiTransition.orderedInterruption, $"{path}.orderedInterruption");
            AssertFloat(utilityTransition.exitTime, unityApiTransition.exitTime, $"{path}.exitTime");
            AssertEqual(utilityTransition.hasExitTime, unityApiTransition.hasExitTime, $"{path}.hasExitTime");
            AssertEqual(utilityTransition.hasFixedDuration, unityApiTransition.hasFixedDuration, $"{path}.hasFixedDuration");
            AssertEqual(utilityTransition.canTransitionToSelf, unityApiTransition.canTransitionToSelf, $"{path}.canTransitionToSelf");
        }

        private static void CompareAnimatorTransition(AnimatorTransition utilityTransition, AnimatorTransition unityApiTransition, AnimatorComparisonContext context, string path)
        {
            CompareTransitionBase(utilityTransition, unityApiTransition, context, path);
        }

        private static void CompareTransitionBase(AnimatorTransitionBase utilityTransition, AnimatorTransitionBase unityApiTransition, AnimatorComparisonContext context, string path)
        {
            AssertObjectReference(utilityTransition, unityApiTransition, path);
            AssertEqual(utilityTransition.GetType(), unityApiTransition.GetType(), $"{path}.type");
            CompareHideFlags(utilityTransition, unityApiTransition, path);
            AssertEqual(utilityTransition.solo, unityApiTransition.solo, $"{path}.solo");
            AssertEqual(utilityTransition.mute, unityApiTransition.mute, $"{path}.mute");
            AssertEqual(utilityTransition.isExit, unityApiTransition.isExit, $"{path}.isExit");
            CompareMappedReference(utilityTransition.destinationState, unityApiTransition.destinationState, context.States, $"{path}.destinationState");
            CompareMappedReference(utilityTransition.destinationStateMachine, unityApiTransition.destinationStateMachine, context.StateMachines, $"{path}.destinationStateMachine");
            CompareConditions(utilityTransition.conditions, unityApiTransition.conditions, $"{path}.conditions");
        }

        private static void CompareConditions(AnimatorCondition[] utilityConditions, AnimatorCondition[] unityApiConditions, string path)
        {
            AssertEqual(utilityConditions.Length, unityApiConditions.Length, $"{path}.Length");
            for (int i = 0; i < utilityConditions.Length; i++)
            {
                AssertEqual(utilityConditions[i].mode, unityApiConditions[i].mode, $"{path}[{i}].mode");
                AssertFloat(utilityConditions[i].threshold, unityApiConditions[i].threshold, $"{path}[{i}].threshold");
                AssertEqual(utilityConditions[i].parameter, unityApiConditions[i].parameter, $"{path}[{i}].parameter");
            }
        }

        private static void CompareCurve(AnimationCurve utilityCurve, AnimationCurve unityApiCurve, string path)
        {
            Assert.That(utilityCurve != null, Is.EqualTo(unityApiCurve != null), path);
            if (utilityCurve == null || unityApiCurve == null)
            {
                return;
            }

            AssertEqual(utilityCurve.preWrapMode, unityApiCurve.preWrapMode, $"{path}.preWrapMode");
            AssertEqual(utilityCurve.postWrapMode, unityApiCurve.postWrapMode, $"{path}.postWrapMode");
            AssertEqual(utilityCurve.length, unityApiCurve.length, $"{path}.length");
            for (int i = 0; i < utilityCurve.length; i++)
            {
                Keyframe utilityKey = utilityCurve.keys[i];
                Keyframe unityApiKey = unityApiCurve.keys[i];
                string keyPath = $"{path}.keys[{i}]";
                AssertFloat(utilityKey.time, unityApiKey.time, $"{keyPath}.time");
                AssertFloat(utilityKey.value, unityApiKey.value, $"{keyPath}.value");
                AssertFloat(utilityKey.inTangent, unityApiKey.inTangent, $"{keyPath}.inTangent");
                AssertFloat(utilityKey.outTangent, unityApiKey.outTangent, $"{keyPath}.outTangent");
                AssertFloat(utilityKey.inWeight, unityApiKey.inWeight, $"{keyPath}.inWeight");
                AssertFloat(utilityKey.outWeight, unityApiKey.outWeight, $"{keyPath}.outWeight");
                AssertEqual(utilityKey.weightedMode, unityApiKey.weightedMode, $"{keyPath}.weightedMode");
            }
        }

        private static void CompareBinding(EditorCurveBinding utilityBinding, EditorCurveBinding unityApiBinding, string path)
        {
            AssertEqual(utilityBinding.path, unityApiBinding.path, $"{path}.path");
            AssertEqual(utilityBinding.type, unityApiBinding.type, $"{path}.type");
            AssertEqual(utilityBinding.propertyName, unityApiBinding.propertyName, $"{path}.propertyName");
            AssertEqual(utilityBinding.isDiscreteCurve, unityApiBinding.isDiscreteCurve, $"{path}.isDiscreteCurve");
            AssertEqual(utilityBinding.isPPtrCurve, unityApiBinding.isPPtrCurve, $"{path}.isPPtrCurve");
            AssertEqual(utilityBinding.isSerializeReferenceCurve, unityApiBinding.isSerializeReferenceCurve, $"{path}.isSerializeReferenceCurve");
        }

        private static EditorCurveBinding[] SortBindings(EditorCurveBinding[] bindings)
        {
            return bindings.OrderBy(binding => binding.path).ThenBy(binding => binding.type != null ? binding.type.FullName : string.Empty).ThenBy(binding => binding.propertyName).ToArray();
        }

        private static void CompareHideFlags(Object utilityObject, Object unityApiObject, string path)
        {
            AssertEqual(utilityObject.hideFlags, unityApiObject.hideFlags, $"{path}.hideFlags");
        }

        private static void CompareVector2(Vector2 utilityValue, Vector2 unityApiValue, string path)
        {
            AssertFloat(utilityValue.x, unityApiValue.x, $"{path}.x");
            AssertFloat(utilityValue.y, unityApiValue.y, $"{path}.y");
        }

        private static void CompareVector3(Vector3 utilityValue, Vector3 unityApiValue, string path)
        {
            AssertFloat(utilityValue.x, unityApiValue.x, $"{path}.x");
            AssertFloat(utilityValue.y, unityApiValue.y, $"{path}.y");
            AssertFloat(utilityValue.z, unityApiValue.z, $"{path}.z");
        }

        private static void AssertFloat(float utilityValue, float unityApiValue, string path)
        {
            Assert.That(utilityValue, Is.EqualTo(unityApiValue).Within(FloatTolerance), path);
        }

        private static void AssertEqual<T>(T utilityValue, T unityApiValue, string path)
        {
            Assert.That(utilityValue, Is.EqualTo(unityApiValue), path);
        }

        private static void AssertObjectReference(Object utilityObject, Object unityApiObject, string path)
        {
            Assert.That((bool)utilityObject, Is.EqualTo((bool)unityApiObject), path);
        }

        private static void CompareMappedReference<T>(T utilityObject, T unityApiObject, ObjectPairMap<T> map, string path) where T : Object
        {
            AssertObjectReference(utilityObject, unityApiObject, path);
            if (!utilityObject || !unityApiObject)
            {
                return;
            }

            Assert.That(map.TryGetUnityApiObject(utilityObject, out T mappedObject), Is.True, $"{path}: utility object is not registered");
            Assert.That(mappedObject, Is.SameAs(unityApiObject), path);
        }

        private void CollectObjects(AnimatorController controller)
        {
            CollectObject(controller);
            foreach (AnimatorControllerLayer layer in controller.layers)
            {
                CollectStateMachine(layer.stateMachine);
            }
        }

        private void CollectStateMachine(AnimatorStateMachine stateMachine)
        {
            if (!CollectObject(stateMachine))
            {
                return;
            }

            foreach (ChildAnimatorState childState in stateMachine.states)
            {
                CollectState(childState.state);
            }

            foreach (AnimatorStateTransition transition in stateMachine.anyStateTransitions)
            {
                CollectObject(transition);
            }

            foreach (AnimatorTransition transition in stateMachine.entryTransitions)
            {
                CollectObject(transition);
            }

            foreach (ChildAnimatorStateMachine childStateMachine in stateMachine.stateMachines)
            {
                foreach (AnimatorTransition transition in stateMachine.GetStateMachineTransitions(childStateMachine.stateMachine))
                {
                    CollectObject(transition);
                }

                CollectStateMachine(childStateMachine.stateMachine);
            }
        }

        private void CollectState(AnimatorState state)
        {
            if (!CollectObject(state))
            {
                return;
            }

            CollectMotion(state.motion);
            foreach (AnimatorStateTransition transition in state.transitions)
            {
                CollectObject(transition);
            }
        }

        private void CollectMotion(Motion motion)
        {
            if (!CollectObject(motion))
            {
                return;
            }

            if (motion is BlendTree tree)
            {
                foreach (ChildMotion child in tree.children)
                {
                    CollectMotion(child.motion);
                }
            }
        }

        private bool CollectObject(Object obj)
        {
            return obj && _objects.Add(obj);
        }

        public sealed class ApiEquivalenceCase
        {
            private ApiEquivalenceCase(string name, string apiSignature, Action<ApiEquivalenceFixture> exercise)
            {
                Name = name;
                ApiSignature = apiSignature;
                Exercise = exercise;
            }

            public string ApiSignature { get; }

            public Action<ApiEquivalenceFixture> Exercise { get; }

            public string Name { get; }

            public static ApiEquivalenceCase Create(string name, string apiSignature, Action<ApiEquivalenceFixture> exercise)
            {
                return new ApiEquivalenceCase(name, apiSignature, exercise);
            }

            public override string ToString()
            {
                return Name;
            }
        }

        public sealed class ApiEquivalenceFixture
        {
            public readonly BuilderMode Mode;

            public ApiEquivalenceFixture(BuilderMode mode)
            {
                Mode = mode;
                Controller = CreateRawController("Fixture Controller");
            }

            public AnimatorController Controller { get; private set; }

            public void CreateControllerWithApi(string name)
            {
                Controller = CreateController(Mode, name);
            }

            public AnimatorStateMachine AddRawLayer(string name)
            {
                AnimatorStateMachine stateMachine = CreateRawStateMachine(name);
                AnimatorControllerLayer layer = new()
                {
                    name = name,
                    stateMachine = stateMachine,
                };
                Controller.layers = AddToArray(Controller.layers, layer);
                return stateMachine;
            }

            public AnimatorControllerLayer CreateRawLayer(string name)
            {
                return new AnimatorControllerLayer
                {
                    name = name,
                    stateMachine = CreateRawStateMachine(name),
                    defaultWeight = 1.0f,
                    blendingMode = AnimatorLayerBlendingMode.Additive,
                    iKPass = true,
                    syncedLayerAffectsTiming = true,
                };
            }

            public void AddRawParameter(AnimatorControllerParameter parameter)
            {
                Controller.parameters = AddToArray(Controller.parameters, parameter);
            }

            public AnimatorState CreateRawState(string name)
            {
                return new AnimatorState
                {
                    name = name,
                    hideFlags = HideFlags.HideInHierarchy,
                    writeDefaultValues = true,
                };
            }

            public AnimatorState AddRawState(AnimatorStateMachine stateMachine, string name, Vector3 position)
            {
                AnimatorState state = CreateRawState(name);
                stateMachine.states = AddToArray(stateMachine.states, new ChildAnimatorState
                {
                    state = state,
                    position = position,
                });
                return state;
            }

            public AnimatorStateMachine CreateRawStateMachine(string name)
            {
                return new AnimatorStateMachine
                {
                    name = name,
                    hideFlags = HideFlags.HideInHierarchy,
                };
            }

            public AnimatorStateMachine AddRawStateMachine(AnimatorStateMachine parent, string name, Vector3 position)
            {
                AnimatorStateMachine stateMachine = CreateRawStateMachine(name);
                parent.stateMachines = AddToArray(parent.stateMachines, new ChildAnimatorStateMachine
                {
                    stateMachine = stateMachine,
                    position = position,
                });
                return stateMachine;
            }

            public BlendTree AttachRawBlendTree()
            {
                AnimatorStateMachine root = AddRawLayer("Base Layer");
                AnimatorState state = AddRawState(root, "BlendTree State", Vector3.zero);
                BlendTree tree = new()
                {
                    name = "BlendTree",
                    hideFlags = HideFlags.HideInHierarchy,
                };
                state.motion = tree;
                return tree;
            }

            public (AnimatorState Source, AnimatorState Destination) AddRawStatePair()
            {
                AnimatorStateMachine root = AddRawLayer("Base Layer");
                AnimatorState source = AddRawState(root, "Source State", Vector3.zero);
                AnimatorState destination = AddRawState(root, "Destination State", new Vector3(200.0f, 0.0f, 0.0f));
                return (source, destination);
            }

            public AnimatorStateTransition AddRawTransition()
            {
                (AnimatorState source, AnimatorState destination) = AddRawStatePair();
                AnimatorStateTransition transition = new()
                {
                    destinationState = destination,
                    hasExitTime = false,
                    hasFixedDuration = true,
                    duration = 0.25f,
                    exitTime = 0.75f,
                    hideFlags = HideFlags.HideInHierarchy,
                };
                source.transitions = AddToArray(source.transitions, transition);
                return transition;
            }
        }

        private sealed class AnimatorComparisonContext
        {
            public readonly ObjectPairMap<Motion> Motions = new();
            public readonly ObjectPairMap<AnimatorStateMachine> StateMachines = new();
            public readonly ObjectPairMap<AnimatorState> States = new();
        }

        private sealed class ObjectPairMap<T> where T : Object
        {
            private readonly Dictionary<T, T> _utilityToUnityApi = new();

            public bool Add(T utilityObject, T unityApiObject, string path)
            {
                if (_utilityToUnityApi.TryGetValue(utilityObject, out T mappedObject))
                {
                    Assert.That(mappedObject, Is.SameAs(unityApiObject), path);
                    return false;
                }

                _utilityToUnityApi.Add(utilityObject, unityApiObject);
                return true;
            }

            public bool TryGetUnityApiObject(T utilityObject, out T unityApiObject)
            {
                return _utilityToUnityApi.TryGetValue(utilityObject, out unityApiObject);
            }
        }
    }
}

using System;
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
        private const string ParameterPrefix = "USTL/v2/";
        private const string AlwaysOneParameterName = "USTL_FT_AlwaysOne";
        private const string IsLocalParameterName = "IsLocal";
        private const string SmoothingParameterName = "USTL_FT_Smoothing";
        private const string LocalSmoothedParameterPrefix = "USTL_FT_Smoothed/Local/";
        private const string BlendShapePropertyPrefix = "blendShape.";
        private const float EyelidNeutralValue = 0.75f;
        private const float DefaultSmoothing = 0.1f;

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
            AnimatorController controller = GenerateAnimatorController($"{GeneratedControllerName} ({source.gameObject.name})", source, context.AvatarRootTransform, ref parameterConfigs);
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

        internal static AnimatorController GenerateAnimatorController(string controllerName, USTLFaceTracking source, Transform avatarRoot, ref List<ParameterConfig> parameterConfigs)
        {
            AnimatorController controller = AnimatorControllerUtility.CreateAnimatorController(controllerName);

            AnimationClip emptyClip = CreateEmptyClip();

            List<ParameterAnimation> parameterAnimations = CollectParameterAnimations(source, avatarRoot);
            if (parameterAnimations.Count == 0)
            {
                return controller;
            }

            AddFloatParameter(controller, AlwaysOneParameterName, 1.0f);
            AddBoolParameter(controller, IsLocalParameterName, false);
            AddFloatParameter(controller, SmoothingParameterName, DefaultSmoothing);
            foreach (ParameterAnimation parameterAnimation in parameterAnimations)
            {
                AddFloatParameter(controller, parameterAnimation.ParameterName, parameterAnimation.DefaultValue);
                AddFloatParameter(controller, parameterAnimation.SmoothedParameterName, parameterAnimation.DefaultValue);
                parameterConfigs.Add(CreateLocalParameterConfig(parameterAnimation.ParameterName, parameterAnimation.DefaultValue));
            }

            CreateLocalUserLayers(controller, emptyClip, ref parameterAnimations);
            CreateRemoteUserLayers(controller, emptyClip, ref parameterAnimations);

            return controller;
        }

        private static void CreateLocalUserLayers(AnimatorController controller, AnimationClip emptyClip, ref List<ParameterAnimation> parameterAnimations)
        {
            AnimatorControllerLayer layer = AnimatorControllerUtility.AddLayer(controller, "USTL FaceTracking");
            layer.defaultWeight = 1.0f;
            EditorUtility.SetDirty(controller);

            BlendTree rootTree = new()
            {
                name = "USTL FaceTracking Root",
                hideFlags = HideFlags.HideInHierarchy,
                blendType = BlendTreeType.Direct,
            };

            AnimatorState remoteState = AnimatorControllerUtility.AddState(layer.stateMachine, "Remote");
            remoteState.writeDefaultValues = true;
            remoteState.motion = emptyClip;

            AnimatorState localState = AnimatorControllerUtility.AddState(layer.stateMachine, "Local");
            localState.motion = rootTree;
            localState.writeDefaultValues = true;

            layer.stateMachine.defaultState = remoteState;

            EditorUtility.SetDirty(rootTree);
            EditorUtility.SetDirty(remoteState);
            EditorUtility.SetDirty(localState);
            EditorUtility.SetDirty(layer.stateMachine);

            AddIsLocalTransitions(remoteState, localState);

            foreach (ParameterAnimation parameterAnimation in parameterAnimations)
            {
                BlendTree smoothingTree = CreateParameterSmoothingBlendTree(parameterAnimation);
                AddDirectBlendTreeChild(rootTree, smoothingTree, AlwaysOneParameterName);

                BlendTree parameterTree = CreateParameterBlendTree(parameterAnimation);
                AddDirectBlendTreeChild(rootTree, parameterTree, AlwaysOneParameterName);
            }
        }

        private static void CreateRemoteUserLayers(AnimatorController controller, AnimationClip emptyClip, ref List<ParameterAnimation> parameterAnimations)
        {
        }

        private static List<ParameterAnimation> CollectParameterAnimations(USTLFaceTracking source, Transform avatarRoot)
        {
            List<ParameterAnimation> parameterAnimations = new();
            if (!source || !source.faceMeshRenderer || !avatarRoot || !IsDescendantOf(source.faceMeshRenderer.transform, avatarRoot))
            {
                return parameterAnimations;
            }

            Dictionary<UnifiedExpression, BlendShapeBinding> blendShapeBindings = CollectBlendShapeBindings(source);
            if (blendShapeBindings.Count == 0)
            {
                return parameterAnimations;
            }

            string rendererPath = AnimationUtility.CalculateTransformPath(source.faceMeshRenderer.transform, avatarRoot);
            Dictionary<VRCFTParameter, ParameterAnimation> parameterAnimationMap = new();
            HashSet<TargetAnimationKey> targetKeys = new();
            FeatureSetting[] featureSettings = source.featureSettings ?? Array.Empty<FeatureSetting>();

            foreach (FeatureSetting featureSetting in featureSettings)
            {
                if (featureSetting == null || featureSetting.syncMode == ParameterSyncMode.None)
                {
                    continue;
                }

                FaceTrackingFeatureDefinition featureDefinition = FaceTrackingFeatureDefinition.All.GetValueOrDefault(featureSetting.feature);
                VRCFTParameterSet parameterSet = featureDefinition?.GetOutputFormatOrDefault(featureSetting.outputFormatId);
                if (parameterSet == null)
                {
                    continue;
                }

                foreach (VRCFTParameter parameter in parameterSet.Parameters)
                {
                    if (!VRCFTParameterDefinition.All.TryGetValue(parameter, out VRCFTParameterDefinition parameterDefinition))
                    {
                        continue;
                    }

                    foreach (ExpressionWeightTarget target in parameterDefinition.ExpressionTargets)
                    {
                        if (target.Expression == UnifiedExpression.None || !blendShapeBindings.TryGetValue(target.Expression, out BlendShapeBinding binding))
                        {
                            continue;
                        }

                        TargetAnimationKey targetKey = new(parameter, target.Expression, target.Type, binding.BlendShapeName);
                        if (!targetKeys.Add(targetKey))
                        {
                            continue;
                        }

                        if (!parameterAnimationMap.TryGetValue(parameter, out ParameterAnimation parameterAnimation))
                        {
                            parameterAnimation = new ParameterAnimation(parameter, GetVRCFTParameterName(parameter), rendererPath, GetDefaultValue(parameterDefinition.Range));
                            parameterAnimationMap[parameter] = parameterAnimation;
                            parameterAnimations.Add(parameterAnimation);
                        }

                        parameterAnimation.Targets.Add(new TargetAnimation(target.Type, binding.BlendShapeName, binding.MaxValue));
                    }
                }
            }

            return parameterAnimations;
        }

        private static Dictionary<UnifiedExpression, BlendShapeBinding> CollectBlendShapeBindings(USTLFaceTracking source)
        {
            Dictionary<UnifiedExpression, BlendShapeBinding> bindings = new();
            BlendshapeSetting[] blendshapeSettings = source.blendshapeSettings ?? Array.Empty<BlendshapeSetting>();

            foreach (BlendshapeSetting setting in blendshapeSettings)
            {
                if (setting == null || setting.expression == UnifiedExpression.None || string.IsNullOrWhiteSpace(setting.blendShapeName))
                {
                    continue;
                }

                bindings[setting.expression] = new BlendShapeBinding(setting.blendShapeName, Mathf.Clamp(setting.maxValue, 0.0f, 100.0f));
            }

            return bindings;
        }

        private static BlendTree CreateParameterBlendTree(ParameterAnimation parameterAnimation)
        {
            BlendTree tree = new()
            {
                name = parameterAnimation.Parameter.ToString(),
                hideFlags = HideFlags.HideInHierarchy,
                blendType = BlendTreeType.Simple1D,
                blendParameter = parameterAnimation.SmoothedParameterName,
                useAutomaticThresholds = false,
            };

            List<float> thresholds = GetThresholds(parameterAnimation.Targets);
            if (thresholds.Count > 0)
            {
                tree.minThreshold = thresholds[0];
                tree.maxThreshold = thresholds[thresholds.Count - 1];
            }

            foreach (float threshold in thresholds)
            {
                AnimationClip clip = CreateThresholdClip(parameterAnimation, threshold);
                AnimatorControllerUtility.AddBlendTreeChild(tree, clip, threshold);
            }

            EditorUtility.SetDirty(tree);
            return tree;
        }

        private static BlendTree CreateParameterSmoothingBlendTree(ParameterAnimation parameterAnimation)
        {
            BlendTree rootTree = new()
            {
                name = $"{parameterAnimation.Parameter} Smooth",
                hideFlags = HideFlags.HideInHierarchy,
                blendType = BlendTreeType.Simple1D,
                blendParameter = SmoothingParameterName,
                useAutomaticThresholds = false,
                minThreshold = 0.0f,
                maxThreshold = 1.0f,
            };

            BlendTree inputTree = CreateSmoothingSourceTree(parameterAnimation.ParameterName, parameterAnimation);
            inputTree.name = $"{parameterAnimation.Parameter} Input";

            BlendTree feedbackTree = CreateSmoothingSourceTree(parameterAnimation.SmoothedParameterName, parameterAnimation);
            feedbackTree.name = $"{parameterAnimation.Parameter} Feedback";

            AnimatorControllerUtility.AddBlendTreeChild(rootTree, inputTree, 0.0f);
            AnimatorControllerUtility.AddBlendTreeChild(rootTree, feedbackTree, 1.0f);

            EditorUtility.SetDirty(rootTree);
            return rootTree;
        }

        private static BlendTree CreateSmoothingSourceTree(string blendParameter, ParameterAnimation parameterAnimation)
        {
            BlendTree tree = new()
            {
                hideFlags = HideFlags.HideInHierarchy,
                blendType = BlendTreeType.Simple1D,
                blendParameter = blendParameter,
                useAutomaticThresholds = false,
                minThreshold = -1.0f,
                maxThreshold = 1.0f,
            };

            AnimationClip minClip = CreateAnimatorParameterClip(parameterAnimation.SmoothedParameterName, -1.0f, $"{parameterAnimation.Parameter} Smooth -1");
            AnimationClip maxClip = CreateAnimatorParameterClip(parameterAnimation.SmoothedParameterName, 1.0f, $"{parameterAnimation.Parameter} Smooth 1");
            AnimatorControllerUtility.AddBlendTreeChild(tree, minClip, -1.0f);
            AnimatorControllerUtility.AddBlendTreeChild(tree, maxClip, 1.0f);

            EditorUtility.SetDirty(tree);
            return tree;
        }

        private static AnimationClip CreateAnimatorParameterClip(string parameterName, float value, string clipName)
        {
            AnimationClip clip = new()
            {
                name = clipName,
                hideFlags = HideFlags.HideInHierarchy,
            };

            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), parameterName), CreateSingleKeyCurve(value));
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static AnimationClip CreateEmptyClip()
        {
            AnimationClip clip = new()
            {
                name = string.Empty,
                hideFlags = HideFlags.HideInHierarchy,
            };
            return clip;
        }

        private static AnimationClip CreateThresholdClip(ParameterAnimation parameterAnimation, float threshold)
        {
            AnimationClip clip = new()
            {
                name = $"{parameterAnimation.Parameter} {threshold:0.###}",
                hideFlags = HideFlags.HideInHierarchy,
            };

            Dictionary<string, float> valuesByBlendShape = new();
            foreach (TargetAnimation target in parameterAnimation.Targets)
            {
                float value = EvaluateWeight(target.CurveType, threshold) * target.MaxValue;
                if (!valuesByBlendShape.TryGetValue(target.BlendShapeName, out float currentValue) || currentValue < value)
                {
                    valuesByBlendShape[target.BlendShapeName] = value;
                }
            }

            foreach (KeyValuePair<string, float> entry in valuesByBlendShape)
            {
                EditorCurveBinding binding = EditorCurveBinding.FloatCurve(parameterAnimation.RendererPath, typeof(SkinnedMeshRenderer), $"{BlendShapePropertyPrefix}{entry.Key}");
                AnimationUtility.SetEditorCurve(clip, binding, CreateConstantCurve(entry.Value));
            }

            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static List<float> GetThresholds(List<TargetAnimation> targets)
        {
            SortedSet<float> thresholds = new();
            foreach (TargetAnimation target in targets)
            {
                switch (target.CurveType)
                {
                    case WeightCurveType.Linear:
                        thresholds.Add(0.0f);
                        thresholds.Add(1.0f);
                        break;
                    case WeightCurveType.PositiveSigned:
                    case WeightCurveType.NegativeSigned:
                        thresholds.Add(-1.0f);
                        thresholds.Add(0.0f);
                        thresholds.Add(1.0f);
                        break;
                    case WeightCurveType.EyelidClosed:
                    case WeightCurveType.EyelidWide:
                        thresholds.Add(0.0f);
                        thresholds.Add(EyelidNeutralValue);
                        thresholds.Add(1.0f);
                        break;
                    default:
                        thresholds.Add(0.0f);
                        thresholds.Add(1.0f);
                        break;
                }
            }

            return new List<float>(thresholds);
        }

        private static float EvaluateWeight(WeightCurveType curveType, float value)
        {
            return curveType switch
            {
                WeightCurveType.Linear => Mathf.Clamp01(value),
                WeightCurveType.PositiveSigned => Mathf.Clamp01(value),
                WeightCurveType.NegativeSigned => Mathf.Clamp01(-value),
                WeightCurveType.EyelidClosed => value < EyelidNeutralValue ? Mathf.InverseLerp(EyelidNeutralValue, 0.0f, value) : 0.0f,
                WeightCurveType.EyelidWide => value > EyelidNeutralValue ? Mathf.InverseLerp(EyelidNeutralValue, 1.0f, value) : 0.0f,
                _ => Mathf.Clamp01(value),
            };
        }

        private static AnimationCurve CreateConstantCurve(float value)
        {
            AnimationCurve curve = new();
            curve.AddKey(0.0f, value);
            curve.AddKey(1.0f, value);
            return curve;
        }

        private static AnimationCurve CreateSingleKeyCurve(float value)
        {
            AnimationCurve curve = new();
            curve.AddKey(0.0f, value);
            return curve;
        }

        private static void AddFloatParameter(AnimatorController controller, string name, float defaultValue)
        {
            AnimatorControllerParameter[] parameters = controller.parameters ?? Array.Empty<AnimatorControllerParameter>();
            foreach (AnimatorControllerParameter parameter in parameters)
            {
                if (parameter.name == name)
                {
                    return;
                }
            }

            AnimatorControllerUtility.AddParameter(controller, new AnimatorControllerParameter
            {
                name = name,
                type = AnimatorControllerParameterType.Float,
                defaultFloat = defaultValue,
            });
        }

        private static void AddBoolParameter(AnimatorController controller, string name, bool defaultValue)
        {
            AnimatorControllerParameter[] parameters = controller.parameters ?? Array.Empty<AnimatorControllerParameter>();
            foreach (AnimatorControllerParameter parameter in parameters)
            {
                if (parameter.name == name)
                {
                    return;
                }
            }

            AnimatorControllerUtility.AddParameter(controller, new AnimatorControllerParameter
            {
                name = name,
                type = AnimatorControllerParameterType.Bool,
                defaultBool = defaultValue,
            });
        }

        private static ParameterConfig CreateLocalParameterConfig(string name, float defaultValue)
        {
            return new ParameterConfig
            {
                nameOrPrefix = name,
                remapTo = string.Empty,
                syncType = ParameterSyncType.Float,
                localOnly = true,
                defaultValue = defaultValue,
                hasExplicitDefaultValue = !Mathf.Approximately(defaultValue, 0.0f),
            };
        }

        private static void AddIsLocalTransitions(AnimatorState remoteState, AnimatorState localState)
        {
            AnimatorStateTransition toLocal = AnimatorControllerUtility.AddTransition(remoteState, localState);
            toLocal.duration = 0.0f;
            toLocal.exitTime = 0.0f;
            AnimatorControllerUtility.AddCondition(toLocal, AnimatorConditionMode.If, 0.0f, IsLocalParameterName);

            AnimatorStateTransition toRemote = AnimatorControllerUtility.AddTransition(localState, remoteState);
            toRemote.duration = 0.0f;
            toRemote.exitTime = 0.0f;
            AnimatorControllerUtility.AddCondition(toRemote, AnimatorConditionMode.IfNot, 0.0f, IsLocalParameterName);
        }

        private static void AddDirectBlendTreeChild(BlendTree tree, Motion motion, string directBlendParameter)
        {
            ChildMotion child = new()
            {
                timeScale = 1.0f,
                motion = motion,
                directBlendParameter = directBlendParameter,
            };

            tree.children = AddToArray(tree.children, child);
            EditorUtility.SetDirty(tree);
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

        private static bool IsDescendantOf(Transform transform, Transform root)
        {
            for (Transform current = transform; current; current = current.parent)
            {
                if (current == root)
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetVRCFTParameterName(VRCFTParameter parameter)
        {
            return $"{ParameterPrefix}{parameter}";
        }

        private static string GetSmoothedParameterName(VRCFTParameter parameter)
        {
            return $"{LocalSmoothedParameterPrefix}{parameter}";
        }

        private static float GetDefaultValue(ParameterRangeKind range)
        {
            return range == ParameterRangeKind.EyeLid ? EyelidNeutralValue : 0.0f;
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

        private readonly struct BlendShapeBinding
        {
            public BlendShapeBinding(string blendShapeName, float maxValue)
            {
                BlendShapeName = blendShapeName;
                MaxValue = maxValue;
            }

            public string BlendShapeName { get; }
            public float MaxValue { get; }
        }

        private readonly struct TargetAnimation
        {
            public TargetAnimation(WeightCurveType curveType, string blendShapeName, float maxValue)
            {
                CurveType = curveType;
                BlendShapeName = blendShapeName;
                MaxValue = maxValue;
            }

            public WeightCurveType CurveType { get; }
            public string BlendShapeName { get; }
            public float MaxValue { get; }
        }

        private readonly struct TargetAnimationKey : IEquatable<TargetAnimationKey>
        {
            private readonly VRCFTParameter _parameter;
            private readonly UnifiedExpression _expression;
            private readonly WeightCurveType _curveType;
            private readonly string _blendShapeName;

            public TargetAnimationKey(VRCFTParameter parameter, UnifiedExpression expression, WeightCurveType curveType, string blendShapeName)
            {
                _parameter = parameter;
                _expression = expression;
                _curveType = curveType;
                _blendShapeName = blendShapeName;
            }

            public bool Equals(TargetAnimationKey other)
            {
                return _parameter == other._parameter && _expression == other._expression && _curveType == other._curveType && _blendShapeName == other._blendShapeName;
            }

            public override bool Equals(object obj)
            {
                return obj is TargetAnimationKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = (int)_parameter;
                    hashCode = (hashCode * 397) ^ (int)_expression;
                    hashCode = (hashCode * 397) ^ (int)_curveType;
                    hashCode = (hashCode * 397) ^ (_blendShapeName != null ? _blendShapeName.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }

        private sealed class ParameterAnimation
        {
            public ParameterAnimation(VRCFTParameter parameter, string parameterName, string rendererPath, float defaultValue)
            {
                Parameter = parameter;
                ParameterName = parameterName;
                SmoothedParameterName = GetSmoothedParameterName(parameter);
                RendererPath = rendererPath;
                DefaultValue = defaultValue;
            }

            public VRCFTParameter Parameter { get; }
            public string ParameterName { get; }
            public string SmoothedParameterName { get; }
            public string RendererPath { get; }
            public float DefaultValue { get; }
            public List<TargetAnimation> Targets { get; } = new();
        }
    }
}

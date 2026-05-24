using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace USTL.FaceTracking.Editor
{
    internal static class GenerateFaceTrackingProcess
    {
        private const string ParameterPrefix = "USTL/v2/";
        private const string AlwaysOneParameterName = "USTL_FT_AlwaysOne";
        private const string IsLocalParameterName = "IsLocal";
        private const string SmoothingParameterName = "USTL_FT_Smoothing";
        private const string SmoothedParameterPrefix = "USTL_FT_Smoothed_";
        private const string DecodedParameterPrefix = "USTL_FT_Decoded_";
        private const string BinaryNegativeParameterSuffix = "Negative";
        private const string BlendShapePropertyPrefix = "blendShape.";
        private const float EyelidNeutralValue = 0.75f;
        private const float DefaultSmoothing = 0.1f;

        internal static AnimatorController GenerateAnimatorController(string controllerName, USTLFaceTracking source, Transform avatarRoot, out List<ParameterAnimation> parameterAnimations)
        {
            AnimatorController controller = AnimatorControllerUtility.CreateAnimatorController(controllerName);
            AddBoolParameter(controller, IsLocalParameterName, true);
            AddFloatParameter(controller, AlwaysOneParameterName, 1.0f);
            AddFloatParameter(controller, SmoothingParameterName, DefaultSmoothing);

            AnimationClip emptyClip = CreateEmptyClip();

            parameterAnimations = CollectParameterAnimations(source, avatarRoot);
            if (parameterAnimations.Count == 0)
            {
                CreateEmptyLayer(controller, emptyClip);
                return controller;
            }

            foreach (ParameterAnimation parameterAnimation in parameterAnimations)
            {
                AddFloatParameter(controller, parameterAnimation.ParameterName, parameterAnimation.DefaultValue);
                AddFloatParameter(controller, parameterAnimation.SmoothedParameterName, parameterAnimation.DefaultValue);
                int binaryBitCount = GetBinaryBitCount(parameterAnimation.SyncMode);
                if (binaryBitCount > 0)
                {
                    AddFloatParameter(controller, parameterAnimation.RemoteDecodedParameterName, parameterAnimation.DefaultValue);
                    for (int bitIndex = 0; bitIndex < binaryBitCount; bitIndex++)
                    {
                        AddBoolParameter(controller, GetBinaryParameterName(parameterAnimation.Parameter, 1 << bitIndex), false);
                    }

                    if (parameterAnimation.Range == ParameterRangeKind.Signed)
                    {
                        AddBoolParameter(controller, GetBinaryNegativeParameterName(parameterAnimation.Parameter), false);
                    }
                }
            }

            CreateLocalUserLayers(controller, emptyClip, ref parameterAnimations);
            CreateRemoteUserLayers(controller, emptyClip, ref parameterAnimations);

            return controller;
        }

        private static void CreateEmptyLayer(AnimatorController controller, AnimationClip emptyClip)
        {
            AnimatorStateMachine stateMachine = new()
            {
                name = controller.MakeUniqueLayerName("USTL FaceTracking Empty"),
                hideFlags = HideFlags.HideInHierarchy,
            };
            AnimatorControllerLayer layer = new()
            {
                name = stateMachine.name,
                stateMachine = stateMachine,
                defaultWeight = 1.0f,
            };

            AnimatorControllerUtility.AddLayer(controller, layer);

            AnimatorState state = AnimatorControllerUtility.AddState(layer.stateMachine, "Empty");
            state.motion = emptyClip;
            state.writeDefaultValues = true;
            layer.stateMachine.defaultState = state;

            EditorUtility.SetDirty(state);
            EditorUtility.SetDirty(layer.stateMachine);
            EditorUtility.SetDirty(controller);
        }

        private static void CreateLocalUserLayers(AnimatorController controller, AnimationClip emptyClip, ref List<ParameterAnimation> parameterAnimations)
        {
            AnimatorStateMachine stateMachine = new()
            {
                name = controller.MakeUniqueLayerName("USTL FaceTracking Local Root"),
                hideFlags = HideFlags.HideInHierarchy,
            };
            AnimatorControllerLayer layer = new()
            {
                name = stateMachine.name,
                stateMachine = stateMachine,
                defaultWeight = 1.0f,
            };

            AnimatorControllerUtility.AddLayer(controller, layer);
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
            List<ParameterAnimation> syncedParameterAnimations = new();
            foreach (ParameterAnimation parameterAnimation in parameterAnimations)
            {
                if (!IsRemoteSyncMode(parameterAnimation.SyncMode))
                {
                    continue;
                }

                syncedParameterAnimations.Add(parameterAnimation);
                if (IsBinarySyncMode(parameterAnimation.SyncMode))
                {
                    CreateBinaryInputLayer(controller, emptyClip, parameterAnimation);
                }
            }

            if (syncedParameterAnimations.Count == 0)
            {
                return;
            }

            AnimatorStateMachine stateMachine = new()
            {
                name = controller.MakeUniqueLayerName("USTL FaceTracking Remote"),
                hideFlags = HideFlags.HideInHierarchy,
            };
            AnimatorControllerLayer layer = new()
            {
                name = stateMachine.name,
                stateMachine = stateMachine,
                defaultWeight = 1.0f,
            };
            AnimatorControllerUtility.AddLayer(controller, layer);

            EditorUtility.SetDirty(controller);

            BlendTree rootTree = new()
            {
                name = "USTL FaceTracking Remote Root",
                hideFlags = HideFlags.HideInHierarchy,
                blendType = BlendTreeType.Direct,
            };

            AnimatorState remoteState = AnimatorControllerUtility.AddState(layer.stateMachine, "Remote");
            remoteState.motion = rootTree;
            remoteState.writeDefaultValues = true;

            AnimatorState localState = AnimatorControllerUtility.AddState(layer.stateMachine, "Local");
            localState.motion = emptyClip;
            localState.writeDefaultValues = true;

            layer.stateMachine.defaultState = remoteState;

            EditorUtility.SetDirty(rootTree);
            EditorUtility.SetDirty(remoteState);
            EditorUtility.SetDirty(localState);
            EditorUtility.SetDirty(layer.stateMachine);

            AddIsLocalTransitions(remoteState, localState);

            foreach (ParameterAnimation parameterAnimation in syncedParameterAnimations)
            {
                BlendTree smoothingTree = CreateParameterSmoothingBlendTree(parameterAnimation, GetRemoteInputParameterName(parameterAnimation));
                AddDirectBlendTreeChild(rootTree, smoothingTree, AlwaysOneParameterName);

                BlendTree parameterTree = CreateParameterBlendTree(parameterAnimation);
                AddDirectBlendTreeChild(rootTree, parameterTree, AlwaysOneParameterName);
            }
        }

        private static void CreateBinaryInputLayer(AnimatorController controller, AnimationClip emptyClip, ParameterAnimation parameterAnimation)
        {
            int bitCount = GetBinaryBitCount(parameterAnimation.SyncMode);
            if (bitCount <= 0)
            {
                return;
            }

            AnimatorStateMachine stateMachine = new()
            {
                name = controller.MakeUniqueLayerName($"USTL FaceTracking {parameterAnimation.Parameter} Binary"),
                hideFlags = HideFlags.HideInHierarchy,
            };
            AnimatorControllerLayer layer = new()
            {
                name = stateMachine.name,
                stateMachine = stateMachine,
                defaultWeight = 1.0f,
            };

            AnimatorControllerUtility.AddLayer(controller, layer);
            EditorUtility.SetDirty(controller);

            AnimatorState localState = AnimatorControllerUtility.AddState(layer.stateMachine, "Local");
            localState.motion = emptyClip;
            localState.writeDefaultValues = true;
            layer.stateMachine.defaultState = localState;
            AddBinaryLocalTransition(layer.stateMachine, localState);

            bool signed = parameterAnimation.Range == ParameterRangeKind.Signed;
            int magnitudeCount = 1 << bitCount;
            int signCount = signed ? 2 : 1;
            for (int signIndex = 0; signIndex < signCount; signIndex++)
            {
                bool negative = signIndex > 0;
                for (int magnitude = 0; magnitude < magnitudeCount; magnitude++)
                {
                    float value = DecodeBinaryValue(magnitude, bitCount, negative);
                    AnimatorState state = AnimatorControllerUtility.AddState(layer.stateMachine, CreateBinaryStateName(magnitude, negative));
                    state.motion = CreateAnimatorParameterConstantClip(parameterAnimation.RemoteDecodedParameterName, value, $"{parameterAnimation.Parameter} Binary {value:0.###}");
                    state.writeDefaultValues = true;

                    AddBinaryRemoteTransition(layer.stateMachine, state, parameterAnimation, magnitude, bitCount, signed, negative);
                    EditorUtility.SetDirty(state);
                }
            }

            EditorUtility.SetDirty(localState);
            EditorUtility.SetDirty(layer.stateMachine);
        }

        private static void AddBinaryLocalTransition(AnimatorStateMachine stateMachine, AnimatorState localState)
        {
            AnimatorStateTransition toLocal = AnimatorControllerUtility.AddAnyStateTransition(stateMachine, localState);
            ConfigureInstantTransition(toLocal);
            toLocal.canTransitionToSelf = false;
            AnimatorControllerUtility.AddCondition(toLocal, AnimatorConditionMode.If, 0.0f, IsLocalParameterName);
        }

        private static void AddBinaryRemoteTransition(AnimatorStateMachine stateMachine, AnimatorState destinationState, ParameterAnimation parameterAnimation, int magnitude, int bitCount, bool signed, bool negative)
        {
            AnimatorStateTransition transition = AnimatorControllerUtility.AddAnyStateTransition(stateMachine, destinationState);
            ConfigureInstantTransition(transition);
            transition.canTransitionToSelf = false;
            AnimatorControllerUtility.AddCondition(transition, AnimatorConditionMode.IfNot, 0.0f, IsLocalParameterName);

            for (int bitIndex = 0; bitIndex < bitCount; bitIndex++)
            {
                int bitValue = 1 << bitIndex;
                AnimatorConditionMode mode = (magnitude & bitValue) != 0 ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot;
                AnimatorControllerUtility.AddCondition(transition, mode, 0.0f, GetBinaryParameterName(parameterAnimation.Parameter, bitValue));
            }

            if (signed)
            {
                AnimatorConditionMode mode = negative ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot;
                AnimatorControllerUtility.AddCondition(transition, mode, 0.0f, GetBinaryNegativeParameterName(parameterAnimation.Parameter));
            }
        }

        private static string CreateBinaryStateName(int magnitude, bool negative)
        {
            return negative ? $"Remote -{magnitude}" : $"Remote {magnitude}";
        }

        private static float DecodeBinaryValue(int magnitude, int bitCount, bool negative)
        {
            int maxMagnitude = (1 << bitCount) - 1;
            float value = maxMagnitude <= 0 ? 0.0f : magnitude / (float)maxMagnitude;
            return negative ? -value : value;
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

                        if (!parameterAnimationMap.TryGetValue(parameter, out ParameterAnimation parameterAnimation))
                        {
                            parameterAnimation = new ParameterAnimation(parameter, GetVRCFTParameterName(parameter), rendererPath, parameterDefinition.Range, GetDefaultValue(parameterDefinition.Range));
                            parameterAnimationMap[parameter] = parameterAnimation;
                            parameterAnimations.Add(parameterAnimation);
                        }

                        parameterAnimation.MergeSyncMode(featureSetting.syncMode);

                        TargetAnimationKey targetKey = new(parameter, target.Expression, target.Type, binding.BlendShapeName);
                        if (!targetKeys.Add(targetKey))
                        {
                            continue;
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
            return CreateParameterSmoothingBlendTree(parameterAnimation, parameterAnimation.ParameterName);
        }

        private static BlendTree CreateParameterSmoothingBlendTree(ParameterAnimation parameterAnimation, string inputParameterName)
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

            BlendTree inputTree = CreateSmoothingSourceTree(inputParameterName, parameterAnimation);
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

        private static AnimationClip CreateAnimatorParameterConstantClip(string parameterName, float value, string clipName)
        {
            AnimationClip clip = new()
            {
                name = clipName,
                hideFlags = HideFlags.HideInHierarchy,
            };

            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), parameterName), CreateConstantCurve(value));
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

        internal static void AddBoolParameter(AnimatorController controller, string name, bool defaultValue)
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

        private static void AddIsLocalTransitions(AnimatorState remoteState, AnimatorState localState)
        {
            AnimatorStateTransition toLocal = AnimatorControllerUtility.AddTransition(remoteState, localState);
            ConfigureInstantTransition(toLocal);
            AnimatorControllerUtility.AddCondition(toLocal, AnimatorConditionMode.If, 0.0f, IsLocalParameterName);
            AnimatorControllerUtility.AddCondition(toLocal, AnimatorConditionMode.If, 0.0f, IsLocalParameterName);

            AnimatorStateTransition toRemote = AnimatorControllerUtility.AddTransition(localState, remoteState);
            ConfigureInstantTransition(toRemote);
            AnimatorControllerUtility.AddCondition(toRemote, AnimatorConditionMode.IfNot, 0.0f, IsLocalParameterName);
        }

        private static void ConfigureInstantTransition(AnimatorStateTransition transition)
        {
            transition.hasExitTime = false;
            transition.hasFixedDuration = true;
            transition.duration = 0.0f;
            transition.exitTime = 0.0f;
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

        internal static string GetVRCFTParameterName(VRCFTParameter parameter)
        {
            return $"{ParameterPrefix}{parameter}";
        }

        internal static string GetBinaryParameterName(VRCFTParameter parameter, int bitValue)
        {
            return $"{GetVRCFTParameterName(parameter)}{bitValue}";
        }

        internal static string GetBinaryNegativeParameterName(VRCFTParameter parameter)
        {
            return $"{GetVRCFTParameterName(parameter)}{BinaryNegativeParameterSuffix}";
        }

        private static string GetSmoothedParameterName(VRCFTParameter parameter)
        {
            return $"{SmoothedParameterPrefix}{parameter}";
        }

        private static string GetRemoteDecodedParameterName(VRCFTParameter parameter)
        {
            return $"{DecodedParameterPrefix}{parameter}";
        }

        private static string GetRemoteInputParameterName(ParameterAnimation parameterAnimation)
        {
            return IsBinarySyncMode(parameterAnimation.SyncMode) ? parameterAnimation.RemoteDecodedParameterName : parameterAnimation.ParameterName;
        }

        private static float GetDefaultValue(ParameterRangeKind range)
        {
            return range == ParameterRangeKind.EyeLid ? EyelidNeutralValue : 0.0f;
        }

        private static bool IsRemoteSyncMode(ParameterSyncMode syncMode)
        {
            return syncMode != ParameterSyncMode.None && syncMode != ParameterSyncMode.LocalOnly;
        }

        private static bool IsBinarySyncMode(ParameterSyncMode syncMode)
        {
            return GetBinaryBitCount(syncMode) > 0;
        }

        internal static int GetBinaryBitCount(ParameterSyncMode syncMode)
        {
            return syncMode switch
            {
                ParameterSyncMode.Binary1Bit => 1,
                ParameterSyncMode.Binary2Bit => 2,
                ParameterSyncMode.Binary3Bit => 3,
                ParameterSyncMode.Binary4Bit => 4,
                _ => 0,
            };
        }

        private static ParameterSyncMode MergeSyncMode(ParameterSyncMode currentSyncMode, ParameterSyncMode nextSyncMode)
        {
            if (!IsRemoteSyncMode(nextSyncMode))
            {
                return currentSyncMode;
            }

            if (currentSyncMode == ParameterSyncMode.Float8 || nextSyncMode == ParameterSyncMode.Float8)
            {
                return ParameterSyncMode.Float8;
            }

            return GetBinaryBitCount(nextSyncMode) > GetBinaryBitCount(currentSyncMode) ? nextSyncMode : currentSyncMode;
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

        internal readonly struct TargetAnimation
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

        internal sealed class ParameterAnimation
        {
            public ParameterAnimation(VRCFTParameter parameter, string parameterName, string rendererPath, ParameterRangeKind range, float defaultValue)
            {
                Parameter = parameter;
                ParameterName = parameterName;
                SmoothedParameterName = GetSmoothedParameterName(parameter);
                RemoteDecodedParameterName = GetRemoteDecodedParameterName(parameter);
                RendererPath = rendererPath;
                Range = range;
                DefaultValue = defaultValue;
                SyncMode = ParameterSyncMode.LocalOnly;
            }

            public VRCFTParameter Parameter { get; }
            public string ParameterName { get; }
            public string SmoothedParameterName { get; }
            public string RemoteDecodedParameterName { get; }
            public string RendererPath { get; }
            public ParameterRangeKind Range { get; }
            public float DefaultValue { get; }
            public ParameterSyncMode SyncMode { get; private set; }
            public List<TargetAnimation> Targets { get; } = new();

            public void MergeSyncMode(ParameterSyncMode syncMode)
            {
                SyncMode = GenerateFaceTrackingProcess.MergeSyncMode(SyncMode, syncMode);
            }
        }
    }
}

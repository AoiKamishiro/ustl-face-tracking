using System.Collections.Generic;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace USTL.FaceTracking.Editor
{
    internal static class FaceTrackingAnimatorControllerGenerator
    {
        internal static void Populate(BuildContext context, USTLFaceTracking source, AnimatorController controller, IReadOnlyList<SelectedParameterSetting> selectedParameters, List<Object> generatedAssets)
        {
            if (!source.faceMeshRenderer)
            {
                Debug.LogWarning("U-Stella FaceTracking requires a face SkinnedMeshRenderer.", source);
                return;
            }

            Mesh faceMesh = source.faceMeshRenderer.sharedMesh;
            if (!faceMesh)
            {
                Debug.LogWarning("U-Stella FaceTracking face mesh renderer has no mesh.", source.faceMeshRenderer);
                return;
            }

            string rendererPath = AnimationUtility.CalculateTransformPath(source.faceMeshRenderer.transform, context.AvatarRootTransform);
            Dictionary<UnifiedExpression, BlendShapeAssignment> assignments = FaceTrackingBlendShapeTargetResolver.BuildAssignmentMap(source.blendShapeAssignments);

            foreach (SelectedParameterSetting setting in selectedParameters)
            {
                if (!VRCFTParameterDefinition.All.TryGetValue(setting.Parameter, out VRCFTParameterDefinition definition))
                {
                    continue;
                }

                List<BlendShapeTarget> targets = FaceTrackingBlendShapeTargetResolver.BuildBlendShapeTargets(definition, assignments, faceMesh);
                if (targets.Count == 0)
                {
                    continue;
                }

                if (ParameterSyncModeUtility.IsBinary(setting.SyncMode))
                {
                    CreateBinaryParameterLayer(controller, setting, definition, targets, rendererPath, generatedAssets);
                    continue;
                }

                CreateFloatParameterLayer(controller, setting.Parameter, definition, targets, rendererPath, generatedAssets);
            }
        }

        internal static void CreateFloatParameterLayer(AnimatorController controller, VRCFTParameter parameter, VRCFTParameterDefinition definition, IReadOnlyList<BlendShapeTarget> targets, string rendererPath, List<Object> generatedAssets)
        {
            string parameterName = FaceTrackingGeneratedParameterNames.FormatFloat(parameter);
            AddAnimatorParameterIfNeeded(controller, parameterName, AnimatorControllerParameterType.Float, FaceTrackingParameterDefaults.GetDefaultValue(definition.Range));

            int layerIndex = AddLayer(controller, $"FT {parameter}");
            AnimatorState state = AnimatorControllerNoUndoUtility.CreateBlendTreeInController(controller, $"{parameter} Blend", out BlendTree tree, layerIndex);
            generatedAssets.Add(controller.layers[layerIndex].stateMachine);
            generatedAssets.Add(state);
            generatedAssets.Add(tree);

            ConfigureBlendTree(tree, parameterName, definition.Range);
            SetDefaultState(controller.layers[layerIndex].stateMachine, state);

            foreach (ParameterSample sample in GetFloatSamples(definition.Range))
            {
                AnimationClip clip = BlendShapeAnimationClipFactory.Create(controller, $"{parameter} {sample.Name}", rendererPath, targets, sample.Value);
                generatedAssets.Add(clip);
                AnimatorControllerNoUndoUtility.AddBlendTreeChild(tree, clip, sample.Value);
            }
        }

        internal static void CreateBinaryParameterLayer(AnimatorController controller, SelectedParameterSetting setting, VRCFTParameterDefinition definition, IReadOnlyList<BlendShapeTarget> targets, string rendererPath, List<Object> generatedAssets)
        {
            int bitCount = ParameterSyncModeUtility.GetBinaryBitCount(setting.SyncMode);
            float defaultValue = FaceTrackingParameterDefaults.GetDefaultValue(definition.Range);
            int defaultMagnitude = BinaryParameterEncoding.GetMagnitude(defaultValue, bitCount);
            bool defaultNegative = defaultMagnitude > 0 && defaultValue < 0f;
            string[] bitParameterNames = new string[bitCount];
            for (int i = 0; i < bitCount; i++)
            {
                string bitParameterName = FaceTrackingGeneratedParameterNames.FormatBinaryBit(setting.Parameter, 1 << i);
                bitParameterNames[i] = bitParameterName;
                AddAnimatorParameterIfNeeded(controller, bitParameterName, AnimatorControllerParameterType.Bool, defaultBool: (defaultMagnitude & (1 << i)) != 0);
            }

            bool isSigned = definition.Range == ParameterRangeKind.Signed;
            string negativeParameterName = FaceTrackingGeneratedParameterNames.FormatBinaryNegative(setting.Parameter);
            if (isSigned)
            {
                AddAnimatorParameterIfNeeded(controller, negativeParameterName, AnimatorControllerParameterType.Bool, defaultBool: defaultNegative);
            }

            int layerIndex = AddLayer(controller, $"FT {setting.Parameter} Binary");
            AnimatorStateMachine stateMachine = controller.layers[layerIndex].stateMachine;
            generatedAssets.Add(stateMachine);

            AnimatorState neutralState = CreateBinaryState(controller, layerIndex, $"{setting.Parameter} Binary 0", rendererPath, targets, 0f, generatedAssets);
            AnimatorState defaultState = neutralState;
            AddBinaryTransition(stateMachine, neutralState, bitParameterNames, 0, false, false, negativeParameterName);

            int valueCount = 1 << bitCount;
            for (int magnitude = 1; magnitude < valueCount; magnitude++)
            {
                float normalizedValue = magnitude / (float)(valueCount - 1);
                AnimatorState positiveState = CreateBinaryState(controller, layerIndex, $"{setting.Parameter} Binary {magnitude}", rendererPath, targets, normalizedValue, generatedAssets);
                AddBinaryTransition(stateMachine, positiveState, bitParameterNames, magnitude, isSigned, false, negativeParameterName);

                if (magnitude == defaultMagnitude && !defaultNegative)
                {
                    defaultState = positiveState;
                }

                if (!isSigned)
                {
                    continue;
                }

                AnimatorState negativeState = CreateBinaryState(controller, layerIndex, $"{setting.Parameter} Binary -{magnitude}", rendererPath, targets, -normalizedValue, generatedAssets);
                AddBinaryTransition(stateMachine, negativeState, bitParameterNames, magnitude, true, true, negativeParameterName);

                if (magnitude == defaultMagnitude && defaultNegative)
                {
                    defaultState = negativeState;
                }
            }

            SetDefaultState(stateMachine, defaultState);
        }

        private static AnimatorState CreateBinaryState(AnimatorController controller, int layerIndex, string name, string rendererPath, IReadOnlyList<BlendShapeTarget> targets, float parameterValue, List<Object> generatedAssets)
        {
            AnimationClip clip = BlendShapeAnimationClipFactory.Create(controller, name, rendererPath, targets, parameterValue);
            generatedAssets.Add(clip);

            AnimatorState state = AnimatorControllerNoUndoUtility.AddMotion(controller, clip, layerIndex);
            generatedAssets.Add(state);
            return state;
        }

        private static void AddBinaryTransition(AnimatorStateMachine stateMachine, AnimatorState state, IReadOnlyList<string> bitParameterNames, int magnitude, bool includeSignCondition, bool negative, string negativeParameterName)
        {
            AnimatorStateTransition transition = AnimatorControllerNoUndoUtility.AddAnyStateTransition(stateMachine, state);
            transition.canTransitionToSelf = false;

            for (int i = 0; i < bitParameterNames.Count; i++)
            {
                bool bitIsSet = (magnitude & (1 << i)) != 0;
                AnimatorControllerNoUndoUtility.AddCondition(transition, bitIsSet ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, bitParameterNames[i]);
            }

            if (includeSignCondition)
            {
                AnimatorControllerNoUndoUtility.AddCondition(transition, negative ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, negativeParameterName);
            }

            EditorUtility.SetDirty(transition);
        }

        private static void ConfigureBlendTree(BlendTree tree, string parameterName, ParameterRangeKind range)
        {
            ParameterSample[] samples = GetFloatSamples(range);
            tree.blendType = BlendTreeType.Simple1D;
            tree.blendParameter = parameterName;
            tree.blendParameterY = parameterName;
            tree.useAutomaticThresholds = false;
            tree.minThreshold = samples[0].Value;
            tree.maxThreshold = samples[^1].Value;
            EditorUtility.SetDirty(tree);
        }

        private static ParameterSample[] GetFloatSamples(ParameterRangeKind range)
        {
            return range switch
            {
                ParameterRangeKind.Signed => new[]
                {
                    new ParameterSample(-1f, "Negative"),
                    new ParameterSample(0f, "Neutral"),
                    new ParameterSample(1f, "Positive"),
                },
                ParameterRangeKind.EyeLid => new[]
                {
                    new ParameterSample(0f, "Closed"),
                    new ParameterSample(FaceTrackingGenerationConstants.EyelidNeutralValue, "Neutral"),
                    new ParameterSample(1f, "Wide"),
                },
                _ => new[]
                {
                    new ParameterSample(0f, "Neutral"),
                    new ParameterSample(1f, "Active"),
                },
            };
        }

        private static int AddLayer(AnimatorController controller, string name)
        {
            AnimatorControllerNoUndoUtility.AddLayer(controller, name);
            AnimatorControllerLayer[] layers = controller.layers;
            int layerIndex = layers.Length - 1;
            layers[layerIndex].defaultWeight = 1f;
            controller.layers = layers;
            EditorUtility.SetDirty(controller);
            return layerIndex;
        }

        private static void SetDefaultState(AnimatorStateMachine stateMachine, AnimatorState state)
        {
            stateMachine.defaultState = state;
            EditorUtility.SetDirty(stateMachine);
        }

        private static void AddAnimatorParameterIfNeeded(AnimatorController controller, string parameterName, AnimatorControllerParameterType type, float defaultFloat = 0f, bool defaultBool = false)
        {
            foreach (AnimatorControllerParameter parameter in controller.parameters)
            {
                if (parameter.name == parameterName)
                {
                    if (parameter.type == type)
                    {
                        parameter.defaultFloat = defaultFloat;
                        parameter.defaultBool = defaultBool;
                        EditorUtility.SetDirty(controller);
                    }

                    return;
                }
            }

            AnimatorControllerNoUndoUtility.AddParameter(controller, new AnimatorControllerParameter
            {
                name = parameterName,
                type = type,
                defaultFloat = defaultFloat,
                defaultBool = defaultBool,
            });
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace USTL.FaceTracking.Editor
{
    public static class VRCParameterUtility
    {
        internal static int CalculateUsage(USTLFaceTracking faceTracking)
        {
            if (!faceTracking || !faceTracking.faceMeshRenderer || !faceTracking.faceMeshRenderer.sharedMesh)
            {
                return 0;
            }

            HashSet<UnifiedExpression> generatedExpressions = GetGeneratedExpressions(faceTracking);
            if (generatedExpressions.Count == 0)
            {
                return 0;
            }

            Dictionary<VRCFTParameter, ParameterSyncMode> syncModesByParameter = new();

            foreach (FeatureSetting setting in faceTracking.featureSettings ?? Array.Empty<FeatureSetting>())
            {
                if (setting == null || setting.syncMode == ParameterSyncMode.None ||
                    !FaceTrackingFeatureDefinition.All.TryGetValue(setting.feature, out FaceTrackingFeatureDefinition definition))
                {
                    continue;
                }

                VRCFTParameterSet parameterSet = definition.GetOutputFormatOrDefault(setting.outputFormatId);
                if (parameterSet == null)
                {
                    continue;
                }

                foreach (VRCFTParameter parameter in parameterSet.Parameters)
                {
                    if (!VRCFTParameterDefinition.All.TryGetValue(parameter, out VRCFTParameterDefinition parameterDefinition) ||
                        !HasGeneratedTarget(parameterDefinition, generatedExpressions))
                    {
                        continue;
                    }

                    if (!syncModesByParameter.TryGetValue(parameter, out ParameterSyncMode currentSyncMode))
                    {
                        syncModesByParameter[parameter] = setting.syncMode;
                        continue;
                    }

                    syncModesByParameter[parameter] = AnimatorGenerator.MergeSyncMode(currentSyncMode, setting.syncMode);
                }
            }

            int totalUsage = 0;
            foreach (KeyValuePair<VRCFTParameter, ParameterSyncMode> entry in syncModesByParameter)
            {
                if (entry.Value == ParameterSyncMode.Float8)
                {
                    totalUsage += 8;
                    continue;
                }

                int binaryBitCount = AnimatorGenerator.GetBinaryBitCount(entry.Value);
                if (binaryBitCount <= 0)
                {
                    continue;
                }

                totalUsage += binaryBitCount;
                if (VRCFTParameterDefinition.All.TryGetValue(entry.Key, out VRCFTParameterDefinition definition) &&
                    definition.Range == ParameterRangeKind.Signed)
                {
                    totalUsage++;
                }
            }

            return totalUsage;
        }

        private static HashSet<UnifiedExpression> GetGeneratedExpressions(USTLFaceTracking faceTracking)
        {
            HashSet<UnifiedExpression> expressions = new();
            Mesh mesh = faceTracking.faceMeshRenderer.sharedMesh;

            foreach (BlendShapeSetting setting in faceTracking.blendShapeSettings ?? Array.Empty<BlendShapeSetting>())
            {
                if (setting == null || setting.expression == UnifiedExpression.None ||
                    string.IsNullOrWhiteSpace(setting.blendShapeName) || setting.maxValue <= 0.0f ||
                    mesh.GetBlendShapeIndex(setting.blendShapeName) < 0)
                {
                    continue;
                }

                expressions.Add(setting.expression);
            }

            return expressions;
        }

        private static bool HasGeneratedTarget(VRCFTParameterDefinition definition, HashSet<UnifiedExpression> generatedExpressions)
        {
            foreach (ExpressionWeightTarget target in definition.ExpressionTargets)
            {
                if (target.Expression != UnifiedExpression.None && generatedExpressions.Contains(target.Expression))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

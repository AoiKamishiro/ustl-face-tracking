using System.Collections.Generic;
using UnityEditor;

namespace USTL.FaceTracking.Editor
{
    internal sealed partial class USTLFaceTrackingEditor
    {
        private OutputFormatExpressionUsageResult GetOutputFormatExpressionUsage(UnifiedExpression expression)
        {
            List<VRCFTParameter> usedParameters = new();
            IReadOnlyList<VRCFTParameter> expressionParameters = HardwareSupportDisplay.GetExpressionParameters(expression);
            SerializedProperty featureSettings = serializedObject.FindProperty(nameof(USTLFaceTracking.featureSettings));
            if (featureSettings == null || expressionParameters.Count == 0)
            {
                return new OutputFormatExpressionUsageResult(OutputFormatUsageStatus.NotEmitted, usedParameters);
            }

            for (int i = 0; i < featureSettings.arraySize; i++)
            {
                SerializedProperty featureSetting = featureSettings.GetArrayElementAtIndex(i);
                SerializedProperty syncModeProperty = featureSetting.FindPropertyRelative(nameof(FaceTrackingFeatureSetting.syncMode));
                if ((ParameterSyncMode)syncModeProperty.intValue == ParameterSyncMode.None)
                {
                    continue;
                }

                FaceTrackingFeatureDefinition featureDefinition = GetFeatureDefinition(featureSetting);
                int outputFormatIndex = GetOutputFormatIndex(featureSetting, featureDefinition);
                VRCFTParameterOutputFormat outputFormat = GetOutputFormat(featureDefinition, outputFormatIndex);
                if (outputFormat == null)
                {
                    continue;
                }

                foreach (VRCFTParameter parameter in outputFormat.Parameters)
                {
                    if (ContainsParameter(expressionParameters, parameter) && !usedParameters.Contains(parameter))
                    {
                        usedParameters.Add(parameter);
                    }
                }
            }

            return new OutputFormatExpressionUsageResult(usedParameters.Count > 0 ? OutputFormatUsageStatus.Emitted : OutputFormatUsageStatus.NotEmitted, usedParameters);
        }

        private static bool ContainsParameter(IReadOnlyList<VRCFTParameter> parameters, VRCFTParameter parameter)
        {
            foreach (VRCFTParameter candidate in parameters)
            {
                if (candidate == parameter)
                {
                    return true;
                }
            }

            return false;
        }

        private static string FormatOutputFormatUsageTooltip(UnifiedExpression expression, OutputFormatExpressionUsageResult usage)
        {
            string statusText = OutputFormatUsageDisplay.FormatStatus(usage.Status);
            return $"{statusText}\n\n{OutputFormatUsageDisplay.FormatTooltip(expression, usage)}";
        }
    }
}

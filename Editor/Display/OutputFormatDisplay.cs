using System.Collections.Generic;
using UnityEditor;

namespace USTL.FaceTracking.Editor
{
    internal static class OutputFormatDisplay
    {
        internal static string FormatFeatureName(SerializedProperty element, FaceTrackingFeatureDefinition featureDefinition)
        {
            if (featureDefinition != null)
            {
                return FaceTrackingEditorText.Get($"feature.{featureDefinition.Feature}", featureDefinition.DisplayName);
            }

            SerializedProperty featureProperty = element.FindPropertyRelative(nameof(FaceTrackingFeatureSetting.feature));
            return $"{(FaceTrackingFeature)featureProperty.intValue}";
        }

        internal static List<string> GetChoices(FaceTrackingFeatureDefinition featureDefinition)
        {
            List<string> choices = new();
            if (featureDefinition == null)
            {
                return choices;
            }

            foreach (VRCFTParameterOutputFormat outputFormat in featureDefinition.OutputFormats)
            {
                choices.Add(outputFormat.DisplayName);
            }

            return choices;
        }

        internal static string GetValue(FaceTrackingFeatureDefinition featureDefinition, int outputFormatIndex)
        {
            if (featureDefinition == null || featureDefinition.OutputFormats.Length == 0)
            {
                return string.Empty;
            }

            return featureDefinition.OutputFormats[ClampIndex(featureDefinition, outputFormatIndex)].DisplayName;
        }

        internal static string FormatChoice(FaceTrackingFeatureDefinition featureDefinition, string value)
        {
            int outputFormatIndex = FindIndex(featureDefinition, value);
            return outputFormatIndex >= 0 ? GetDisplayName(featureDefinition, outputFormatIndex) : value;
        }

        internal static string FormatTooltip(FaceTrackingFeatureDefinition featureDefinition, int outputFormatIndex)
        {
            if (featureDefinition == null || featureDefinition.OutputFormats.Length == 0)
            {
                return string.Empty;
            }

            VRCFTParameterOutputFormat outputFormat = featureDefinition.OutputFormats[ClampIndex(featureDefinition, outputFormatIndex)];
            string displayName = GetDisplayName(featureDefinition, outputFormatIndex);
            string description = GetDescription(featureDefinition, outputFormatIndex, outputFormat);
            if (!string.IsNullOrWhiteSpace(description))
            {
                return $"{displayName}\n\n{description}";
            }

            if (outputFormat.Parameters.Length == 0)
            {
                return $"{displayName}\n\n{FaceTrackingEditorText.Get("tooltip.no_generated_parameters", "No avatar parameters or FX layers are generated for this output format.")}";
            }

            string parametersLabel = FaceTrackingEditorText.Get("tooltip.parameters", "Generated Parameters");
            return $"{displayName}\n\n{parametersLabel}: \n{string.Join(", ", outputFormat.Parameters)}";
        }

        internal static int FindIndex(FaceTrackingFeatureDefinition featureDefinition, string displayName)
        {
            if (featureDefinition == null)
            {
                return -1;
            }

            for (int i = 0; i < featureDefinition.OutputFormats.Length; i++)
            {
                if (featureDefinition.OutputFormats[i].DisplayName == displayName || GetDisplayName(featureDefinition, i) == displayName)
                {
                    return i;
                }
            }

            return -1;
        }

        internal static int ClampIndex(FaceTrackingFeatureDefinition featureDefinition, int outputFormatIndex)
        {
            return VRCFTParameterOutputFormatUtility.ClampIndex(featureDefinition, outputFormatIndex);
        }

        private static string GetDisplayName(FaceTrackingFeatureDefinition featureDefinition, int outputFormatIndex)
        {
            if (featureDefinition == null || featureDefinition.OutputFormats.Length == 0)
            {
                return string.Empty;
            }

            int clampedIndex = ClampIndex(featureDefinition, outputFormatIndex);
            return FaceTrackingEditorText.Get($"output_format.{featureDefinition.Feature}.{clampedIndex}", featureDefinition.OutputFormats[clampedIndex].DisplayName);
        }

        private static string GetDescription(FaceTrackingFeatureDefinition featureDefinition, int outputFormatIndex, VRCFTParameterOutputFormat outputFormat)
        {
            if (featureDefinition == null || outputFormat == null)
            {
                return string.Empty;
            }

            int clampedIndex = ClampIndex(featureDefinition, outputFormatIndex);
            return FaceTrackingEditorText.Get($"output_format.{featureDefinition.Feature}.{clampedIndex}.tooltip", outputFormat.Description ?? string.Empty);
        }
    }
}

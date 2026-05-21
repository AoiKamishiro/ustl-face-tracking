using System;

namespace USTL.FaceTracking
{
    internal static class VRCFTParameterOutputFormatUtility
    {
        internal static int ClampIndex(FaceTrackingFeatureDefinition featureDefinition, int outputFormatIndex)
        {
            int maxOutputFormatIndex = featureDefinition == null ? -1 : featureDefinition.OutputFormats.Count - 1;
            return maxOutputFormatIndex <= 0 ? 0 : Math.Max(0, Math.Min(outputFormatIndex, maxOutputFormatIndex));
        }

        internal static bool UsesGeneratedParameters(VRCFTParameterOutputFormat outputFormat)
        {
            return outputFormat == null || outputFormat.Parameters.Length > 0;
        }
    }
}

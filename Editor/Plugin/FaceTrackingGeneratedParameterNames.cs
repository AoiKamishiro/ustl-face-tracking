namespace USTL.FaceTracking.Editor
{
    internal static class FaceTrackingGeneratedParameterNames
    {
        internal static string FormatFloat(VRCFTParameter parameter)
        {
            return $"{FaceTrackingGenerationConstants.VRCFTParameterPrefix}{parameter}";
        }

        internal static string FormatBinaryBit(VRCFTParameter parameter, int bitValue)
        {
            return $"{FormatFloat(parameter)}{bitValue}";
        }

        internal static string FormatBinaryNegative(VRCFTParameter parameter)
        {
            return $"{FormatFloat(parameter)}Negative";
        }
    }
}

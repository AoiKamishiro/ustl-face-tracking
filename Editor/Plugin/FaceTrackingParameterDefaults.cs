namespace USTL.FaceTracking.Editor
{
    internal static class FaceTrackingParameterDefaults
    {
        internal static float GetDefaultValue(ParameterRangeKind range)
        {
            return range switch
            {
                ParameterRangeKind.EyeLid => FaceTrackingGenerationConstants.EyelidNeutralValue,
                _ => 0f,
            };
        }

        internal static float GetDefaultValue(VRCFTParameter parameter)
        {
            return VRCFTParameterDefinition.All.TryGetValue(parameter, out VRCFTParameterDefinition definition) ? GetDefaultValue(definition.Range) : 0f;
        }
    }
}

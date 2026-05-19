using System.Collections.Generic;

namespace USTL.FaceTracking.Editor
{
    internal static class SelectedParameterCollector
    {
        internal static List<SelectedParameterSetting> Collect(USTLFaceTracking source)
        {
            List<SelectedParameterSetting> parameters = new();
            HashSet<VRCFTParameter> visited = new();

            if (source.featureSettings == null)
            {
                return parameters;
            }

            foreach (FaceTrackingFeatureSetting setting in source.featureSettings)
            {
                if (setting == null || setting.syncMode == ParameterSyncMode.None || !FaceTrackingFeatureDefinition.All.TryGetValue(setting.feature, out FaceTrackingFeatureDefinition feature) || feature.OutputFormats.Length == 0)
                {
                    continue;
                }

                VRCFTParameterOutputFormat outputFormat = feature.OutputFormats[VRCFTParameterOutputFormatUtility.ClampIndex(feature, setting.outputFormatIndex)];
                foreach (VRCFTParameter parameter in outputFormat.Parameters)
                {
                    if (visited.Add(parameter))
                    {
                        parameters.Add(new SelectedParameterSetting(parameter, setting.syncMode));
                    }
                }
            }

            return parameters;
        }
    }
}

using System.Collections.Generic;

namespace USTL.FaceTracking.Editor
{
    public static class VRCParameterUtility
    {
        internal static int CalculateUsage(USTLFaceTracking faceTracking)
        {
            int totalUsage = 0;

            foreach (FeatureSetting setting in faceTracking.featureSettings)
            {
                IReadOnlyList<VRCFTParameter> parameters = new List<VRCFTParameter>();
                foreach (VRCFTParameterSet set in FaceTrackingFeatureDefinition.All[setting.feature].OutputFormats)
                {
                    if (set.Id == setting.outputFormatId)
                    {
                        parameters = set.Parameters;
                        break;
                    }
                }

                HashSet<UnifiedExpression> expressions = new();
                foreach (VRCFTParameter parameter in parameters)
                {
                    foreach (ExpressionWeightTarget target in VRCFTParameterDefinition.All[parameter].ExpressionTargets)
                    {
                        expressions.Add(target.Expression);
                    }
                }

                int ratio = setting.syncMode switch
                {
                    ParameterSyncMode.None => 0,
                    ParameterSyncMode.LocalOnly => 0,
                    ParameterSyncMode.Binary1Bit => 1,
                    ParameterSyncMode.Binary2Bit => 2,
                    ParameterSyncMode.Binary3Bit => 3,
                    ParameterSyncMode.Binary4Bit => 4,
                    ParameterSyncMode.Float8 => 8,
                    _ => 0,
                };

                totalUsage += expressions.Count * ratio;
            }

            return totalUsage;
        }
    }
}

using System;
using System.Collections.Generic;

namespace USTL.FaceTracking.Editor
{
    internal static class FaceTrackingEditorUtility
    {
        private static List<FaceTrackingFeature> _allFeatures;
        private static List<ParameterSyncMode> _allSyncModes;
        private static List<UnifiedExpression> _allExpressions;
        private static List<SupportedHardwares> _allHardwares;

        internal static IReadOnlyList<FaceTrackingFeature> AllFeatures
        {
            get
            {
                if (_allFeatures != null)
                {
                    return _allFeatures;
                }

                Array all = Enum.GetValues(typeof(FaceTrackingFeature));
                _allFeatures = new List<FaceTrackingFeature>(all.Length);
                foreach (FaceTrackingFeature feature in all)
                {
                    _allFeatures.Add(feature);
                }

                return _allFeatures;
            }
        }

        internal static IReadOnlyList<ParameterSyncMode> AllSyncModes
        {
            get
            {
                if (_allSyncModes != null)
                {
                    return _allSyncModes;
                }

                Array all = Enum.GetValues(typeof(ParameterSyncMode));
                _allSyncModes = new List<ParameterSyncMode>(all.Length);
                foreach (ParameterSyncMode parameter in all)
                {
                    _allSyncModes.Add(parameter);
                }

                return _allSyncModes;
            }
        }

        internal static IReadOnlyList<UnifiedExpression> AllExpressions
        {
            get
            {
                if (_allExpressions != null)
                {
                    return _allExpressions;
                }

                Array all = Enum.GetValues(typeof(UnifiedExpression));
                _allExpressions = new List<UnifiedExpression>(all.Length - 1);
                foreach (UnifiedExpression expression in all)
                {
                    if (expression == UnifiedExpression.None)
                    {
                        continue;
                    }

                    _allExpressions.Add(expression);
                }

                return _allExpressions;
            }
        }

        internal static IReadOnlyList<SupportedHardwares> AllHardwares
        {
            get
            {
                if (_allHardwares != null)
                {
                    return _allHardwares;
                }

                Array all = Enum.GetValues(typeof(SupportedHardwares));
                _allHardwares = new List<SupportedHardwares>(all.Length - 1);
                foreach (SupportedHardwares hardware in all)
                {
                    if (hardware == SupportedHardwares.None)
                    {
                        continue;
                    }

                    _allHardwares.Add(hardware);
                }

                return _allHardwares;
            }
        }
    }
}

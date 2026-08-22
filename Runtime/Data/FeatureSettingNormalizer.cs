using System;
using System.Collections.Generic;

namespace USTL.FaceTracking
{
    internal static class FeatureSettingNormalizer
    {
        private static readonly FaceTrackingFeature[] Features = GetFeatures();
        private static readonly HashSet<ParameterSyncMode> SyncModes = GetSyncModes();

        internal static FeatureSetting[] Normalize(FeatureSetting[] settings)
        {
            Dictionary<FaceTrackingFeature, FeatureSetting> current = new(settings?.Length ?? 0);
            if (settings != null)
            {
                foreach (FeatureSetting setting in settings)
                {
                    if (setting != null && Array.IndexOf(Features, setting.feature) >= 0)
                    {
                        current[setting.feature] = setting;
                    }
                }
            }

            FeatureSetting[] normalized = new FeatureSetting[Features.Length];
            bool hasChanges = settings == null || settings.Length != normalized.Length;

            for (int i = 0; i < Features.Length; i++)
            {
                FaceTrackingFeature feature = Features[i];
                current.TryGetValue(feature, out FeatureSetting existing);
                VRCFTParameterSetId outputFormatId = NormalizeOutputFormatId(feature, existing?.outputFormatId ?? VRCFTParameterSetId.None);
                ParameterSyncMode syncMode = existing == null ? ParameterSyncMode.LocalOnly : NormalizeSyncMode(existing.syncMode);

                if (existing != null && existing.outputFormatId == outputFormatId && existing.syncMode == syncMode)
                {
                    normalized[i] = existing;
                }
                else
                {
                    normalized[i] = new FeatureSetting
                    {
                        feature = feature,
                        outputFormatId = outputFormatId,
                        syncMode = syncMode,
                    };
                    hasChanges = true;
                }

                if (!hasChanges && !ReferenceEquals(settings[i], normalized[i]))
                {
                    hasChanges = true;
                }
            }

            return hasChanges ? normalized : settings;
        }

        private static VRCFTParameterSetId NormalizeOutputFormatId(FaceTrackingFeature feature, VRCFTParameterSetId outputFormatId)
        {
            if (!FaceTrackingFeatureDefinition.All.TryGetValue(feature, out FaceTrackingFeatureDefinition definition) ||
                definition.OutputFormats.Count == 0)
            {
                return VRCFTParameterSetId.None;
            }

            foreach (VRCFTParameterSet outputFormat in definition.OutputFormats)
            {
                if (outputFormat.Id == outputFormatId)
                {
                    return outputFormatId;
                }
            }

            return definition.OutputFormats[0].Id;
        }

        private static ParameterSyncMode NormalizeSyncMode(ParameterSyncMode syncMode)
        {
            return SyncModes.Contains(syncMode) ? syncMode : ParameterSyncMode.LocalOnly;
        }

        private static FaceTrackingFeature[] GetFeatures()
        {
            Array values = Enum.GetValues(typeof(FaceTrackingFeature));
            List<FaceTrackingFeature> features = new(values.Length);
            foreach (FaceTrackingFeature feature in values)
            {
                if (Convert.ToInt64(feature) >= 0) features.Add(feature);
            }

            return features.ToArray();
        }

        private static HashSet<ParameterSyncMode> GetSyncModes()
        {
            Array values = Enum.GetValues(typeof(ParameterSyncMode));
            HashSet<ParameterSyncMode> modes = new();
            foreach (ParameterSyncMode mode in values)
            {
                if (Convert.ToInt64(mode) >= 0) modes.Add(mode);
            }

            return modes;
        }
    }
}

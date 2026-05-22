using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace USTL.FaceTracking.Editor
{
    internal static class FeatureSettingsInitializer
    {
        internal static void EnsureInitialized(SerializedObject serializedObject)
        {
            serializedObject.Update();

            IReadOnlyList<FaceTrackingFeature> features = FaceTrackingEditorUtility.AllFeatures;

            SerializedProperty settings = serializedObject.FindProperty(nameof(USTLFaceTracking.featureSettings));
            Dictionary<FaceTrackingFeature, FeatureSetting> current = new(settings.arraySize);
            for (int i = 0; i < settings.arraySize; i++)
            {
                SerializedProperty element = settings.GetArrayElementAtIndex(i);
                FaceTrackingFeature feature = (FaceTrackingFeature)element.FindPropertyRelative(nameof(FeatureSetting.feature)).intValue;
                if (!FaceTrackingEditorUtility.AllFeatures.Contains(feature))
                {
                    continue;
                }

                VRCFTParameterSetId outputFormatId = (VRCFTParameterSetId)element.FindPropertyRelative(nameof(FeatureSetting.outputFormatId)).intValue;
                ParameterSyncMode syncMode = (ParameterSyncMode)element.FindPropertyRelative(nameof(FeatureSetting.syncMode)).intValue;
                FeatureSetting setting = new()
                {
                    feature = feature,
                    outputFormatId = ValidateOutputFormatId(feature, outputFormatId),
                    syncMode = ValidateSyncMode(syncMode),
                };
                current[feature] = setting;
            }

            bool hasChanges = settings.arraySize != features.Count;
            settings.arraySize = features.Count;

            for (int i = 0; i < features.Count; i++)
            {
                SerializedProperty elementProperty = settings.GetArrayElementAtIndex(i);
                SerializedProperty featureProperty = elementProperty.FindPropertyRelative(nameof(FeatureSetting.feature));
                SerializedProperty outputFormatIdProperty = elementProperty.FindPropertyRelative(nameof(FeatureSetting.outputFormatId));
                SerializedProperty syncModeProperty = elementProperty.FindPropertyRelative(nameof(FeatureSetting.syncMode));

                if ((FaceTrackingFeature)featureProperty.intValue != features[i])
                {
                    featureProperty.intValue = (int)features[i];

                    if (!current.ContainsKey(features[i]))
                    {
                        FaceTrackingFeatureDefinition definition = FaceTrackingFeatureDefinition.All.GetValueOrDefault(features[i]);
                        outputFormatIdProperty.intValue = (int)(definition == null || definition.OutputFormats.Count == 0 ? VRCFTParameterSetId.None : definition.OutputFormats[0].Id);
                        syncModeProperty.intValue = (int)ParameterSyncMode.LocalOnly;
                    }
                    else
                    {
                        outputFormatIdProperty.intValue = (int)current[features[i]].outputFormatId;
                        syncModeProperty.intValue = (int)current[features[i]].syncMode;
                    }

                    hasChanges = true;
                }
                else
                {
                    if (!current.ContainsKey(features[i]))
                    {
                        FaceTrackingFeatureDefinition definition = FaceTrackingFeatureDefinition.All.GetValueOrDefault(features[i]);
                        outputFormatIdProperty.intValue = (int)(definition == null || definition.OutputFormats.Count == 0 ? VRCFTParameterSetId.None : definition.OutputFormats[0].Id);
                        syncModeProperty.intValue = (int)ParameterSyncMode.LocalOnly;
                        hasChanges = true;
                    }
                    else
                    {
                        if (outputFormatIdProperty.intValue != (int)current[features[i]].outputFormatId)
                        {
                            outputFormatIdProperty.intValue = (int)current[features[i]].outputFormatId;
                            hasChanges = true;
                        }

                        if (syncModeProperty.intValue != (int)current[features[i]].syncMode)
                        {
                            syncModeProperty.intValue = (int)current[features[i]].syncMode;
                            hasChanges = true;
                        }
                    }
                }
            }

            if (hasChanges)
            {
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static VRCFTParameterSetId ValidateOutputFormatId(FaceTrackingFeature feature, VRCFTParameterSetId outputFormatId)
        {
            FaceTrackingFeatureDefinition definition = FaceTrackingFeatureDefinition.All.GetValueOrDefault(feature);
            if (definition == null || definition.OutputFormats.Count == 0)
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

        private static ParameterSyncMode ValidateSyncMode(ParameterSyncMode mode)
        {
            return FaceTrackingEditorUtility.AllSyncModes.Contains(mode) ? mode : ParameterSyncMode.LocalOnly;
        }
    }
}

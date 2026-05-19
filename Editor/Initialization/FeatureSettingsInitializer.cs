using System;
using System.Collections.Generic;
using UnityEditor;

namespace USTL.FaceTracking.Editor
{
    internal static class FeatureSettingsInitializer
    {
        internal static void EnsureInitialized(SerializedObject serializedObject)
        {
            serializedObject.Update();

            SerializedProperty featureSettings = serializedObject.FindProperty(nameof(USTLFaceTracking.featureSettings));
            IReadOnlyList<FaceTrackingFeatureDefinition> featureDefinitions = FaceTrackingEditorUtility.AllFeatureDefinitions;
            Dictionary<FaceTrackingFeature, FeatureSettingSnapshot> existingSettings = BuildExistingSettingMap(featureSettings);

            bool hasChanges = featureSettings.arraySize != featureDefinitions.Count;
            featureSettings.arraySize = featureDefinitions.Count;

            for (int i = 0; i < featureDefinitions.Count; i++)
            {
                FaceTrackingFeatureDefinition featureDefinition = featureDefinitions[i];
                SerializedProperty element = featureSettings.GetArrayElementAtIndex(i);
                SerializedProperty featureProperty = element.FindPropertyRelative(nameof(FaceTrackingFeatureSetting.feature));
                SerializedProperty outputFormatIndexProperty = element.FindPropertyRelative(nameof(FaceTrackingFeatureSetting.outputFormatIndex));
                SerializedProperty syncModeProperty = element.FindPropertyRelative(nameof(FaceTrackingFeatureSetting.syncMode));

                int featureValue = (int)featureDefinition.Feature;
                if (featureProperty.intValue != featureValue)
                {
                    featureProperty.intValue = featureValue;
                    hasChanges = true;
                }

                int outputFormatIndex = 0;
                ParameterSyncMode syncMode = ParameterSyncMode.LocalOnly;
                if (existingSettings.TryGetValue(featureDefinition.Feature, out FeatureSettingSnapshot existingSetting))
                {
                    outputFormatIndex = existingSetting.OutputFormatIndex;
                    syncMode = existingSetting.SyncMode;
                }

                outputFormatIndex = VRCFTParameterOutputFormatUtility.ClampIndex(featureDefinition, outputFormatIndex);
                syncMode = ClampSyncMode(featureDefinition.OutputFormats[outputFormatIndex], syncMode);

                if (outputFormatIndexProperty.intValue != outputFormatIndex)
                {
                    outputFormatIndexProperty.intValue = outputFormatIndex;
                    hasChanges = true;
                }

                if (syncModeProperty.intValue != (int)syncMode)
                {
                    syncModeProperty.intValue = (int)syncMode;
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static Dictionary<FaceTrackingFeature, FeatureSettingSnapshot> BuildExistingSettingMap(SerializedProperty featureSettings)
        {
            Dictionary<FaceTrackingFeature, FeatureSettingSnapshot> settings = new();
            if (featureSettings == null)
            {
                return settings;
            }

            for (int i = 0; i < featureSettings.arraySize; i++)
            {
                SerializedProperty element = featureSettings.GetArrayElementAtIndex(i);
                FaceTrackingFeature feature = (FaceTrackingFeature)element.FindPropertyRelative(nameof(FaceTrackingFeatureSetting.feature)).intValue;
                int outputFormatIndex = element.FindPropertyRelative(nameof(FaceTrackingFeatureSetting.outputFormatIndex)).intValue;
                ParameterSyncMode syncMode = (ParameterSyncMode)element.FindPropertyRelative(nameof(FaceTrackingFeatureSetting.syncMode)).intValue;

                settings[feature] = new FeatureSettingSnapshot(outputFormatIndex, syncMode);
            }

            return settings;
        }

        private static ParameterSyncMode ClampSyncMode(VRCFTParameterOutputFormat outputFormat, ParameterSyncMode syncMode)
        {
            if (!VRCFTParameterOutputFormatUtility.UsesGeneratedParameters(outputFormat))
            {
                return ParameterSyncMode.None;
            }

            return Enum.IsDefined(typeof(ParameterSyncMode), syncMode) ? syncMode : ParameterSyncMode.LocalOnly;
        }
    }
}

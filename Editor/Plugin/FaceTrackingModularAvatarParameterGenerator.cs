using System.Collections.Generic;
using nadena.dev.modular_avatar.core;

namespace USTL.FaceTracking.Editor
{
    internal static class FaceTrackingModularAvatarParameterGenerator
    {
        internal static void Populate(ModularAvatarParameters parameters, IReadOnlyList<SelectedParameterSetting> selectedParameters)
        {
            HashSet<string> registeredNames = new();

            foreach (SelectedParameterSetting setting in selectedParameters)
            {
                if (ParameterSyncModeUtility.IsBinary(setting.SyncMode))
                {
                    AddBinaryParameters(parameters, registeredNames, setting);
                    continue;
                }

                AddFloatParameter(parameters, registeredNames, FaceTrackingGeneratedParameterNames.FormatFloat(setting.Parameter), setting.SyncMode == ParameterSyncMode.LocalOnly, FaceTrackingParameterDefaults.GetDefaultValue(setting.Parameter));
            }
        }

        private static void AddFloatParameter(ModularAvatarParameters parameters, HashSet<string> registeredNames, string parameterName, bool localOnly, float defaultValue)
        {
            if (!registeredNames.Add(parameterName))
            {
                return;
            }

            parameters.parameters.Add(new ParameterConfig
            {
                nameOrPrefix = parameterName,
                syncType = ParameterSyncType.Float,
                localOnly = localOnly,
                defaultValue = defaultValue,
                saved = false,
            });
        }

        private static void AddBinaryParameters(ModularAvatarParameters parameters, HashSet<string> registeredNames, SelectedParameterSetting setting)
        {
            int bitCount = ParameterSyncModeUtility.GetBinaryBitCount(setting.SyncMode);
            float defaultValue = FaceTrackingParameterDefaults.GetDefaultValue(setting.Parameter);
            int defaultMagnitude = BinaryParameterEncoding.GetMagnitude(defaultValue, bitCount);
            for (int i = 0; i < bitCount; i++)
            {
                AddBoolParameter(parameters, registeredNames, FaceTrackingGeneratedParameterNames.FormatBinaryBit(setting.Parameter, 1 << i), (defaultMagnitude & (1 << i)) != 0);
            }

            if (BinaryParameterEncoding.IsSignedParameter(setting.Parameter))
            {
                AddBoolParameter(parameters, registeredNames, FaceTrackingGeneratedParameterNames.FormatBinaryNegative(setting.Parameter), defaultMagnitude > 0 && defaultValue < 0f);
            }
        }

        private static void AddBoolParameter(ModularAvatarParameters parameters, HashSet<string> registeredNames, string parameterName, bool defaultValue)
        {
            if (!registeredNames.Add(parameterName))
            {
                return;
            }

            parameters.parameters.Add(new ParameterConfig
            {
                nameOrPrefix = parameterName,
                syncType = ParameterSyncType.Bool,
                localOnly = false,
                defaultValue = defaultValue ? 1f : 0f,
                saved = false,
            });
        }
    }
}

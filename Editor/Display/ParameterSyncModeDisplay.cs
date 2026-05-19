using System.Collections.Generic;

namespace USTL.FaceTracking.Editor
{
    internal static class ParameterSyncModeDisplay
    {
        private static readonly ParameterSyncMode[] Values =
        {
            ParameterSyncMode.None,
            ParameterSyncMode.LocalOnly,
            ParameterSyncMode.Float8,
            ParameterSyncMode.Binary1Bit,
            ParameterSyncMode.Binary2Bit,
            ParameterSyncMode.Binary3Bit,
            ParameterSyncMode.Binary4Bit,
        };

        private static readonly string[] DisplayNames =
        {
            "Disabled",
            "Local Only",
            "Float (8-bit)",
            "Binary (1-bit)",
            "Binary (2-bit)",
            "Binary (3-bit)",
            "Binary (4-bit)",
        };

        private static readonly string[] NativeChoices =
        {
            "VRChat Native",
        };

        internal static List<string> GetChoices()
        {
            return new List<string>(DisplayNames);
        }

        internal static List<string> GetNativeChoices()
        {
            return new List<string>
            {
                GetNativeDisplayName(),
            };
        }

        internal static string GetNativeDisplayName()
        {
            return FaceTrackingEditorText.Get("sync_mode.VRChatNative", NativeChoices[0]);
        }

        internal static string GetDisplayName(ParameterSyncMode syncMode)
        {
            for (int i = 0; i < Values.Length; i++)
            {
                if (Values[i] == syncMode)
                {
                    return DisplayNames[i];
                }
            }

            return GetDisplayName(ParameterSyncMode.LocalOnly);
        }

        internal static string FormatChoice(string displayName)
        {
            for (int i = 0; i < DisplayNames.Length; i++)
            {
                if (DisplayNames[i] == displayName)
                {
                    return GetLocalizedDisplayName(Values[i]);
                }
            }

            return displayName;
        }

        internal static ParameterSyncMode Find(string displayName)
        {
            return TryFind(displayName, out ParameterSyncMode syncMode) ? syncMode : ParameterSyncMode.LocalOnly;
        }

        internal static bool TryFind(string displayName, out ParameterSyncMode syncMode)
        {
            for (int i = 0; i < DisplayNames.Length; i++)
            {
                if (DisplayNames[i] == displayName || GetLocalizedDisplayName(Values[i]) == displayName)
                {
                    syncMode = Values[i];
                    return true;
                }
            }

            syncMode = ParameterSyncMode.LocalOnly;
            return false;
        }

        internal static string FormatTooltip(ParameterSyncMode syncMode)
        {
            return syncMode switch
            {
                ParameterSyncMode.None => FaceTrackingEditorText.Get("sync_mode.None.tooltip", "Do not use this feature."),
                ParameterSyncMode.LocalOnly => FaceTrackingEditorText.Get("sync_mode.LocalOnly.tooltip", "Use local generation only; do not create synced avatar parameters."),
                ParameterSyncMode.Float8 => FaceTrackingEditorText.Get("sync_mode.Float8.tooltip", "Sync with one float parameter."),
                ParameterSyncMode.Binary1Bit => FaceTrackingEditorText.Get("sync_mode.Binary1Bit.tooltip", "Sync with one binary parameter."),
                ParameterSyncMode.Binary2Bit => FaceTrackingEditorText.Get("sync_mode.Binary2Bit.tooltip", "Sync with two binary parameters."),
                ParameterSyncMode.Binary3Bit => FaceTrackingEditorText.Get("sync_mode.Binary3Bit.tooltip", "Sync with three binary parameters."),
                ParameterSyncMode.Binary4Bit => FaceTrackingEditorText.Get("sync_mode.Binary4Bit.tooltip", "Sync with four binary parameters."),
                _ => string.Empty,
            };
        }

        internal static string FormatNativeTooltip()
        {
            return FaceTrackingEditorText.Get("sync_mode.VRChatNative.tooltip", "Use VRChat native tracking. This package creates no synced avatar parameters for this output format; remote display is handled by VRChat.");
        }

        private static string GetLocalizedDisplayName(ParameterSyncMode syncMode)
        {
            return FaceTrackingEditorText.Get($"sync_mode.{syncMode}", GetDisplayName(syncMode));
        }
    }
}

namespace USTL.FaceTracking.Editor
{
    internal static class SyncParameterUsageDisplay
    {
        internal static string FormatSummary(SyncParameterUsage usage)
        {
            return string.Format(T("summary.sync_parameter_usage", "Sync Parameter Usage: {0} bits ({1}/{2} parameters, {3} without blend shape assignments)"), usage.ConsumedBits, usage.ConsumedParameterCount, usage.ExpectedParameterCount, usage.UnassignedParameterCount);
        }

        internal static string FormatTooltip(SyncParameterUsage usage)
        {
            return string.Format(T("summary.sync_parameter_usage.tooltip", "Counts selected features that have at least one valid blend shape assignment. Float sync costs 8 bits and one synced avatar parameter; binary sync costs the selected bit count, plus one sign bit for signed binary parameters. Selected features can use up to {0} synced avatar parameters; {1} are consumed, and {2} are skipped because they have no valid blend shape assignment."), usage.ExpectedParameterCount, usage.ConsumedParameterCount, usage.UnassignedParameterCount);
        }

        private static string T(string key, string fallback)
        {
            return FaceTrackingEditorText.Get(key, fallback);
        }
    }
}

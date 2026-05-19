namespace USTL.FaceTracking.Editor
{
    internal readonly struct FeatureSettingSnapshot
    {
        internal FeatureSettingSnapshot(int outputFormatIndex, ParameterSyncMode syncMode)
        {
            OutputFormatIndex = outputFormatIndex;
            SyncMode = syncMode;
        }

        internal int OutputFormatIndex { get; }
        internal ParameterSyncMode SyncMode { get; }
    }
}

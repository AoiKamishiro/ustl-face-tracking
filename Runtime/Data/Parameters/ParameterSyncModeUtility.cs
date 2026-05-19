namespace USTL.FaceTracking
{
    internal static class ParameterSyncModeUtility
    {
        internal static bool IsBinary(ParameterSyncMode syncMode)
        {
            return syncMode == ParameterSyncMode.Binary1Bit || syncMode == ParameterSyncMode.Binary2Bit || syncMode == ParameterSyncMode.Binary3Bit || syncMode == ParameterSyncMode.Binary4Bit;
        }

        internal static int GetBinaryBitCount(ParameterSyncMode syncMode)
        {
            return syncMode switch
            {
                ParameterSyncMode.Binary1Bit => 1,
                ParameterSyncMode.Binary2Bit => 2,
                ParameterSyncMode.Binary3Bit => 3,
                ParameterSyncMode.Binary4Bit => 4,
                _ => 0,
            };
        }
    }
}

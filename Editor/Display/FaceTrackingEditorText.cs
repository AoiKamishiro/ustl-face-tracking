namespace USTL.FaceTracking.Editor
{
    internal static class FaceTrackingEditorText
    {
        private const string LocalizationKeyPrefix = "ustl.facetracking.editor.";

        internal static string Get(string key, string fallback)
        {
            return FaceTrackingLocalization.S(LocalizationKeyPrefix + key, fallback);
        }
    }
}

using System;
using UnityEngine.UIElements;

namespace USTL.FaceTracking.Editor
{
    internal sealed class SyncParameterUsageLabel : HelpBox
    {
        internal SyncParameterUsageLabel(USTLFaceTracking faceTracking)
        {
            messageType = HelpBoxMessageType.Info;
            name = "sync-parameter-usage";
            FaceTracking = faceTracking;
        }

        private USTLFaceTracking FaceTracking { get; }

        internal string SummaryFormat { get; set; } = "Sync Parameter Usage: {0} bits";

        public Action OnLangChanged { get; set; }

        public void Rebuild()
        {
            text = string.Format(SummaryFormat, VRCParameterUtility.CalculateUsage(FaceTracking));
        }
    }
}

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

        internal string SummaryFormat { get; set; } = "Sync Parameter Usage: {0} bits ({1}/{2} parameters, {3} without blend shape assignments)";

        public Action OnLangChanged { get; set; }

        public void Rebuild()
        {
            //TODO: 表示項目はあとで調整したい
            text = $"Use {VRCParameterUtility.CalculateUsage(FaceTracking)} parameters.";
            // text = string.Format(SummaryFormat, ParameterUsage.ConsumedBits, ParameterUsage.ConsumedParameterCount, ParameterUsage.ExpectedParameterCount, ParameterUsage.UnassignedParameterCount);
        }
    }
}

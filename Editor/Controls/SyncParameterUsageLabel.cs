using UnityEngine.UIElements;

namespace USTL.FaceTracking.Editor
{
    internal sealed class SyncParameterUsageLabel : LocalizedHelpBox
    {
        private SyncParameterUsage _parameterUsage;

        internal SyncParameterUsageLabel()
        {
            messageType = HelpBoxMessageType.Info;
            name = "sync-parameter-usage";
        }

        internal string SummaryFormat { get; set; } = "Sync Parameter Usage: {0} bits ({1}/{2} parameters, {3} without blend shape assignments)";

        internal SyncParameterUsage ParameterUsage
        {
            get => _parameterUsage;
            set
            {
                _parameterUsage = value;
                Rebuild();
            }
        }

        public void Rebuild()
        {
            text = string.Format(SummaryFormat, ParameterUsage.ConsumedBits, ParameterUsage.ConsumedParameterCount, ParameterUsage.ExpectedParameterCount, ParameterUsage.UnassignedParameterCount);
        }
    }
}

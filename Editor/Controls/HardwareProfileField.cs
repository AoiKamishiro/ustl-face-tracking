using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace USTL.FaceTracking.Editor
{
    internal sealed class HardwareProfileField : EnumFlagsField
    {
        private const string FieldName = "tracking-hardware";

        internal HardwareProfileField() : base(SupportedHardwares.None)
        {
            name = FieldName;
            label = "Tracking Hardware";
            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;

            labelElement.style.unityTextAlign = TextAnchor.MiddleLeft;
        }
    }
}

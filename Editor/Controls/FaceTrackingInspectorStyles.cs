using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace USTL.FaceTracking.Editor
{
    internal static class FaceTrackingInspectorStyles
    {
        internal const int InspectorFieldSpacing = 2;

        internal static readonly string PopupFieldTextUssClassName = BasePopupField<string, string>.textUssClassName;

        private static readonly Color InvalidBlendShapeNameTextColor = new(1f, 0.25f, 0.25f);


        internal static StyleColor GetBlendShapeNameTextColor(bool isInvalid)
        {
            return isInvalid ? InvalidBlendShapeNameTextColor : StyleKeyword.Null;
        }

        internal static void ApplySupportStatusText(Label label, HardwareSupportStatus status)
        {
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.color = GetSupportStatusTextColor(status);
        }

        internal static void ApplyHardwareKeyAvailabilityStatusText(Label label, HardwareKeyAvailabilityStatus status)
        {
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.color = GetHardwareKeyAvailabilityStatusTextColor(status);
        }

        internal static Color GetSupportStatusTextColor(HardwareSupportStatus status)
        {
            bool proSkin = EditorGUIUtility.isProSkin;
            return status switch
            {
                HardwareSupportStatus.Full => proSkin ? new Color(0.42f, 0.86f, 0.48f) : new Color(0.05f, 0.45f, 0.1f),
                HardwareSupportStatus.Converted => proSkin ? new Color(1f, 0.72f, 0.28f) : new Color(0.72f, 0.39f, 0.02f),
                HardwareSupportStatus.Unsupported => proSkin ? new Color(1f, 0.46f, 0.42f) : new Color(0.62f, 0.05f, 0.04f),
                HardwareSupportStatus.Unknown => proSkin ? new Color(0.62f, 0.72f, 0.86f) : new Color(0.24f, 0.34f, 0.48f),
                _ => proSkin ? Color.white : Color.black,
            };
        }

        private static Color GetHardwareKeyAvailabilityStatusTextColor(HardwareKeyAvailabilityStatus status)
        {
            bool proSkin = EditorGUIUtility.isProSkin;
            return status switch
            {
                HardwareKeyAvailabilityStatus.Available => proSkin ? new Color(0.42f, 0.86f, 0.48f) : new Color(0.05f, 0.45f, 0.1f),
                HardwareKeyAvailabilityStatus.Unavailable => proSkin ? new Color(1f, 0.46f, 0.42f) : new Color(0.62f, 0.05f, 0.04f),
                HardwareKeyAvailabilityStatus.Unknown => proSkin ? new Color(0.62f, 0.72f, 0.86f) : new Color(0.24f, 0.34f, 0.48f),
                _ => proSkin ? Color.white : Color.black,
            };
        }
    }
}

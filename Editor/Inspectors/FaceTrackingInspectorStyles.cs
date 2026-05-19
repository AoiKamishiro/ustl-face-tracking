using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace USTL.FaceTracking.Editor
{
    internal static class FaceTrackingInspectorStyles
    {
        internal const int InspectorFieldSpacing = 2;
        internal const int FeatureSettingItemHeight = 28;
        internal const int FeatureSettingFeatureColumnWidth = 148;
        internal const int FeatureSettingSupportColumnWidth = 40;
        internal const int FeatureSettingSyncModeColumnWidth = 112;
        internal const int BlendShapeAssignmentItemHeight = 32;
        internal const int BlendShapeAssignmentExpressionColumnWidth = 148;
        internal const int BlendShapeAssignmentSupportColumnWidth = 40;
        internal const int BlendShapeAssignmentValueColumnWidth = 112;

        private const int InspectorHorizontalPadding = 15;
        private const int InspectorVerticalPadding = 4;
        private const int LogoPaddingTop = 4;
        private const int LogoPaddingBottom = 8;
        private const int FeatureSettingCellHorizontalPadding = 6;
        private const int FeatureSettingCellVerticalPadding = 2;
        private const int BlendShapeAssignmentCellHorizontalPadding = 6;
        private const int BlendShapeAssignmentCellVerticalPadding = 4;
        private const int SyncParameterUsageHorizontalPadding = 8;
        private const int SyncParameterUsageVerticalPadding = 6;

        internal static readonly string PopupFieldTextUssClassName = BasePopupField<string, string>.textUssClassName;

        private static readonly Color InvalidBlendShapeNameTextColor = new(1f, 0.25f, 0.25f);

        internal static void ApplyInspectorRoot(VisualElement root)
        {
            root.style.paddingTop = InspectorVerticalPadding;
            root.style.paddingRight = InspectorHorizontalPadding;
            root.style.paddingBottom = InspectorVerticalPadding;
            root.style.paddingLeft = InspectorHorizontalPadding;
            root.style.marginTop = 0;
            root.style.marginBottom = 0;
        }

        internal static void ApplyFieldSpacing(VisualElement element)
        {
            element.style.marginTop = 0;
            element.style.marginBottom = InspectorFieldSpacing;
        }

        internal static void ApplyLogoContainer(VisualElement element)
        {
            element.style.flexDirection = FlexDirection.Row;
            element.style.alignItems = Align.Center;
            element.style.justifyContent = Justify.Center;
            element.style.paddingTop = LogoPaddingTop;
            element.style.paddingBottom = LogoPaddingBottom;
            element.style.marginBottom = InspectorFieldSpacing;
        }

        internal static void ApplyLogoImage(Image image)
        {
            image.style.flexShrink = 0;
        }

        internal static void ApplyEmbeddedField(VisualElement element)
        {
            element.style.marginTop = 0;
            element.style.marginRight = 0;
            element.style.marginBottom = 0;
            element.style.marginLeft = 0;
            element.style.flexGrow = 1;
        }

        internal static void ApplyHardwareProfileField(VisualElement element)
        {
            element.AddToClassList(BaseField<Object>.ussClassName);
            element.style.flexDirection = FlexDirection.Row;
            element.style.alignItems = Align.Center;
        }

        internal static void ApplyHardwareProfileLabel(Label label)
        {
            label.AddToClassList(BaseField<Object>.labelUssClassName);
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
        }

        internal static void ApplyHardwareProfileButton(Button button)
        {
            button.AddToClassList(BaseField<Object>.inputUssClassName);
            button.style.flexGrow = 1;
            button.style.unityTextAlign = TextAnchor.MiddleLeft;
        }

        internal static void ApplyStatusIndicatorHeader(Label label)
        {
            label.style.flexGrow = 1;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
        }

        internal static void ApplyFeatureSettingCellBase(VisualElement element)
        {
            element.style.paddingTop = FeatureSettingCellVerticalPadding;
            element.style.paddingRight = FeatureSettingCellHorizontalPadding;
            element.style.paddingBottom = FeatureSettingCellVerticalPadding;
            element.style.paddingLeft = FeatureSettingCellHorizontalPadding;
            element.style.flexGrow = 1;
            element.style.minHeight = FeatureSettingItemHeight;
        }

        internal static void ApplyBlendShapeAssignmentCellBase(VisualElement element)
        {
            element.style.paddingTop = BlendShapeAssignmentCellVerticalPadding;
            element.style.paddingRight = BlendShapeAssignmentCellHorizontalPadding;
            element.style.paddingBottom = BlendShapeAssignmentCellVerticalPadding;
            element.style.paddingLeft = BlendShapeAssignmentCellHorizontalPadding;
            element.style.flexGrow = 1;
            element.style.minHeight = BlendShapeAssignmentItemHeight;
        }

        internal static void ApplySyncParameterUsageLabel(Label label)
        {
            label.style.marginTop = 0;
            label.style.marginBottom = InspectorFieldSpacing;
            label.style.paddingTop = SyncParameterUsageVerticalPadding;
            label.style.paddingRight = SyncParameterUsageHorizontalPadding;
            label.style.paddingBottom = SyncParameterUsageVerticalPadding;
            label.style.paddingLeft = SyncParameterUsageHorizontalPadding;
            label.style.minHeight = FeatureSettingItemHeight;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.backgroundColor = GetSummaryBackgroundColor();
            label.style.borderTopWidth = 1;
            label.style.borderRightWidth = 1;
            label.style.borderBottomWidth = 1;
            label.style.borderLeftWidth = 1;
            label.style.borderTopColor = GetSeparatorColor();
            label.style.borderRightColor = GetSeparatorColor();
            label.style.borderBottomColor = GetSeparatorColor();
            label.style.borderLeftColor = GetSeparatorColor();
        }

        internal static void ApplyFeatureSettingCell(VisualElement element, int index)
        {
            ApplyListCell(element, index);
        }

        internal static void ApplyBlendShapeAssignmentCell(VisualElement element, int index)
        {
            ApplyListCell(element, index);
        }

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

        private static void ApplyListCell(VisualElement element, int index)
        {
            element.style.backgroundColor = GetRowColor(index);
            element.style.borderBottomWidth = 1;
            element.style.borderBottomColor = GetSeparatorColor();
        }

        private static Color GetRowColor(int index)
        {
            bool isEven = index % 2 == 0;
            if (EditorGUIUtility.isProSkin)
            {
                return isEven ? new Color(1f, 1f, 1f, 0.03f) : new Color(1f, 1f, 1f, 0.07f);
            }

            return isEven ? new Color(0f, 0f, 0f, 0.02f) : new Color(0f, 0f, 0f, 0.05f);
        }

        private static Color GetSeparatorColor()
        {
            return EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.12f) : new Color(0f, 0f, 0f, 0.16f);
        }

        private static Color GetSummaryBackgroundColor()
        {
            return EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.05f) : new Color(0f, 0f, 0f, 0.04f);
        }

        private static Color GetSupportStatusTextColor(HardwareSupportStatus status)
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

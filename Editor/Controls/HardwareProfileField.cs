using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace USTL.FaceTracking.Editor
{
    internal sealed class HardwareProfileField : EnumFlagsField, ILocalization
    {
        private const string FieldName = "tracking-hardware";

        internal HardwareProfileField() : base(SupportedHardwares.None)
        {
            name = FieldName;
            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;

            labelElement.style.unityTextAlign = TextAnchor.MiddleLeft;
        }

        internal string LabelText
        {
            get => label;
            set => label = value;
        }

        internal string ButtonTooltip
        {
            get => tooltip;
            set => tooltip = value;
        }

        public Action OnLangChanged { get; set; }
    }
}

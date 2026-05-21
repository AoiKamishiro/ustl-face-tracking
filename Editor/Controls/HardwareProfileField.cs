using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace USTL.FaceTracking.Editor
{
    internal sealed class HardwareProfileField : BaseField<int>, ILocalization
    {
        private const string ButtonName = "tracking-hardware";
        private readonly Button _button;

        internal HardwareProfileField() : this(new Button())
        {
        }

        private HardwareProfileField(Button button) : base(null, button)
        {
            _button = button;

            AddToClassList(ussClassName);
            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;

            labelElement.AddToClassList(labelUssClassName);
            labelElement.style.unityTextAlign = TextAnchor.MiddleLeft;

            _button.name = ButtonName;
            _button.clicked += ShowMenu;
            _button.AddToClassList(inputUssClassName);
            _button.style.flexGrow = 1;
            _button.style.unityTextAlign = TextAnchor.MiddleLeft;
            RefreshButtonText();
        }

        public List<HardwareSupportProfile> ItemsSource { get; set; } = new();

        internal string LabelText
        {
            get => label;
            set => label = value;
        }

        internal string ButtonTooltip
        {
            get => _button.tooltip;
            set => _button.tooltip = value;
        }

        public Action OnLangChanged { get; set; }

        public override void SetValueWithoutNotify(int newValue)
        {
            base.SetValueWithoutNotify(newValue);
            RefreshButtonText();
        }

        private static bool ContainProfile(int profiles, HardwareSupportProfile profile)
        {
            return profile.Flag != 0 && (profiles & profile.Flag) == profile.Flag;
        }

        private void ShowMenu()
        {
            int trackingHardwareProfiles = value;
            GenericMenu menu = new();
            menu.AddItem(new GUIContent("None"), trackingHardwareProfiles == 0, () => SetValue(0));
            menu.AddSeparator(string.Empty);

            foreach (HardwareSupportProfile profile in ItemsSource)
            {
                menu.AddItem(new GUIContent(profile.DisplayName), ContainProfile(trackingHardwareProfiles, profile), () => ToggleProfile(profile));
            }

            menu.ShowAsContext();
        }

        private void ToggleProfile(HardwareSupportProfile profile)
        {
            int newHardwareProfiles = ContainProfile(value, profile) ? value & ~profile.Flag : value | profile.Flag;
            SetValue(newHardwareProfiles);
        }

        private void SetValue(int trackingHardwareProfiles)
        {
            if (value == trackingHardwareProfiles)
            {
                return;
            }

            value = trackingHardwareProfiles;
        }

        private void RefreshButtonText()
        {
            List<string> selected = new();
            foreach (HardwareSupportProfile profile in ItemsSource)
            {
                if (ContainProfile(value, profile))
                {
                    selected.Add(profile.DisplayName);
                }
            }

            _button.text = selected.Count == 0 ? "None" : selected.Count <= 2 ? string.Join(", ", selected) : $"{selected.Count} selected";
        }
    }
}

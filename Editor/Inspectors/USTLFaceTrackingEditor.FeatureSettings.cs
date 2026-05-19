using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace USTL.FaceTracking.Editor
{
    internal sealed partial class USTLFaceTrackingEditor
    {
        private MultiColumnListView CreateFeatureSettingView()
        {
            MultiColumnListView view = new()
            {
                bindingPath = nameof(USTLFaceTracking.featureSettings),
                itemsSource = _featureSettingItems,
                fixedItemHeight = FaceTrackingInspectorStyles.FeatureSettingItemHeight,
                showAddRemoveFooter = false,
                showBoundCollectionSize = false,
                showBorder = true,
                reorderable = false,
                selectionType = SelectionType.None,
                showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
                virtualizationMethod = CollectionVirtualizationMethod.FixedHeight,
            };

            view.columns.Add(CreateFeatureColumn());
            view.columns.Add(CreateHardwareSupportColumn());
            view.columns.Add(CreateOutputFormatColumn());
            view.columns.Add(CreateSyncModeColumn());
            view.style.width = Length.Percent(100);
            view.style.flexGrow = 0;
            return view;
        }

        private Column CreateFeatureColumn()
        {
            return new Column
            {
                title = T("column.feature", "Feature"),
                width = FaceTrackingInspectorStyles.FeatureSettingFeatureColumnWidth,
                minWidth = FaceTrackingInspectorStyles.FeatureSettingFeatureColumnWidth,
                maxWidth = FaceTrackingInspectorStyles.FeatureSettingFeatureColumnWidth,
                stretchable = false,
                resizable = false,
                makeCell = CreateFeatureCell,
                bindCell = BindFeatureCell,
            };
        }

        private Column CreateHardwareSupportColumn()
        {
            return new Column
            {
                title = T("column.hardware_support", "Hardware Support"),
                width = FaceTrackingInspectorStyles.FeatureSettingSupportColumnWidth,
                minWidth = FaceTrackingInspectorStyles.FeatureSettingSupportColumnWidth,
                maxWidth = FaceTrackingInspectorStyles.FeatureSettingSupportColumnWidth,
                stretchable = false,
                resizable = false,
                makeHeader = CreateStatusIndicatorHeader,
                bindHeader = header => BindStatusIndicatorHeader(header, T("column.hardware_support_short", "HW"), T("column.hardware_support", "Hardware Support")),
                makeCell = CreateHardwareSupportCell,
                bindCell = BindHardwareSupportCell,
            };
        }

        private Column CreateOutputFormatColumn()
        {
            return new Column
            {
                title = T("column.output_format", "Output Format"),
                stretchable = true,
                makeCell = CreateOutputFormatCell,
                bindCell = BindOutputFormatCell,
            };
        }

        private Column CreateSyncModeColumn()
        {
            return new Column
            {
                title = T("column.sync_mode", "Sync Mode"),
                width = FaceTrackingInspectorStyles.FeatureSettingSyncModeColumnWidth,
                minWidth = FaceTrackingInspectorStyles.FeatureSettingSyncModeColumnWidth,
                maxWidth = FaceTrackingInspectorStyles.FeatureSettingSyncModeColumnWidth,
                stretchable = false,
                resizable = false,
                makeCell = CreateSyncModeCell,
                bindCell = BindSyncModeCell,
            };
        }

        private static VisualElement CreateFeatureCell()
        {
            Label label = new();
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            FaceTrackingInspectorStyles.ApplyFeatureSettingCellBase(label);
            return label;
        }

        private static VisualElement CreateHardwareSupportCell()
        {
            Label label = new();
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            FaceTrackingInspectorStyles.ApplyFeatureSettingCellBase(label);
            return label;
        }

        private VisualElement CreateOutputFormatCell()
        {
            VisualElement innerRoot = new();
            FaceTrackingInspectorStyles.ApplyFeatureSettingCellBase(innerRoot);

            DropdownField field = new()
            {
                name = OutputFormatDropdownName,
            };
            FaceTrackingInspectorStyles.ApplyEmbeddedField(field);

            innerRoot.Add(field);
            return innerRoot;
        }

        private VisualElement CreateSyncModeCell()
        {
            VisualElement innerRoot = new();
            FaceTrackingInspectorStyles.ApplyFeatureSettingCellBase(innerRoot);

            DropdownField field = new()
            {
                name = SyncModeDropdownName,
            };
            FaceTrackingInspectorStyles.ApplyEmbeddedField(field);

            innerRoot.Add(field);
            return innerRoot;
        }

        private void BindFeatureCell(VisualElement element, int index)
        {
            if (element is not Label label)
            {
                return;
            }

            SerializedProperty featureSetting = GetFeatureSettingElementAt(index);
            FaceTrackingFeatureDefinition featureDefinition = GetFeatureDefinition(featureSetting);
            label.text = OutputFormatDisplay.FormatFeatureName(featureSetting, featureDefinition);
            label.tooltip = label.text;
            FaceTrackingInspectorStyles.ApplyFeatureSettingCell(label, index);
        }

        private void BindHardwareSupportCell(VisualElement element, int index)
        {
            if (element is not Label label)
            {
                return;
            }

            SerializedProperty featureSetting = GetFeatureSettingElementAt(index);
            FaceTrackingFeatureDefinition featureDefinition = GetFeatureDefinition(featureSetting);
            FaceTrackingHardwareProfile trackingHardwareProfiles = GetHardwareProfiles();
            int outputFormatIndex = GetOutputFormatIndex(featureSetting, featureDefinition);
            VRCFTParameterOutputFormat outputFormat = GetOutputFormat(featureDefinition, outputFormatIndex);
            HardwareSupportStatus status = HardwareSupportDisplay.GetOutputFormatStatus(trackingHardwareProfiles, outputFormat);
            string featureName = OutputFormatDisplay.FormatFeatureName(featureSetting, featureDefinition);
            string outputFormatName = featureDefinition == null || outputFormat == null ? string.Empty : OutputFormatDisplay.FormatChoice(featureDefinition, outputFormat.DisplayName);

            label.text = StatusIndicatorText;
            label.tooltip = HardwareSupportDisplay.FormatStatusTooltip(trackingHardwareProfiles, featureName, outputFormatName, outputFormat, status);
            FaceTrackingInspectorStyles.ApplyFeatureSettingCell(label, index);
            FaceTrackingInspectorStyles.ApplySupportStatusText(label, status);
        }

        private void BindOutputFormatCell(VisualElement element, int index)
        {
            DropdownField dropdownField = element.Q<DropdownField>(OutputFormatDropdownName);
            if (dropdownField == null)
            {
                return;
            }

            SerializedProperty featureSetting = GetFeatureSettingElementAt(index);
            FaceTrackingFeatureDefinition featureDefinition = GetFeatureDefinition(featureSetting);
            SerializedProperty syncModeProperty = featureSetting.FindPropertyRelative(nameof(FaceTrackingFeatureSetting.syncMode));
            SerializedProperty outputFormatIndexProperty = featureSetting.FindPropertyRelative(nameof(FaceTrackingFeatureSetting.outputFormatIndex));
            ParameterSyncMode syncMode = (ParameterSyncMode)syncModeProperty.intValue;
            int outputFormatIndex = OutputFormatDisplay.ClampIndex(featureDefinition, outputFormatIndexProperty.intValue);
            VRCFTParameterOutputFormat outputFormat = GetOutputFormat(featureDefinition, outputFormatIndex);
            List<string> choices = OutputFormatDisplay.GetChoices(featureDefinition);
            bool usesGeneratedParameters = VRCFTParameterOutputFormatUtility.UsesGeneratedParameters(outputFormat);

            dropdownField.UnregisterValueChangedCallback(OnOutputFormatChanged);
            dropdownField.userData = index;
            dropdownField.choices = choices;
            dropdownField.formatListItemCallback = value => OutputFormatDisplay.FormatChoice(featureDefinition, value);
            dropdownField.formatSelectedValueCallback = value => OutputFormatDisplay.FormatChoice(featureDefinition, value);
            dropdownField.SetEnabled(choices.Count > 0 && (syncMode != ParameterSyncMode.None || !usesGeneratedParameters));
            dropdownField.SetValueWithoutNotify(OutputFormatDisplay.GetValue(featureDefinition, outputFormatIndex));
            dropdownField.tooltip = syncMode != ParameterSyncMode.None || !usesGeneratedParameters ? OutputFormatDisplay.FormatTooltip(featureDefinition, outputFormatIndex) : T("tooltip.not_used", "Not used");
            dropdownField.RegisterValueChangedCallback(OnOutputFormatChanged);
            FaceTrackingInspectorStyles.ApplyFeatureSettingCell(element, index);
        }

        private void BindSyncModeCell(VisualElement element, int index)
        {
            DropdownField dropdownField = element.Q<DropdownField>(SyncModeDropdownName);
            if (dropdownField == null)
            {
                return;
            }

            SerializedProperty featureSetting = GetFeatureSettingElementAt(index);
            SerializedProperty syncModeProperty = featureSetting.FindPropertyRelative(nameof(FaceTrackingFeatureSetting.syncMode));
            ParameterSyncMode syncMode = (ParameterSyncMode)syncModeProperty.intValue;
            FaceTrackingFeatureDefinition featureDefinition = GetFeatureDefinition(featureSetting);
            int outputFormatIndex = GetOutputFormatIndex(featureSetting, featureDefinition);
            VRCFTParameterOutputFormat outputFormat = GetOutputFormat(featureDefinition, outputFormatIndex);
            bool usesGeneratedParameters = VRCFTParameterOutputFormatUtility.UsesGeneratedParameters(outputFormat);

            dropdownField.UnregisterValueChangedCallback(OnSyncModeChanged);
            dropdownField.userData = index;
            dropdownField.choices = usesGeneratedParameters ? ParameterSyncModeDisplay.GetChoices() : ParameterSyncModeDisplay.GetNativeChoices();
            dropdownField.formatListItemCallback = ParameterSyncModeDisplay.FormatChoice;
            dropdownField.formatSelectedValueCallback = ParameterSyncModeDisplay.FormatChoice;
            dropdownField.SetEnabled(usesGeneratedParameters);
            dropdownField.SetValueWithoutNotify(usesGeneratedParameters ? ParameterSyncModeDisplay.GetDisplayName(syncMode) : ParameterSyncModeDisplay.GetNativeDisplayName());
            dropdownField.tooltip = usesGeneratedParameters ? ParameterSyncModeDisplay.FormatTooltip(syncMode) : ParameterSyncModeDisplay.FormatNativeTooltip();
            dropdownField.RegisterValueChangedCallback(OnSyncModeChanged);
            FaceTrackingInspectorStyles.ApplyFeatureSettingCell(element, index);
        }

        private SerializedProperty GetFeatureSettingElementAt(int index)
        {
            return serializedObject.FindProperty(nameof(USTLFaceTracking.featureSettings)).GetArrayElementAtIndex(index);
        }

        private static FaceTrackingFeatureDefinition GetFeatureDefinition(SerializedProperty element)
        {
            FaceTrackingFeature feature = GetFeature(element);
            FaceTrackingFeatureDefinition.All.TryGetValue(feature, out FaceTrackingFeatureDefinition featureDefinition);
            return featureDefinition;
        }

        private static FaceTrackingFeature GetFeature(SerializedProperty element)
        {
            SerializedProperty featureProperty = element.FindPropertyRelative(nameof(FaceTrackingFeatureSetting.feature));
            return (FaceTrackingFeature)featureProperty.intValue;
        }

        private static int GetOutputFormatIndex(SerializedProperty featureSetting, FaceTrackingFeatureDefinition featureDefinition)
        {
            SerializedProperty outputFormatIndexProperty = featureSetting.FindPropertyRelative(nameof(FaceTrackingFeatureSetting.outputFormatIndex));
            return OutputFormatDisplay.ClampIndex(featureDefinition, outputFormatIndexProperty.intValue);
        }

        private static VRCFTParameterOutputFormat GetOutputFormat(FaceTrackingFeatureDefinition featureDefinition, int outputFormatIndex)
        {
            if (featureDefinition == null || featureDefinition.OutputFormats.Length == 0)
            {
                return null;
            }

            return featureDefinition.OutputFormats[OutputFormatDisplay.ClampIndex(featureDefinition, outputFormatIndex)];
        }

        private void RefreshFeatureSettingItems()
        {
            SerializedProperty featureSettings = serializedObject.FindProperty(nameof(USTLFaceTracking.featureSettings));
            _featureSettingItems.Clear();
            for (int i = 0; i < featureSettings.arraySize; i++)
            {
                _featureSettingItems.Add(i);
            }

            if (_featureSettingView != null)
            {
                _featureSettingView.itemsSource = _featureSettingItems;
                _featureSettingView.Rebuild();
            }
        }

        private void OnOutputFormatChanged(ChangeEvent<string> evt)
        {
            if (evt.currentTarget is not DropdownField dropdownField || dropdownField.userData is not int index)
            {
                return;
            }

            serializedObject.Update();

            SerializedProperty featureSetting = GetFeatureSettingElementAt(index);
            FaceTrackingFeatureDefinition featureDefinition = GetFeatureDefinition(featureSetting);
            int outputFormatIndex = OutputFormatDisplay.FindIndex(featureDefinition, evt.newValue);
            if (outputFormatIndex < 0)
            {
                return;
            }

            SerializedProperty outputFormatIndexProperty = featureSetting.FindPropertyRelative(nameof(FaceTrackingFeatureSetting.outputFormatIndex));
            if (outputFormatIndexProperty.intValue == outputFormatIndex)
            {
                return;
            }

            outputFormatIndexProperty.intValue = outputFormatIndex;
            VRCFTParameterOutputFormat outputFormat = GetOutputFormat(featureDefinition, outputFormatIndex);
            SerializedProperty syncModeProperty = featureSetting.FindPropertyRelative(nameof(FaceTrackingFeatureSetting.syncMode));
            if (!VRCFTParameterOutputFormatUtility.UsesGeneratedParameters(outputFormat))
            {
                syncModeProperty.intValue = (int)ParameterSyncMode.None;
            }
            else if ((ParameterSyncMode)syncModeProperty.intValue == ParameterSyncMode.None)
            {
                syncModeProperty.intValue = (int)ParameterSyncMode.LocalOnly;
            }

            serializedObject.ApplyModifiedProperties();
            dropdownField.tooltip = OutputFormatDisplay.FormatTooltip(featureDefinition, outputFormatIndex);
            _featureSettingView?.Rebuild();
            _blendShapeAssignmentView?.Rebuild();
            RefreshSyncParameterUsageLabel();
        }

        private void OnSyncModeChanged(ChangeEvent<string> evt)
        {
            if (evt.currentTarget is not DropdownField dropdownField || dropdownField.userData is not int index)
            {
                return;
            }

            serializedObject.Update();

            SerializedProperty featureSetting = GetFeatureSettingElementAt(index);
            SerializedProperty syncModeProperty = featureSetting.FindPropertyRelative(nameof(FaceTrackingFeatureSetting.syncMode));
            ParameterSyncMode currentSyncMode = (ParameterSyncMode)syncModeProperty.intValue;
            if (!ParameterSyncModeDisplay.TryFind(evt.newValue, out ParameterSyncMode syncMode))
            {
                dropdownField.SetValueWithoutNotify(ParameterSyncModeDisplay.GetDisplayName(currentSyncMode));
                dropdownField.tooltip = ParameterSyncModeDisplay.FormatTooltip(currentSyncMode);
                return;
            }

            if (syncModeProperty.intValue == (int)syncMode)
            {
                return;
            }

            syncModeProperty.intValue = (int)syncMode;
            serializedObject.ApplyModifiedProperties();
            dropdownField.tooltip = ParameterSyncModeDisplay.FormatTooltip(syncMode);
            _featureSettingView?.Rebuild();
            _blendShapeAssignmentView?.Rebuild();
            RefreshSyncParameterUsageLabel();
        }
    }
}

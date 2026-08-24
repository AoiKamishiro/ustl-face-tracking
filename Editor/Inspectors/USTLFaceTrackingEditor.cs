using System;
using System.Collections.Generic;
using System.Linq;
using nadena.dev.modular_avatar.core.editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using USTL.Core.Editor;

namespace USTL.FaceTracking.Editor
{
    [CustomEditor(typeof(USTLFaceTracking))]
    internal sealed class USTLFaceTrackingEditor : USTLEditorBase
    {
        private const string FeatureSettingsFoldoutName = "feature-settings";
        private const string BlendShapeAssignmentFoldoutName = "blend-shape-assignments";

        [SerializeField] private bool featureSettingsFoldoutOpen;
        [SerializeField] private bool blendShapeAssignmentFoldoutOpen;

        private BlendShapeSettingView BlendShapeSettingView { get; set; }
        private FeatureSettingView FeatureSettingView { get; set; }
        private IntegerField SyncParameterUsageLabel { get; set; }


        private SerializedProperty SpFaceMeshRendererField => serializedObject.FindProperty(nameof(USTLFaceTracking.faceMeshRenderer));
        private SerializedProperty SpTrackingHardwareField => serializedObject.FindProperty(nameof(USTLFaceTracking.trackingHardwareProfiles));
        private SerializedProperty SpBlendShapeAssignments => serializedObject.FindProperty(nameof(USTLFaceTracking.blendShapeSettings));
        private SerializedProperty SpFeatureSettings => serializedObject.FindProperty(nameof(USTLFaceTracking.featureSettings));

        protected override void BuildInspectorGUI(VisualElement root)
        {
            // FaceMeshRendererField

            FaceMeshRendererField faceMeshRendererField = new()
            {
                bindingPath = nameof(USTLFaceTracking.faceMeshRenderer),
            };
            faceMeshRendererField.AddToClassList(USTLLocalizer.TrClassName);
            faceMeshRendererField.AddToClassList($"{USTLLocalizer.TrClassNamePrefix}__ft__field__face_mesh_renderer");
            faceMeshRendererField.RegisterValueChangedCallback(_ => Refresh());
            root.Add(faceMeshRendererField);

            // HardwareProfileField

            HardwareProfileField hardwareProfileField = new()
            {
                bindingPath = nameof(USTLFaceTracking.trackingHardwareProfiles),
            };
            hardwareProfileField.AddToClassList(USTLLocalizer.TrClassName);
            hardwareProfileField.AddToClassList($"{USTLLocalizer.TrClassNamePrefix}__ft__field__tracking_hardware");
            hardwareProfileField.RegisterValueChangedCallback(_ => Refresh());
            root.Add(hardwareProfileField);

            // FeatureSettingView

            FeatureSettingView featureSettingView = new(BindCell_FeatureSettings_Feature, BindCell_FeatureSettings_HardwareSupport, BindCell_FeatureSettings_OutputFormat, BindCell_FeatureSettings_SyncMode)
            {
                itemsSource = Enumerable.Range(0, SpFeatureSettings.arraySize).ToList(),
            };
            featureSettingView.AddToClassList(USTLLocalizer.TrClassName);
            featureSettingView.AddToClassList($"{USTLLocalizer.TrClassNamePrefix}__ft__field__feature_settings");
            featureSettingView.OnOutputFormatChanged += _ => Refresh();
            featureSettingView.OnSyncModeChanged += _ => Refresh();
            FeatureSettingView = featureSettingView;

            Foldout featureFoldout = new()
            {
                name = FeatureSettingsFoldoutName,
                value = featureSettingsFoldoutOpen,
            };
            featureFoldout.AddToClassList(USTLLocalizer.TrClassName);
            featureFoldout.AddToClassList($"{USTLLocalizer.TrClassNamePrefix}__ft__section__feature_settings");
            featureFoldout.RegisterValueChangedCallback(evt => featureSettingsFoldoutOpen = evt.newValue);
            featureFoldout.Add(featureSettingView);
            root.Add(featureFoldout);

            // BlendShapeSettingView
            BlendShapeSettingView blendShapeSettingView = new(BindCell_BlendShapeSettings_Expression, BindCell_BlendShapeSettings_HardwareSupport, BindCell_BlendShapeSettings_BlendShape, BindCell_BlendShapeSettings_MaxValue)
            {
                itemsSource = Enumerable.Range(0, SpBlendShapeAssignments.arraySize).ToList(),
            };
            blendShapeSettingView.AddToClassList(USTLLocalizer.TrClassName);
            blendShapeSettingView.AddToClassList($"{USTLLocalizer.TrClassNamePrefix}__ft__field__blend_shape_settings");
            blendShapeSettingView.OnAssignmentChanged += _ => Refresh();
            BlendShapeSettingView = blendShapeSettingView;

            Foldout blendShapeFold = new()
            {
                name = BlendShapeAssignmentFoldoutName,
                value = blendShapeAssignmentFoldoutOpen,
            };
            blendShapeFold.AddToClassList(USTLLocalizer.TrClassName);
            blendShapeFold.AddToClassList($"{USTLLocalizer.TrClassNamePrefix}__ft__section__blend_shape_settings");
            blendShapeFold.RegisterValueChangedCallback(evt => blendShapeAssignmentFoldoutOpen = evt.newValue);
            blendShapeFold.Add(blendShapeSettingView);
            root.Add(blendShapeFold);

            // SyncParameterUsageLabel

            IntegerField parameterUsageField = new();
            parameterUsageField.SetEnabled(false);
            parameterUsageField.value = VRCParameterUtility.CalculateUsage(target as USTLFaceTracking);
            parameterUsageField.AddToClassList(USTLLocalizer.TrClassName);
            parameterUsageField.AddToClassList($"{USTLLocalizer.TrClassNamePrefix}__ft__field__parameter_usage");
            root.Add(parameterUsageField);
            SyncParameterUsageLabel = parameterUsageField;

            // LanguageSwitcherElement

            VisualElement languageSwitcher = new LanguageSwitcherElement();
            languageSwitcher.style.marginTop = 4;
            root.Add(languageSwitcher);
        }

        private static Color SupportedHardwareStatusIndicator(HardwareSupportStatus status)
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

        #region Refresh

        private bool _refreshQueued;

        private void Refresh()
        {
            if (_refreshQueued)
            {
                return;
            }

            _refreshQueued = true;
            Root.schedule.Execute(DoRefresh);
        }

        private void DoRefresh()
        {
            _refreshQueued = false;

            serializedObject.Update();
            SyncParameterUsageLabel.value = VRCParameterUtility.CalculateUsage(target as USTLFaceTracking);
            BlendShapeSettingView.RefreshItems();
            FeatureSettingView.RefreshItems();
            USTLLocalizer.Localize(Root);
        }

        #endregion

        #region BindCalls Feature

        private void BindCell_FeatureSettings_Feature(Label label, int index)
        {
            FeatureSetting setting = new(SpFeatureSettings, index);

            USTLLocalizer.RemoveLocalizeClass(label);
            USTLLocalizer.AddLocalizeClass(label, $"{USTLLocalizer.TrClassNamePrefix}__ft__enum__feature__{setting.Feature.ToString()}");

            USTLLocalizer.Localize(label);
        }

        private void BindCell_FeatureSettings_HardwareSupport(Label label, int index)
        {
            FeatureSetting setting = new(SpFeatureSettings, index);
            TrackingHardwareSetting hwSetting = new(SpTrackingHardwareField);
            HardwareSupportStatus status = HardwareSupportStatus.Unknown;
            foreach (SupportedHardwareDefinition profile in hwSetting.HardwareSupportProfiles)
            {
                if (status == HardwareSupportStatus.Full)
                {
                    break;
                }

                HardwareSupportStatus tmp = profile.GetStatus(setting.OutputFormat);
                if (status > tmp)
                {
                    status = tmp;
                }
            }

            label.style.color = SupportedHardwareStatusIndicator(status);
        }

        private void BindCell_FeatureSettings_OutputFormat(DropdownField dropdownField, int index)
        {
            FeatureSetting setting = new(SpFeatureSettings, index);
            FaceTrackingFeatureDefinition featureDefinition = setting.FeatureDefinition;

            List<string> choices = new();

            int formatIndex = 0;
            if (featureDefinition != null)
            {
                choices.AddRange(featureDefinition.OutputFormats.Select(outputFormat => outputFormat.Id.ToString()));
                formatIndex = featureDefinition.IndexOfOutputFormat(setting.OutputFormat.Id);
            }

            dropdownField.UnregisterValueChangedCallback(ChangeCallback_FeatureSettings_OnOutputFormatChanged);
            dropdownField.userData = index;
            dropdownField.choices = choices;
            dropdownField.formatListItemCallback = FormatString;
            dropdownField.formatSelectedValueCallback = FormatString;
            dropdownField.SetValueWithoutNotify(formatIndex >= 0 && formatIndex < choices.Count ? choices[formatIndex] : string.Empty);
            dropdownField.RegisterValueChangedCallback(ChangeCallback_FeatureSettings_OnOutputFormatChanged);
            USTLLocalizer.RemoveLocalizeClass(dropdownField);
            USTLLocalizer.AddLocalizeClass(dropdownField);
            return;

            string FormatString(string key)
            {
                string localized = Tr($"{USTLLocalizer.TrClassNamePrefix}__ft__enum__set_id__{key}", key);
                return string.IsNullOrEmpty(localized) ? key : localized;
            }
        }

        private void BindCell_FeatureSettings_SyncMode(DropdownField dropdownField, int index)
        {
            FeatureSetting setting = new(SpFeatureSettings, index);
            ParameterSyncMode syncMode = setting.SyncMode;
            bool isVrcMode = setting.Feature is FaceTrackingFeature.EyeDirection or FaceTrackingFeature.EyeLid && setting.OutputFormatId == VRCFTParameterSetId.VRChatNative;
            dropdownField.UnregisterValueChangedCallback(ChangeCallback_FeatureSettings_OnSyncModeChanged);
            dropdownField.userData = index;
            dropdownField.choices = EnumUtility.GetAllElements<ParameterSyncMode>().Select(mode => mode.ToString()).ToList();
            dropdownField.formatListItemCallback = FormatString;
            dropdownField.formatSelectedValueCallback = FormatString;
            dropdownField.SetValueWithoutNotify(syncMode.ToString());
            dropdownField.RegisterValueChangedCallback(ChangeCallback_FeatureSettings_OnSyncModeChanged);
            dropdownField.SetEnabled(!isVrcMode);
            USTLLocalizer.RemoveLocalizeClass(dropdownField);
            USTLLocalizer.AddLocalizeClass(dropdownField);
            if (isVrcMode && syncMode != ParameterSyncMode.None)
            {
                setting.SyncModeProperty.intValue = (int)ParameterSyncMode.None;
                serializedObject.ApplyModifiedProperties();
                syncMode = ParameterSyncMode.None;
                dropdownField.SetValueWithoutNotify(syncMode.ToString());
            }

            return;

            string FormatString(string key)
            {
                string localized = Tr($"{USTLLocalizer.TrClassNamePrefix}__ft__enum__sync_mode__{key}", key);
                return string.IsNullOrEmpty(localized) ? key : localized;
            }
        }


        private void ChangeCallback_FeatureSettings_OnOutputFormatChanged(ChangeEvent<string> evt)
        {
            if (evt.currentTarget is not DropdownField { userData: int index, } dropdownField)
            {
                return;
            }

            serializedObject.Update();

            FeatureSetting setting = new(SpFeatureSettings, index);
            FaceTrackingFeatureDefinition featureDefinition = setting.FeatureDefinition;
            int currentFormatIndex = setting.OutputFormatIndex;
            int newFormatIndex = dropdownField.index;
            if (featureDefinition == null || newFormatIndex < 0 || featureDefinition.OutputFormats.Count <= newFormatIndex)
            {
                dropdownField.SetValueWithoutNotify(currentFormatIndex >= 0 && currentFormatIndex < dropdownField.choices.Count ? dropdownField.choices[currentFormatIndex] : string.Empty);
                return;
            }

            if (newFormatIndex == currentFormatIndex)
            {
                return;
            }

            setting.OutputFormatProperty.intValue = (int)featureDefinition.OutputFormats[newFormatIndex].Id;
            serializedObject.ApplyModifiedProperties();
        }

        private void ChangeCallback_FeatureSettings_OnSyncModeChanged(ChangeEvent<string> evt)
        {
            if (evt.currentTarget is not DropdownField { userData: int index, } dropdownField)
            {
                return;
            }

            serializedObject.Update();

            FeatureSetting setting = new(SpFeatureSettings, index);
            ParameterSyncMode currentSyncMode = setting.SyncMode;
            if (!Enum.TryParse(evt.newValue, out ParameterSyncMode newSyncMode))
            {
                dropdownField.SetValueWithoutNotify(currentSyncMode.ToString());
                return;
            }

            if (!EnumUtility.GetAllElements<ParameterSyncMode>().Contains(newSyncMode))
            {
                dropdownField.SetValueWithoutNotify(currentSyncMode.ToString());
                return;
            }

            if (newSyncMode == currentSyncMode)
            {
                return;
            }

            setting.SyncModeProperty.intValue = (int)newSyncMode;
            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region BindCalls BlendShape

        private void BindCell_BlendShapeSettings_Expression(Label label, int index)
        {
            BlendShapeSetting setting = new(SpBlendShapeAssignments, index);
            label.text = setting.Expression.ToString();
            label.tooltip = label.text;
        }

        private void BindCell_BlendShapeSettings_HardwareSupport(Label label, int index)
        {
            BlendShapeSetting setting = new(SpBlendShapeAssignments, index);
            TrackingHardwareSetting hwSetting = new(SpTrackingHardwareField);
            HardwareSupportStatus status = HardwareSupportStatus.Unknown;
            foreach (SupportedHardwareDefinition profile in hwSetting.HardwareSupportProfiles)
            {
                if (status == HardwareSupportStatus.Full)
                {
                    break;
                }

                HardwareSupportStatus tmp = profile.GetStatus(setting.Expression);
                if (status > tmp)
                {
                    status = tmp;
                }
            }

            label.style.color = SupportedHardwareStatusIndicator(status);
        }

        private void BindCell_BlendShapeSettings_BlendShape(DropdownField field, int index)
        {
            BlendShapeSetting setting = new(SpBlendShapeAssignments, index);
            FaceMeshSetting faceSetting = new(SpFaceMeshRendererField);
            IReadOnlyList<string> blendShapes = faceSetting.BlendShapes;
            List<string> choices = GetChoicesForValue(blendShapes, setting.BlendShape);
            field.Unbind();
            field.choices = choices;
            field.BindProperty(setting.BlendShapeProperty);
            field.SetEnabled(IsBlendShapeSettingEditable(setting.Expression));

            TextElement textElement = field.Q<TextElement>(className: BasePopupField<string, string>.textUssClassName);

            textElement.style.color = !blendShapes.Contains(setting.BlendShape) ? new Color(1f, 0.25f, 0.25f) : StyleKeyword.Null;

            return;

            List<string> GetChoicesForValue(IReadOnlyList<string> list, string currentValue)
            {
                if (string.IsNullOrEmpty(currentValue) || list.Contains(currentValue))
                {
                    return list.ToList();
                }

                List<string> newChoices = new(list.Count + 1);
                newChoices.AddRange(list);
                newChoices.Add(currentValue);
                return newChoices;
            }
        }

        private void BindCell_BlendShapeSettings_MaxValue(RangeFloatField field, int index)
        {
            BlendShapeSetting setting = new(SpBlendShapeAssignments, index);
            field.Unbind();
            field.BindProperty(setting.MaxValueProperty);
            field.SetEnabled(IsBlendShapeSettingEditable(setting.Expression));
        }

        private bool IsBlendShapeSettingEditable(UnifiedExpression expression)
        {
            if (expression == UnifiedExpression.None)
            {
                return false;
            }

            for (int i = 0; i < SpFeatureSettings.arraySize; i++)
            {
                FeatureSetting featureSetting = new(SpFeatureSettings, i);
                if (featureSetting.SyncMode == ParameterSyncMode.None || featureSetting.OutputFormat == null)
                {
                    continue;
                }

                foreach (VRCFTParameter parameter in featureSetting.OutputFormat.Parameters)
                {
                    if (!VRCFTParameterDefinition.All.TryGetValue(parameter, out VRCFTParameterDefinition definition))
                    {
                        continue;
                    }

                    foreach (ExpressionWeightTarget tgt in definition.ExpressionTargets)
                    {
                        if (tgt.Expression == expression)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        #endregion

        #region Wrapper

        private readonly struct FaceMeshSetting
        {
            public FaceMeshSetting(SerializedProperty serializedProperty)
            {
                FaceMeshProperty = serializedProperty;
            }

            public SerializedProperty FaceMeshProperty { get; }
            public SkinnedMeshRenderer FaceMeshRenderer => FaceMeshProperty.objectReferenceValue as SkinnedMeshRenderer;
            public Mesh FaceMesh => FaceMeshRenderer?.sharedMesh;
            public IReadOnlyList<string> BlendShapes => MeshUtility.GetBlendShapeNames(FaceMesh);
        }

        private readonly struct TrackingHardwareSetting
        {
            public TrackingHardwareSetting(SerializedProperty serializedProperty)
            {
                TrackingHardwareProperty = serializedProperty;
            }

            public SerializedProperty TrackingHardwareProperty { get; }
            public SupportedHardwares TrackingHardware => (SupportedHardwares)TrackingHardwareProperty.intValue;

            public List<SupportedHardwareDefinition> HardwareSupportProfiles
            {
                get
                {
                    List<SupportedHardwareDefinition> profiles = new();
                    foreach (SupportedHardwares hardware in EnumUtility.GetAllElements<SupportedHardwares>())
                    {
                        if (hardware != SupportedHardwares.None && (TrackingHardware & hardware) == hardware)
                        {
                            profiles.Add(SupportedHardwareDefinition.All[hardware]);
                        }
                    }

                    return profiles;
                }
            }
        }

        private readonly struct FeatureSetting
        {
            public FeatureSetting(SerializedProperty arraySerializedProperty, int index)
            {
                FeatureProperty = arraySerializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative(nameof(FaceTracking.FeatureSetting.feature));
                OutputFormatProperty = arraySerializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative(nameof(FaceTracking.FeatureSetting.outputFormatId));
                SyncModeProperty = arraySerializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative(nameof(FaceTracking.FeatureSetting.syncMode));
            }

            public SerializedProperty FeatureProperty { get; }
            public SerializedProperty OutputFormatProperty { get; }
            public SerializedProperty SyncModeProperty { get; }

            public FaceTrackingFeature Feature => (FaceTrackingFeature)FeatureProperty.intValue;

            public FaceTrackingFeatureDefinition FeatureDefinition => FaceTrackingFeatureDefinition.All.GetValueOrDefault(Feature);
            public VRCFTParameterSetId OutputFormatId => (VRCFTParameterSetId)OutputFormatProperty.intValue;
            public VRCFTParameterSet OutputFormat => FeatureDefinition?.GetOutputFormatOrDefault(OutputFormatId);
            public int OutputFormatIndex => FeatureDefinition?.IndexOfOutputFormat(OutputFormatId) ?? -1;

            public ParameterSyncMode SyncMode => (ParameterSyncMode)SyncModeProperty.intValue;
        }

        private readonly struct BlendShapeSetting
        {
            public BlendShapeSetting(SerializedProperty arraySerializedProperty, int index)
            {
                ExpressionProperty = arraySerializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative(nameof(FaceTracking.BlendShapeSetting.expression));
                BlendShapeProperty = arraySerializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative(nameof(FaceTracking.BlendShapeSetting.blendShapeName));
                MaxValueProperty = arraySerializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative(nameof(FaceTracking.BlendShapeSetting.maxValue));
            }

            public SerializedProperty ExpressionProperty { get; }
            public SerializedProperty BlendShapeProperty { get; }
            public SerializedProperty MaxValueProperty { get; }

            public UnifiedExpression Expression => (UnifiedExpression)ExpressionProperty.intValue;
            public string BlendShape => BlendShapeProperty.stringValue;
            public float MaxValue => MaxValueProperty.floatValue;
        }

        #endregion
    }
}

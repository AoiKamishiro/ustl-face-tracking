using System;
using System.Collections.Generic;
using System.Linq;
using nadena.dev.modular_avatar.core.editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace USTL.FaceTracking.Editor
{
    [CustomEditor(typeof(USTLFaceTracking))]
    internal sealed class USTLFaceTrackingEditor : USTLEditorBase
    {
        private const string LocalizationKeyPrefix = "ustl.facetracking.editor.";

        private const string FeatureSettingsFoldoutName = "feature-settings";
        private const string BlendShapeAssignmentFoldoutName = "blend-shape-assignments";

        [SerializeField] private bool featureSettingsFoldoutOpen;
        [SerializeField] private bool blendShapeAssignmentFoldoutOpen;

        private BlendShapeSettingView BlendShapeSettingView { get; set; }
        private FeatureSettingView FeatureSettingView { get; set; }
        private SyncParameterUsageLabel SyncParameterUsageLabel { get; set; }

        private SerializedProperty SpFaceMeshRendererField => serializedObject.FindProperty(nameof(USTLFaceTracking.faceMeshRenderer));
        private SerializedProperty SpTrackingHardwareField => serializedObject.FindProperty(nameof(USTLFaceTracking.trackingHardwareProfiles));
        private SerializedProperty SpBlendshapeAssignments => serializedObject.FindProperty(nameof(USTLFaceTracking.blendshapeSettings));
        private SerializedProperty SpFeatureSettings => serializedObject.FindProperty(nameof(USTLFaceTracking.featureSettings));

        private void OnEnable()
        {
            FeatureSettingsInitializer.EnsureInitialized(serializedObject);
            BlendshapeSettingInitializer.EnsureInitialized(serializedObject);
        }

        private static string Tr(string key, string fallback)
        {
            return LocalizationUtility.S(LocalizationKeyPrefix + key, fallback);
        }


        protected override void BuildInspectorGUI(VisualElement root)
        {
            // FaceMeshRenderField

            FaceMeshRendererField faceMeshRendererField = new()
            {
                bindingPath = nameof(USTLFaceTracking.faceMeshRenderer),
            };
            faceMeshRendererField.RegisterValueChangedCallback(_ => Refresh());
            faceMeshRendererField.OnLangChanged = () => { faceMeshRendererField.label = Tr("field.face_mesh_renderer", "Face Mesh Renderer"); };
            root.Add(faceMeshRendererField);

            // HardwareProfileField

            HardwareProfileField hardwareProfileField = new()
            {
                bindingPath = nameof(USTLFaceTracking.trackingHardwareProfiles),
                LabelText = "Tracking Hardware",
                ButtonTooltip = "Select one or more face-tracking hardware devices used by this avatar.",
            };
            hardwareProfileField.RegisterValueChangedCallback(_ => Refresh());
            hardwareProfileField.OnLangChanged = () =>
            {
                hardwareProfileField.LabelText = Tr("field.tracking_hardware", "Tracking Hardware");
                hardwareProfileField.ButtonTooltip = Tr("tooltip.tracking_hardware", "Select one or more face-tracking hardware devices used by this avatar.");
            };
            root.Add(hardwareProfileField);

            // FeatureSettingView

            FeatureSettingView featureSettingView = new(BindCell_FeatureSettings_Feature, BindCell_FeatureSettings_HardwareSupport, BindCell_FeatureSettings_OutputFormat, BindCell_FeatureSettings_SyncMode)
            {
                itemsSource = Enumerable.Range(0, SpFeatureSettings.arraySize).ToList(),
            };
            featureSettingView.OnOutputFormatChanged += _ => Refresh();
            featureSettingView.OnSyncModeChanged += _ => Refresh();
            featureSettingView.OnLangChanged = () =>
            {
                featureSettingView.Column0Title = Tr("column.feature", "Feature");
                featureSettingView.Column1Title = Tr("column.hardware_support_short", "HW");
                featureSettingView.Column2Title = Tr("column.output_format", "Output Format");
                featureSettingView.Column3Title = Tr("column.sync_mode", "Sync Mode");
                featureSettingView.Rebuild();
            };
            FeatureSettingView = featureSettingView;

            LocalizedFoldout featureFoldout = new()
            {
                name = FeatureSettingsFoldoutName,
                value = featureSettingsFoldoutOpen,
                text = "Feature Settings",
            };
            featureFoldout.RegisterValueChangedCallback(evt => featureSettingsFoldoutOpen = evt.newValue);
            featureFoldout.Add(featureSettingView);
            featureFoldout.OnLangChanged = () => { featureFoldout.text = Tr("section.feature_settings", "Feature Settings"); };
            root.Add(featureFoldout);

            // BlendShapeSettingView
            BlendShapeSettingView blendShapeSettingView = new(BindCell_BlendshapeSettings_Expression, BindCell_BlendshapeSettings_HardwareSupport, BindCell_BlendshapeSettings_Blendshape, BindCell_BlendshapeSettings_maxValue)
            {
                itemsSource = Enumerable.Range(0, SpBlendshapeAssignments.arraySize).ToList(),
            };
            blendShapeSettingView.OnAssignmentChanged += _ => Refresh();
            blendShapeSettingView.OnLangChanged = () =>
            {
                blendShapeSettingView.Column0Title = Tr("column.unified_expression", "Unified Expression");
                blendShapeSettingView.Column1Title = Tr("column.hardware_support_short", "HW");
                blendShapeSettingView.Column2Title = Tr("column.blend_shape", "Blend Shape");
                blendShapeSettingView.Column3Title = Tr("column.max_value", "Max Value");
                blendShapeSettingView.Rebuild();
            };
            BlendShapeSettingView = blendShapeSettingView;

            LocalizedFoldout blendshapeFold = new()
            {
                name = BlendShapeAssignmentFoldoutName,
                value = blendShapeAssignmentFoldoutOpen,
                text = "Blend Shape Assignments",
            };
            blendshapeFold.RegisterValueChangedCallback(evt => blendShapeAssignmentFoldoutOpen = evt.newValue);
            blendshapeFold.Add(blendShapeSettingView);
            blendshapeFold.OnLangChanged = () => { blendshapeFold.text = Tr("section.blend_shape_assignments", "Blend Shape Assignments"); };
            root.Add(blendshapeFold);

            // SyncParameterUsageLabel

            SyncParameterUsageLabel syncParameterUsageLabel = new(target as USTLFaceTracking);
            syncParameterUsageLabel.OnLangChanged = () =>
            {
                syncParameterUsageLabel.SummaryFormat = Tr("summary.sync_parameter_usage", "Sync Parameter Usage: {0} bits ({1}/{2} parameters, {3} without blend shape assignments)");
                syncParameterUsageLabel.Rebuild();
            };
            SyncParameterUsageLabel = syncParameterUsageLabel;
            root.Add(syncParameterUsageLabel);

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

        #region Reflesh

        private void Refresh()
        {
            serializedObject.Update();
            SyncParameterUsageLabel.Rebuild();
            BlendShapeSettingView.Rebuild();
            FeatureSettingView.Rebuild();
        }

        #endregion

        #region BindCalls Feature

        private void BindCell_FeatureSettings_Feature(LocalizationLabel label, int index)
        {
            FeatureSetting setting = new(SpFeatureSettings, index);

            string text = "Invalid";
            if (setting.FeatureDefinition != null)
            {
                text = Tr(setting.FeatureDefinition.TranslationKey, setting.FeatureDefinition.DisplayName);
            }

            label.text = text;
            label.tooltip = text;
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

        private void BindCell_FeatureSettings_OutputFormat(LocalizationDropdownField dropdownField, int index)
        {
            FeatureSetting setting = new(SpFeatureSettings, index);
            FaceTrackingFeatureDefinition featureDefinition = setting.FeatureDefinition;

            List<string> choices = new();

            int formatIndex = 0;
            if (featureDefinition != null)
            {
                choices.AddRange(featureDefinition.OutputFormats.Select(outputFormat => Tr($"output_format.{outputFormat.Id}", outputFormat.DisplayName)));
                formatIndex = featureDefinition.IndexOfOutputFormat(setting.OutputFormat.Id);
            }

            dropdownField.UnregisterValueChangedCallback(ChangeCallback_FeatureSettings_OnOutputFormatChanged);
            dropdownField.userData = index;
            dropdownField.choices = choices;

            dropdownField.SetValueWithoutNotify(choices[formatIndex]);
            dropdownField.RegisterValueChangedCallback(ChangeCallback_FeatureSettings_OnOutputFormatChanged);
        }

        private void BindCell_FeatureSettings_SyncMode(EnumField enumField, int index)
        {
            FeatureSetting setting = new(SpFeatureSettings, index);
            ParameterSyncMode syncMode = setting.SyncMode;

            enumField.UnregisterValueChangedCallback(ChangeCallback_FeatureSettings_OnSyncModeChanged);
            enumField.userData = index;

            if (setting.Feature is FaceTrackingFeature.EyeDirection or FaceTrackingFeature.EyeLid && setting.OutputFormatId == VRCFTParameterSetId.VRChatNative)
            {
                if (syncMode != ParameterSyncMode.None)
                {
                    setting.SyncModeProperty.intValue = (int)ParameterSyncMode.None;
                    serializedObject.ApplyModifiedProperties();
                    syncMode = ParameterSyncMode.None;
                }

                enumField.SetEnabled(false);
            }
            else
            {
                enumField.SetEnabled(true);
            }

            enumField.SetValueWithoutNotify(syncMode);
            enumField.RegisterValueChangedCallback(ChangeCallback_FeatureSettings_OnSyncModeChanged);
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

        private void ChangeCallback_FeatureSettings_OnSyncModeChanged(ChangeEvent<Enum> evt)
        {
            if (evt.currentTarget is not EnumField { userData: int index, } enumField)
            {
                return;
            }

            serializedObject.Update();

            FeatureSetting setting = new(SpFeatureSettings, index);
            ParameterSyncMode currentSyncMode = setting.SyncMode;
            if (evt.newValue is not ParameterSyncMode newSyncMode)
            {
                enumField.SetValueWithoutNotify(currentSyncMode);
                return;
            }

            if (!FaceTrackingEditorUtility.AllSyncModes.Contains(newSyncMode))
            {
                enumField.SetValueWithoutNotify(currentSyncMode);
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

        #region BindCalls Blendshape

        private void BindCell_BlendshapeSettings_Expression(Label label, int index)
        {
            BlendshapeSetting setting = new(SpBlendshapeAssignments, index);
            label.text = setting.Expression.ToString();
            label.tooltip = label.text;
        }

        private void BindCell_BlendshapeSettings_HardwareSupport(Label label, int index)
        {
            BlendshapeSetting setting = new(SpBlendshapeAssignments, index);
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

        private void BindCell_BlendshapeSettings_Blendshape(DropdownField field, int index)
        {
            BlendshapeSetting setting = new(SpBlendshapeAssignments, index);
            FaceMeshSetting faceSetting = new(SpFaceMeshRendererField);
            IReadOnlyList<string> blendshapes = faceSetting.Blendshapes;
            List<string> choices = GetChoicesForValue(blendshapes, setting.Blendshape);
            field.Unbind();
            field.choices = choices;
            field.BindProperty(setting.BlendshapeProperty);

            TextElement textElement = field.Q<TextElement>(className: BasePopupField<string, string>.textUssClassName);

            textElement.style.color = !blendshapes.Contains(setting.Blendshape) ? new Color(1f, 0.25f, 0.25f) : StyleKeyword.Null;

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

        private void BindCell_BlendshapeSettings_maxValue(RangeFloatField field, int index)
        {
            BlendshapeSetting setting = new(SpBlendshapeAssignments, index);
            field.Unbind();
            field.BindProperty(setting.MaxValueProperty);
        }

        #endregion

        #region Weapper

        private readonly struct FaceMeshSetting
        {
            public FaceMeshSetting(SerializedProperty serializedProperty)
            {
                FaceMeshProperty = serializedProperty;
            }

            public SerializedProperty FaceMeshProperty { get; }
            public SkinnedMeshRenderer FaceMeshRenderer => FaceMeshProperty.objectReferenceValue as SkinnedMeshRenderer;
            public Mesh FaceMesh => FaceMeshRenderer?.sharedMesh;
            public IReadOnlyList<string> Blendshapes => MeshUtility.GetBlendShapeNames(FaceMesh);
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
                    foreach (SupportedHardwares hardware in FaceTrackingEditorUtility.AllHardwares)
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

        private readonly struct BlendshapeSetting
        {
            public BlendshapeSetting(SerializedProperty arraySerializedProperty, int index)
            {
                ExpressionProperty = arraySerializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative(nameof(FaceTracking.BlendshapeSetting.expression));
                BlendshapeProperty = arraySerializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative(nameof(FaceTracking.BlendshapeSetting.blendShapeName));
                MaxValueProperty = arraySerializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative(nameof(FaceTracking.BlendshapeSetting.maxValue));
            }

            public SerializedProperty ExpressionProperty { get; }
            public SerializedProperty BlendshapeProperty { get; }
            public SerializedProperty MaxValueProperty { get; }

            public UnifiedExpression Expression => (UnifiedExpression)ExpressionProperty.intValue;
            public string Blendshape => BlendshapeProperty.stringValue;
            public float MaxValue => MaxValueProperty.floatValue;
        }

        #endregion
    }
}

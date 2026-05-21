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
        private const string FeatureSettingsFoldoutName = "feature-settings";
        private const string BlendShapeAssignmentFoldoutName = "blend-shape-assignments";

        [SerializeField] private bool featureSettingsFoldoutOpen;
        [SerializeField] private bool blendShapeAssignmentFoldoutOpen;

        private BlendShapeSettingView BlendShapeSettingView { get; set; }
        private FeatureSettingView FeatureSettingView { get; set; }
        private SyncParameterUsageLabel SyncParameterUsageLabel { get; set; }

        private SerializedProperty SpFaceMeshRendererField => serializedObject.FindProperty(nameof(USTLFaceTracking.faceMeshRenderer));
        private SerializedProperty SpTrackingHardwareField => serializedObject.FindProperty(nameof(USTLFaceTracking.trackingHardwareProfiles));
        private SerializedProperty SpBlendshapeAssignments => serializedObject.FindProperty(nameof(USTLFaceTracking.blendShapeAssignments));
        private SerializedProperty SpFeatureSettings => serializedObject.FindProperty(nameof(USTLFaceTracking.featureSettings));

        private void OnEnable()
        {
            FeatureSettingsInitializer.EnsureInitialized(serializedObject);
            BlendShapeAssignmentInitializer.EnsureInitialized(serializedObject);
        }


        protected override void BuildInspectorGUI(VisualElement root)
        {
            // FaceMeshRenderField

            FaceMeshRendererField faceMeshRendererField = new()
            {
                bindingPath = nameof(USTLFaceTracking.faceMeshRenderer),
            };
            faceMeshRendererField.RegisterValueChangedCallback(_ => Refresh());
            faceMeshRendererField.OnLangChanged += () => { faceMeshRendererField.label = T("field.face_mesh_renderer", "Face Mesh Renderer"); };
            root.Add(faceMeshRendererField);

            // HardwareProfileField

            HardwareProfileField hardwareProfileField = new()
            {
                bindingPath = nameof(USTLFaceTracking.trackingHardwareProfiles),
                LabelText = "Tracking Hardware",
                ButtonTooltip = "Select one or more face-tracking hardware devices used by this avatar.",
                ItemsSource = HardwareSupportData.Profiles.ToList(),
            };
            hardwareProfileField.RegisterValueChangedCallback(_ => Refresh());
            hardwareProfileField.OnLangChanged += () =>
            {
                hardwareProfileField.LabelText = T("field.tracking_hardware", "Tracking Hardware");
                hardwareProfileField.ButtonTooltip = T("tooltip.tracking_hardware", "Select one or more face-tracking hardware devices used by this avatar.");
            };
            root.Add(hardwareProfileField);

            // FeatureSettingView

            FeatureSettingView featureSettingView = new(BindCell_FeatureSettings_Feature, BindCell_FeatureSettings_HardwareSupport, BindCell_FeatureSettings_OutputFormat, BindCell_FeatureSettings_SyncMode)
            {
                itemsSource = Enumerable.Range(0, SpFeatureSettings.arraySize).ToList(),
            };
            featureSettingView.OnOutputFormatChanged += _ => Refresh();
            featureSettingView.OnSyncModeChanged += _ => Refresh();
            featureSettingView.OnLangChanged += () =>
            {
                featureSettingView.Column0Title = T("column.feature", "Feature");
                featureSettingView.Column1Title = T("column.hardware_support_short", "HW");
                featureSettingView.Column2Title = T("column.output_format", "Output Format");
                featureSettingView.Column3Title = T("column.sync_mode", "Sync Mode");
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
            featureFoldout.OnLangChanged += () => { featureFoldout.text = T("section.feature_settings", "Feature Settings"); };
            root.Add(featureFoldout);

            // BlendShapeSettingView
            BlendShapeSettingView blendShapeSettingView = new(BindCell_BlendshapeSettings_Expression, BindCell_BlendshapeSettings_HardwareSupport, BindCell_BlendshapeSettings_Blendshape, BindCell_BlendshapeSettings_maxValue)
            {
                itemsSource = Enumerable.Range(0, SpBlendshapeAssignments.arraySize).ToList(),
            };
            blendShapeSettingView.OnAssignmentChanged += _ => Refresh();
            blendShapeSettingView.OnLangChanged += () =>
            {
                blendShapeSettingView.Column0Title = T("column.unified_expression", "Unified Expression");
                blendShapeSettingView.Column1Title = T("column.hardware_support_short", "HW");
                blendShapeSettingView.Column2Title = T("column.blend_shape", "Blend Shape");
                blendShapeSettingView.Column3Title = T("column.max_value", "Max Value");
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
            blendshapeFold.OnLangChanged += () => { blendshapeFold.text = T("section.blend_shape_assignments", "Blend Shape Assignments"); };
            root.Add(blendshapeFold);

            // SyncParameterUsageLabel

            SyncParameterUsageLabel syncParameterUsageLabel = new();
            syncParameterUsageLabel.OnLangChanged += () =>
            {
                syncParameterUsageLabel.SummaryFormat = T("summary.sync_parameter_usage", "Sync Parameter Usage: {0} bits ({1}/{2} parameters, {3} without blend shape assignments)");
                syncParameterUsageLabel.Rebuild();
            };
            SyncParameterUsageLabel = syncParameterUsageLabel;
            root.Add(syncParameterUsageLabel);

            // LanguageSwitcherElement

            VisualElement languageSwitcher = new LanguageSwitcherElement();
            languageSwitcher.style.marginTop = 4;
            root.Add(languageSwitcher);
        }


        #region Reflesh

        private void Refresh()
        {
            serializedObject.Update();
            SyncParameterUsage usage = SyncParameterUsageCalculator.Calculate(target as USTLFaceTracking);
            SyncParameterUsageLabel.ParameterUsage = usage;
            BlendShapeSettingView.Rebuild();
            FeatureSettingView.Rebuild();
        }

        #endregion

        #region BindCalls Feature

        private void BindCell_FeatureSettings_Feature(Label label, int index)
        {
            FeatureSetting setting = new(SpFeatureSettings, index);

            label.text = OutputFormatDisplay.FormatFeatureName(setting.FeatureProperty, setting.FeatureDefinition);
            label.tooltip = label.text;
        }

        private void BindCell_FeatureSettings_HardwareSupport(Label label, int index)
        {
            FeatureSetting setting = new(SpFeatureSettings, index);
            TrackingHardwareSetting hwSetting = new(SpTrackingHardwareField);
            FaceTrackingFeatureDefinition featureDefinition = setting.FeatureDefinition;
            HardwareSupportStatus status = HardwareSupportStatus.Unknown;
            if (featureDefinition != null)
            {
                VRCFTParameterOutputFormat outputFormat = featureDefinition.OutputFormats[OutputFormatDisplay.ClampIndex(featureDefinition, setting.OutputFormatIndex)];
                HardwareSupportStatus tmp = HardwareSupportDisplay.GetOutputFormatStatus(hwSetting.TrackingHardware, outputFormat);
                if (status > tmp)
                {
                    status = tmp;
                }
            }

            label.style.color = FaceTrackingInspectorStyles.GetSupportStatusTextColor(status);
        }

        private void BindCell_FeatureSettings_OutputFormat(DropdownField dropdownField, int index)
        {
            FeatureSetting setting = new(SpFeatureSettings, index);
            FaceTrackingFeatureDefinition featureDefinition = setting.FeatureDefinition;
            List<string> choices = OutputFormatDisplay.GetChoices(featureDefinition);

            dropdownField.UnregisterValueChangedCallback(ChangeCallback_FeatureSettings_OnOutputFormatChanged);
            dropdownField.userData = index;
            dropdownField.choices = choices;
            dropdownField.formatListItemCallback = value => OutputFormatDisplay.FormatChoice(featureDefinition, value);
            dropdownField.formatSelectedValueCallback = value => OutputFormatDisplay.FormatChoice(featureDefinition, value);
            dropdownField.SetValueWithoutNotify(dropdownField.choices[setting.OutputFormatIndex]);
            dropdownField.RegisterValueChangedCallback(ChangeCallback_FeatureSettings_OnOutputFormatChanged);
        }

        private void BindCell_FeatureSettings_SyncMode(DropdownField dropdownField, int index)
        {
            FeatureSetting setting = new(SpFeatureSettings, index);
            ParameterSyncMode syncMode = setting.SyncMode;

            dropdownField.UnregisterValueChangedCallback(ChangeCallback_FeatureSettings_OnSyncModeChanged);
            dropdownField.userData = index;
            dropdownField.choices = ParameterSyncModeDisplay.GetChoices();
            dropdownField.formatListItemCallback = ParameterSyncModeDisplay.FormatChoice;
            dropdownField.formatSelectedValueCallback = ParameterSyncModeDisplay.FormatChoice;

            if (setting.Feature is FaceTrackingFeature.EyeDirection or FaceTrackingFeature.EyeLid && setting.OutputFormatIndex == 2)
            {
                if (syncMode != ParameterSyncMode.None)
                {
                    setting.SyncModeProperty.intValue = (int)ParameterSyncMode.None;
                    serializedObject.ApplyModifiedProperties();
                    syncMode = ParameterSyncMode.None;
                }

                dropdownField.SetEnabled(false);
            }
            else
            {
                dropdownField.SetEnabled(true);
            }

            dropdownField.SetValueWithoutNotify(dropdownField.choices[(int)syncMode]);
            dropdownField.RegisterValueChangedCallback(ChangeCallback_FeatureSettings_OnSyncModeChanged);
        }


        private void ChangeCallback_FeatureSettings_OnOutputFormatChanged(ChangeEvent<string> evt)
        {
            if (evt.currentTarget is not DropdownField { userData: int index, } dropdownField)
            {
                return;
            }

            serializedObject.Update();

            FeatureSetting setting = new(SpFeatureSettings, index);
            int currentFormatIndex = setting.OutputFormatIndex;
            int newFormatIndex = dropdownField.index;
            if (newFormatIndex < 0 || setting.FeatureDefinition.OutputFormats.Count <= newFormatIndex)
            {
                dropdownField.SetValueWithoutNotify(dropdownField.choices[currentFormatIndex]);
                return;
            }

            if (newFormatIndex == currentFormatIndex)
            {
                return;
            }

            setting.OutputFormatProperty.intValue = newFormatIndex;
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
            ParameterSyncMode newSyncMode = (ParameterSyncMode)dropdownField.index;
            if (!FaceTrackingEditorUtility.AllSyncModes.Contains(newSyncMode))
            {
                dropdownField.SetValueWithoutNotify(dropdownField.choices[(int)currentSyncMode]);
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
            foreach (HardwareSupportProfile profile in hwSetting.HardwareSupportProfiles)
            {
                if (status == HardwareSupportStatus.Full)
                {
                    break;
                }

                HardwareSupportStatus tmp = HardwareSupportData.GetExpressionStatus(profile, setting.Expression);
                if (status > tmp)
                {
                    status = tmp;
                }
            }

            label.style.color = FaceTrackingInspectorStyles.GetSupportStatusTextColor(status);
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

            TextElement textElement = field.Q<TextElement>(className: FaceTrackingInspectorStyles.PopupFieldTextUssClassName);

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
            private static readonly Dictionary<Mesh, IReadOnlyList<string>> BlendshapeCache = new();

            private static IReadOnlyList<string> GetBlendshape(Mesh mesh)
            {
                if (BlendshapeCache.TryGetValue(mesh, out IReadOnlyList<string> blendshape))
                {
                    return blendshape;
                }

                List<string> blendshapeList = new(mesh.blendShapeCount);
                for (int i = 0; i < mesh.blendShapeCount; i++)
                {
                    blendshapeList.Add(mesh.GetBlendShapeName(i));
                }

                BlendshapeCache.Add(mesh, blendshapeList);
                return blendshapeList;
            }

            [InitializeOnLoadMethod]
            private static void ResetCache()
            {
                BlendshapeCache.Clear();
            }

            public FaceMeshSetting(SerializedProperty serializedProperty)
            {
                FaceMeshProperty = serializedProperty;
            }

            public SerializedProperty FaceMeshProperty { get; }
            public SkinnedMeshRenderer FaceMeshRenderer => FaceMeshProperty.objectReferenceValue as SkinnedMeshRenderer;
            public Mesh FaceMesh => FaceMeshRenderer.sharedMesh;
            public IReadOnlyList<string> Blendshapes => GetBlendshape(FaceMesh);
        }

        private readonly struct TrackingHardwareSetting
        {
            public TrackingHardwareSetting(SerializedProperty serializedProperty)
            {
                TrackingHardwareProperty = serializedProperty;
            }

            public SerializedProperty TrackingHardwareProperty { get; }
            public int TrackingHardware => TrackingHardwareProperty.intValue;
            public List<HardwareSupportProfile> HardwareSupportProfiles => HardwareSupportData.GetProfiles(TrackingHardware);
        }

        private readonly struct FeatureSetting
        {
            public FeatureSetting(SerializedProperty arraySerializedProperty, int index)
            {
                FeatureProperty = arraySerializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative(nameof(FaceTrackingFeatureSetting.feature));
                OutputFormatProperty = arraySerializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative(nameof(FaceTrackingFeatureSetting.outputFormatIndex));
                SyncModeProperty = arraySerializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative(nameof(FaceTrackingFeatureSetting.syncMode));
            }

            public SerializedProperty FeatureProperty { get; }
            public SerializedProperty OutputFormatProperty { get; }
            public SerializedProperty SyncModeProperty { get; }

            public FaceTrackingFeature Feature => (FaceTrackingFeature)FeatureProperty.intValue;

            public FaceTrackingFeatureDefinition FeatureDefinition => FaceTrackingFeatureDefinition.All.GetValueOrDefault(Feature);
            public int OutputFormatIndex => OutputFormatProperty.intValue;
            public ParameterSyncMode SyncMode => (ParameterSyncMode)SyncModeProperty.intValue;
        }

        private readonly struct BlendshapeSetting
        {
            public BlendshapeSetting(SerializedProperty arraySerializedProperty, int index)
            {
                ExpressionProperty = arraySerializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative(nameof(BlendShapeAssignment.expression));
                BlendshapeProperty = arraySerializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative(nameof(BlendShapeAssignment.blendShapeName));
                MaxValueProperty = arraySerializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative(nameof(BlendShapeAssignment.maxValue));
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

using System.Collections.Generic;
using nadena.dev.modular_avatar.core.editor;
using nadena.dev.ndmf.localization;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace USTL.FaceTracking.Editor
{
    [CustomEditor(typeof(USTLFaceTracking))]
    internal sealed partial class USTLFaceTrackingEditor : FaceTrackingEditorBase
    {
        private const string HardwareProfileButtonName = "tracking-hardware";
        private const string OutputFormatDropdownName = "output-format";
        private const string FeatureSettingsFoldoutName = "feature-settings";
        private const string SyncModeDropdownName = "sync-mode";
        private const string BlendShapeNameFieldName = "blend-shape-name";
        private const string MaxValueFieldName = "max-value";
        private const string BlendShapeAssignmentFoldoutName = "blend-shape-assignments";
        private const string SyncParameterUsageLabelName = "sync-parameter-usage";
        private const string StatusIndicatorText = "●";

        [SerializeField] private bool _featureSettingsFoldoutOpen;
        [SerializeField] private bool _blendShapeAssignmentFoldoutOpen;

        private readonly BlendShapeNameChoices _blendShapeNameChoices = new();
        private readonly List<int> _featureSettingItems = new();
        private readonly List<int> _blendShapeAssignmentItems = new();

        private ObjectField _faceMeshRendererField;
        private Button _hardwareProfileButton;
        private VisualElement _hardwareProfileField;
        private Foldout _featureSettingsFoldout;
        private MultiColumnListView _featureSettingView;
        private Foldout _blendShapeAssignmentFoldout;
        private MultiColumnListView _blendShapeAssignmentView;
        private Label _syncParameterUsageLabel;

        private VisualElement HardwareProfileField => _hardwareProfileField ??= CreateHardwareProfileField();

        private ObjectField FaceMeshRendererField => _faceMeshRendererField ??= CreateFaceMeshRendererField();

        private Foldout FeatureSettingsFoldout => _featureSettingsFoldout ??= CreateFeatureSettingsFoldout();

        private MultiColumnListView FeatureSettingView => _featureSettingView ??= CreateFeatureSettingView();

        private Foldout BlendShapeAssignmentFoldout => _blendShapeAssignmentFoldout ??= CreateBlendShapeAssignmentFoldout();

        private MultiColumnListView BlendShapeAssignmentView => _blendShapeAssignmentView ??= CreateBlendShapeAssignmentView();

        private Label SyncParameterUsageLabel => _syncParameterUsageLabel ??= CreateSyncParameterUsageLabel();

        private void OnEnable()
        {
            FeatureSettingsInitializer.EnsureInitialized(serializedObject);
            BlendShapeAssignmentInitializer.EnsureInitialized(serializedObject);
            RefreshFeatureSettingItems();
            RefreshBlendShapeAssignmentItems();
        }

        protected override void BuildInspectorGUI(VisualElement root)
        {
            RefreshBlendShapeAssignmentItems();
            RefreshFeatureSettingItems();
            AddFaceMeshRendererField(root);
            AddHardwareProfileField(root);
            AddFeatureSettingView(root);
            AddBlendShapeAssignmentView(root);
            AddLanguageSwitcher(root);
            TrackHardwareProfileChanges(root);
            TrackBlendShapeMeshChanges(root);
            TrackFeatureSettingChanges(root);
            FaceTrackingLocalization.Localize(root);
            LanguagePrefs.RegisterLanguageChangeCallback(root, _ => ApplyLocalizedText());
            ApplyLocalizedText();
        }

        private void AddFeatureSettingView(VisualElement root)
        {
            root.Add(FeatureSettingsFoldout);
        }

        private void AddBlendShapeAssignmentView(VisualElement root)
        {
            RefreshBlendShapeNameChoices();
            root.Add(BlendShapeAssignmentFoldout);
            root.Add(SyncParameterUsageLabel);
            RefreshBlendShapeAssignmentViewForBlendShapeMesh(false);
            RefreshSyncParameterUsageLabel();
        }

        private Foldout CreateFeatureSettingsFoldout()
        {
            Foldout foldout = CreateListViewFoldout(FeatureSettingsFoldoutName, _featureSettingsFoldoutOpen);
            foldout.RegisterValueChangedCallback(evt => _featureSettingsFoldoutOpen = evt.newValue);
            foldout.Add(FeatureSettingView);
            return foldout;
        }

        private Foldout CreateBlendShapeAssignmentFoldout()
        {
            Foldout foldout = CreateListViewFoldout(BlendShapeAssignmentFoldoutName, _blendShapeAssignmentFoldoutOpen);
            foldout.RegisterValueChangedCallback(evt => _blendShapeAssignmentFoldoutOpen = evt.newValue);
            foldout.Add(BlendShapeAssignmentView);
            return foldout;
        }

        private static Foldout CreateListViewFoldout(string name, bool isOpen)
        {
            Foldout foldout = new()
            {
                name = name,
                value = isOpen,
            };
            FaceTrackingInspectorStyles.ApplyFieldSpacing(foldout);
            return foldout;
        }

        private static void AddLanguageSwitcher(VisualElement root)
        {
            VisualElement languageSwitcher = new LanguageSwitcherElement();
            languageSwitcher.style.marginTop = FaceTrackingInspectorStyles.InspectorFieldSpacing;
            root.Add(languageSwitcher);
        }

        private void TrackBlendShapeMeshChanges(VisualElement root)
        {
            root.TrackPropertyValue(serializedObject.FindProperty(nameof(USTLFaceTracking.faceMeshRenderer)), _ => RefreshBlendShapeAssignmentViewForBlendShapeMesh(true));
            root.schedule.Execute(RefreshBlendShapeAssignmentViewIfBlendShapeMeshChanged).Every(250);
        }

        private void TrackFeatureSettingChanges(VisualElement root)
        {
            root.TrackPropertyValue(serializedObject.FindProperty(nameof(USTLFaceTracking.featureSettings)), _ => RefreshSyncParameterUsageLabel());
            root.TrackPropertyValue(serializedObject.FindProperty(nameof(USTLFaceTracking.blendShapeAssignments)), _ => RefreshSyncParameterUsageLabel());
        }

        private void TrackHardwareProfileChanges(VisualElement root)
        {
            root.TrackPropertyValue(serializedObject.FindProperty(nameof(USTLFaceTracking.trackingHardwareProfiles)), _ =>
            {
                RefreshHardwareProfileField();
                _featureSettingView?.Rebuild();
                _blendShapeAssignmentView?.Rebuild();
            });
        }

        private void ApplyLocalizedText()
        {
            RefreshHardwareProfileField();
            FaceMeshRendererField.label = T("field.face_mesh_renderer", "Face Mesh Renderer");

            if (_featureSettingsFoldout != null)
            {
                _featureSettingsFoldout.text = T("section.feature_settings", "Feature Settings");
            }

            if (_blendShapeAssignmentFoldout != null)
            {
                _blendShapeAssignmentFoldout.text = T("section.blend_shape_assignments", "Blend Shape Assignments");
            }

            if (_featureSettingView != null)
            {
                _featureSettingView.columns[0].title = T("column.feature", "Feature");
                _featureSettingView.columns[1].title = T("column.hardware_support_short", "HW");
                _featureSettingView.columns[2].title = T("column.output_format", "Output Format");
                _featureSettingView.columns[3].title = T("column.sync_mode", "Sync Mode");
                _featureSettingView.Rebuild();
            }

            if (_blendShapeAssignmentView != null)
            {
                _blendShapeAssignmentView.columns[0].title = T("column.unified_expression", "Unified Expression");
                _blendShapeAssignmentView.columns[1].title = T("column.hardware_support_short", "HW");
                _blendShapeAssignmentView.columns[2].title = T("column.blend_shape", "Blend Shape");
                _blendShapeAssignmentView.columns[3].title = T("column.max_value", "Max Value");
                _blendShapeAssignmentView.Rebuild();
            }

            RefreshSyncParameterUsageLabel();
        }

        private static Label CreateSyncParameterUsageLabel()
        {
            Label label = new()
            {
                name = SyncParameterUsageLabelName,
            };
            FaceTrackingInspectorStyles.ApplySyncParameterUsageLabel(label);
            return label;
        }

        private void RefreshSyncParameterUsageLabel()
        {
            if (_syncParameterUsageLabel == null)
            {
                return;
            }

            serializedObject.Update();
            SyncParameterUsage usage = SyncParameterUsageCalculator.Calculate(target as USTLFaceTracking);
            _syncParameterUsageLabel.text = SyncParameterUsageDisplay.FormatSummary(usage);
            _syncParameterUsageLabel.tooltip = SyncParameterUsageDisplay.FormatTooltip(usage);
        }

        private static VisualElement CreateStatusIndicatorHeader()
        {
            Label label = new();
            FaceTrackingInspectorStyles.ApplyStatusIndicatorHeader(label);
            return label;
        }

        private static void BindStatusIndicatorHeader(VisualElement element, string text, string tooltip)
        {
            if (element is not Label label)
            {
                return;
            }

            label.text = text;
            label.tooltip = tooltip;
        }

        private static string T(string key, string fallback)
        {
            return FaceTrackingEditorText.Get(key, fallback);
        }
    }
}

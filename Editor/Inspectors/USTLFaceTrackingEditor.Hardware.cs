using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace USTL.FaceTracking.Editor
{
    internal sealed partial class USTLFaceTrackingEditor
    {
        private static ObjectField CreateFaceMeshRendererField()
        {
            return new ObjectField
            {
                bindingPath = nameof(USTLFaceTracking.faceMeshRenderer),
                label = T("field.face_mesh_renderer", "Face Mesh Renderer"),
                objectType = typeof(SkinnedMeshRenderer),
                allowSceneObjects = true,
            };
        }

        private VisualElement CreateHardwareProfileField()
        {
            VisualElement field = new();
            FaceTrackingInspectorStyles.ApplyHardwareProfileField(field);

            Label label = new()
            {
                text = T("field.tracking_hardware", "Tracking Hardware"),
            };
            FaceTrackingInspectorStyles.ApplyHardwareProfileLabel(label);

            _hardwareProfileButton = new Button(ShowHardwareProfileMenu)
            {
                name = HardwareProfileButtonName,
            };
            FaceTrackingInspectorStyles.ApplyHardwareProfileButton(_hardwareProfileButton);

            field.Add(label);
            field.Add(_hardwareProfileButton);
            return field;
        }

        private void AddHardwareProfileField(VisualElement root)
        {
            root.Add(HardwareProfileField);
            FaceTrackingInspectorStyles.ApplyFieldSpacing(HardwareProfileField);
            RefreshHardwareProfileField();
        }

        private void AddFaceMeshRendererField(VisualElement root)
        {
            root.Add(FaceMeshRendererField);
            FaceTrackingInspectorStyles.ApplyFieldSpacing(FaceMeshRendererField);
        }

        private FaceTrackingHardwareProfile GetHardwareProfiles()
        {
            SerializedProperty property = serializedObject.FindProperty(nameof(USTLFaceTracking.trackingHardwareProfiles));
            return property == null ? FaceTrackingHardwareProfile.None : (FaceTrackingHardwareProfile)property.intValue;
        }

        private void RefreshHardwareProfileField()
        {
            if (_hardwareProfileField == null || _hardwareProfileButton == null)
            {
                return;
            }

            FaceTrackingHardwareProfile trackingHardwareProfiles = GetHardwareProfiles();
            if (_hardwareProfileField[0] is Label label)
            {
                label.text = T("field.tracking_hardware", "Tracking Hardware");
            }

            _hardwareProfileButton.text = HardwareSupportDisplay.FormatSelectedProfiles(trackingHardwareProfiles);
            _hardwareProfileButton.tooltip = T("tooltip.tracking_hardware", "Select one or more face-tracking hardware devices used by this avatar.");
        }

        private void ShowHardwareProfileMenu()
        {
            FaceTrackingHardwareProfile trackingHardwareProfiles = GetHardwareProfiles();
            GenericMenu menu = new();
            menu.AddItem(new GUIContent(HardwareSupportDisplay.FormatProfile(FaceTrackingHardwareProfile.None)), trackingHardwareProfiles == FaceTrackingHardwareProfile.None, () => SetHardwareProfiles(FaceTrackingHardwareProfile.None));
            menu.AddSeparator(string.Empty);

            foreach (FaceTrackingHardwareProfile profile in HardwareSupportDisplay.Profiles)
            {
                menu.AddItem(new GUIContent(HardwareSupportDisplay.FormatProfile(profile)), HardwareSupportDisplay.HasProfile(trackingHardwareProfiles, profile), () => ToggleHardwareProfile(profile));
            }

            menu.ShowAsContext();
        }

        private void ToggleHardwareProfile(FaceTrackingHardwareProfile profile)
        {
            FaceTrackingHardwareProfile trackingHardwareProfiles = GetHardwareProfiles();
            FaceTrackingHardwareProfile newHardwareProfiles = HardwareSupportDisplay.HasProfile(trackingHardwareProfiles, profile) ? trackingHardwareProfiles & ~profile : trackingHardwareProfiles | profile;
            SetHardwareProfiles(newHardwareProfiles);
        }

        private void SetHardwareProfiles(FaceTrackingHardwareProfile trackingHardwareProfiles)
        {
            serializedObject.Update();
            SerializedProperty property = serializedObject.FindProperty(nameof(USTLFaceTracking.trackingHardwareProfiles));
            if (property == null || property.intValue == (int)trackingHardwareProfiles)
            {
                return;
            }

            property.intValue = (int)trackingHardwareProfiles;
            serializedObject.ApplyModifiedProperties();
            RefreshHardwareProfileField();
            _featureSettingView?.Rebuild();
            _blendShapeAssignmentView?.Rebuild();
        }
    }
}

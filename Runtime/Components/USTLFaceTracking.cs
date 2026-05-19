using UnityEngine;
using VRC.SDKBase;

namespace USTL.FaceTracking
{
    [AddComponentMenu("U-Stella/U-Stella FaceTracking")]
    public class USTLFaceTracking : MonoBehaviour, IEditorOnly
    {
        [SerializeField] internal int trackingHardwareProfiles;
        [SerializeField] internal SkinnedMeshRenderer faceMeshRenderer;
        [SerializeField] internal BlendShapeAssignment[] blendShapeAssignments;
        [SerializeField] internal FaceTrackingFeatureSetting[] featureSettings;
    }
}

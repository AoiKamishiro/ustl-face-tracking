using System;
using UnityEngine;
using VRC.SDKBase;

namespace USTL.FaceTracking
{
    [AddComponentMenu("U-Stella/U-Stella FaceTracking")]
    public class USTLFaceTracking : MonoBehaviour, IEditorOnly
    {
        [SerializeField] internal SupportedHardwares trackingHardwareProfiles;
        [SerializeField] internal SkinnedMeshRenderer faceMeshRenderer;
        [SerializeField] internal FeatureSetting[] featureSettings;
        [SerializeField] internal BlendshapeSetting[] blendshapeSettings;
    }

    [Serializable]
    internal sealed class FeatureSetting
    {
        [SerializeField] internal FaceTrackingFeature feature;
        [SerializeField] internal VRCFTParameterSetId outputFormatId;
        [SerializeField] internal ParameterSyncMode syncMode = ParameterSyncMode.LocalOnly;
    }

    [Serializable]
    internal sealed class BlendshapeSetting
    {
        [SerializeField] internal UnifiedExpression expression;
        [SerializeField] internal string blendShapeName;
        [SerializeField] internal float maxValue;
    }
}

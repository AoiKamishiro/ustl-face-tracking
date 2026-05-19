using System;
using UnityEngine;

namespace USTL.FaceTracking
{
    [Serializable]
    internal sealed class FaceTrackingFeatureSetting
    {
        [SerializeField] internal FaceTrackingFeature feature;
        [SerializeField] internal int outputFormatIndex;
        [SerializeField] internal ParameterSyncMode syncMode = ParameterSyncMode.LocalOnly;
    }
}

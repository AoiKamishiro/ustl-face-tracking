using UnityEngine;

namespace USTL.FaceTracking.Editor
{
    internal static class BlendshapeUtility
    {
        internal const float DefaultMaxValue = 100.0f;
        internal const float MinMaxValue = 0.0f;
        internal const float MaxMaxValue = 1000.0f;

        private const float ReferenceMaxValue = DefaultMaxValue;

        internal static float ClampMaxValue(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value) ? DefaultMaxValue : Mathf.Clamp(value, MinMaxValue, MaxMaxValue);
        }

        internal static float GetMaxValueDeltaScale(float maxValue)
        {
            float clampedMaxValue = ClampMaxValue(maxValue);
            return clampedMaxValue <= 0.0f ? 0.0f : ReferenceMaxValue / clampedMaxValue;
        }
    }
}

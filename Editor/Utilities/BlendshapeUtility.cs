using UnityEngine;

namespace USTL.FaceTracking.Editor
{
    internal static class BlendshapeUtility
    {
        internal const float DefaultMaxValue = BlendShapeSettingNormalizer.DefaultMaxValue;
        internal const float MinMaxValue = BlendShapeSettingNormalizer.MinMaxValue;
        internal const float MaxMaxValue = BlendShapeSettingNormalizer.MaxMaxValue;

        private const float ReferenceMaxValue = DefaultMaxValue;

        internal static float ClampMaxValue(float value)
        {
            return BlendShapeSettingNormalizer.NormalizeMaxValue(value);
        }

        internal static float GetMaxValueDeltaScale(float maxValue)
        {
            float clampedMaxValue = ClampMaxValue(maxValue);
            return clampedMaxValue <= 0.0f ? 0.0f : ReferenceMaxValue / clampedMaxValue;
        }
    }
}

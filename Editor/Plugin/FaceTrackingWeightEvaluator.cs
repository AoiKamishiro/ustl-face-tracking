using UnityEngine;

namespace USTL.FaceTracking.Editor
{
    internal static class FaceTrackingWeightEvaluator
    {
        internal static float Evaluate(WeightCurveType curveType, float value)
        {
            return curveType switch
            {
                WeightCurveType.Linear => Mathf.Clamp01(value),
                WeightCurveType.PositiveSigned => Mathf.Clamp01(value),
                WeightCurveType.NegativeSigned => Mathf.Clamp01(-value),
                WeightCurveType.EyelidClosed => value <= FaceTrackingGenerationConstants.EyelidNeutralValue ? Mathf.Clamp01(1f - value / FaceTrackingGenerationConstants.EyelidNeutralValue) : 0f,
                WeightCurveType.EyelidWide => value <= FaceTrackingGenerationConstants.EyelidNeutralValue ? 0f : Mathf.Clamp01((value - FaceTrackingGenerationConstants.EyelidNeutralValue) / (1f - FaceTrackingGenerationConstants.EyelidNeutralValue)),
                _ => 0f,
            };
        }
    }
}

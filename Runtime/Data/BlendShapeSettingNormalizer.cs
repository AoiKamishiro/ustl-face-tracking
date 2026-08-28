using System;
using System.Collections.Generic;
using UnityEngine;

namespace USTL.FaceTracking
{
    internal static class BlendShapeSettingNormalizer
    {
        internal const float DefaultMaxValue = 100.0f;
        internal const float MinMaxValue = 0.0f;
        internal const float MaxMaxValue = 1000.0f;

        private static readonly UnifiedExpression[] Expressions = GetExpressions();

        internal static BlendShapeSetting[] Normalize(BlendShapeSetting[] settings)
        {
            Dictionary<UnifiedExpression, BlendShapeSetting> current = new(settings?.Length ?? 0);
            if (settings != null)
            {
                foreach (BlendShapeSetting setting in settings)
                {
                    if (setting != null && Array.IndexOf(Expressions, setting.expression) >= 0)
                    {
                        current[setting.expression] = setting;
                    }
                }
            }

            BlendShapeSetting[] normalized = new BlendShapeSetting[Expressions.Length];
            bool hasChanges = settings == null || settings.Length != normalized.Length;

            for (int i = 0; i < Expressions.Length; i++)
            {
                UnifiedExpression expression = Expressions[i];
                current.TryGetValue(expression, out BlendShapeSetting existing);

                string blendShapeName = string.IsNullOrWhiteSpace(existing?.blendShapeName)
                    ? expression.ToString()
                    : existing.blendShapeName;
                float maxValue = NormalizeMaxValue(existing?.maxValue ?? DefaultMaxValue);

                if (existing != null &&
                    existing.expression == expression &&
                    existing.blendShapeName == blendShapeName &&
                    Mathf.Approximately(existing.maxValue, maxValue))
                {
                    normalized[i] = existing;
                }
                else
                {
                    normalized[i] = new BlendShapeSetting
                    {
                        expression = expression,
                        blendShapeName = blendShapeName,
                        maxValue = maxValue,
                    };
                    hasChanges = true;
                }

                if (!hasChanges && !ReferenceEquals(settings[i], normalized[i]))
                {
                    hasChanges = true;
                }
            }

            return hasChanges ? normalized : settings;
        }

        internal static float NormalizeMaxValue(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value)
                ? DefaultMaxValue
                : Mathf.Clamp(value, MinMaxValue, MaxMaxValue);
        }

        private static UnifiedExpression[] GetExpressions()
        {
            Array values = Enum.GetValues(typeof(UnifiedExpression));
            List<UnifiedExpression> expressions = new(values.Length);
            foreach (UnifiedExpression expression in values)
            {
                if (Convert.ToInt64(expression) >= 0)
                {
                    expressions.Add(expression);
                }
            }

            return expressions.ToArray();
        }
    }
}

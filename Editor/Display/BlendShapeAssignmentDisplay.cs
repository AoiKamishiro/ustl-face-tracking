using UnityEditor;
using UnityEngine;

namespace USTL.FaceTracking.Editor
{
    internal static class BlendShapeAssignmentDisplay
    {
        internal static string FormatTitle(SerializedProperty element)
        {
            SerializedProperty expressionProperty = element.FindPropertyRelative(nameof(BlendShapeAssignment.expression));
            return $"{(UnifiedExpression)expressionProperty.intValue}";
        }

        internal static float ClampValue(float value)
        {
            return Mathf.Clamp(value, 0f, 100f);
        }
    }
}

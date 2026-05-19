using System.Collections.Generic;
using UnityEditor;

namespace USTL.FaceTracking.Editor
{
    internal static class BlendShapeAssignmentInitializer
    {
        private const float DefaultMaxValue = 100.0f;

        internal static void EnsureInitialized(SerializedObject serializedObject)
        {
            serializedObject.Update();

            SerializedProperty blendShapeAssignments = serializedObject.FindProperty(nameof(USTLFaceTracking.blendShapeAssignments));
            IReadOnlyList<UnifiedExpression> expressions = FaceTrackingEditorUtility.AllExpressions;
            int oldArraySize = blendShapeAssignments.arraySize;
            bool hasChanges = oldArraySize != expressions.Count;

            blendShapeAssignments.arraySize = expressions.Count;

            for (int i = 0; i < expressions.Count; i++)
            {
                UnifiedExpression expression = expressions[i];
                SerializedProperty element = blendShapeAssignments.GetArrayElementAtIndex(i);
                SerializedProperty expressionProperty = element.FindPropertyRelative(nameof(BlendShapeAssignment.expression));
                bool expressionChanged = expressionProperty.intValue != (int)expression;
                bool shouldInitializeDefaults = i >= oldArraySize || expressionChanged;

                if (expressionChanged)
                {
                    expressionProperty.intValue = (int)expression;
                    hasChanges = true;
                }

                if (shouldInitializeDefaults)
                {
                    element.FindPropertyRelative(nameof(BlendShapeAssignment.blendShapeName)).stringValue = expression.ToString();
                    element.FindPropertyRelative(nameof(BlendShapeAssignment.maxValue)).floatValue = DefaultMaxValue;
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}

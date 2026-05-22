using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace USTL.FaceTracking.Editor
{
    internal static class BlendshapeSettingInitializer
    {
        internal static void EnsureInitialized(SerializedObject serializedObject)
        {
            serializedObject.Update();

            IReadOnlyList<UnifiedExpression> expressions = FaceTrackingEditorUtility.AllExpressions;

            SerializedProperty settings = serializedObject.FindProperty(nameof(USTLFaceTracking.blendshapeSettings));
            Dictionary<UnifiedExpression, BlendshapeSetting> current = new(settings.arraySize);
            for (int i = 0; i < settings.arraySize; i++)
            {
                SerializedProperty element = settings.GetArrayElementAtIndex(i);
                UnifiedExpression expression = (UnifiedExpression)element.FindPropertyRelative(nameof(BlendshapeSetting.expression)).intValue;
                if (!FaceTrackingEditorUtility.AllExpressions.Contains(expression))
                {
                    continue;
                }

                string blendshape = element.FindPropertyRelative(nameof(BlendshapeSetting.blendShapeName)).stringValue;
                float maxValue = element.FindPropertyRelative(nameof(BlendshapeSetting.maxValue)).floatValue;
                BlendshapeSetting setting = new()
                {
                    expression = expression,
                    blendShapeName = ValidateBlendshape(expression, blendshape),
                    maxValue = ValidateMaxValue(maxValue),
                };
                current[expression] = setting;
            }

            bool hasChanges = settings.arraySize != expressions.Count;
            settings.arraySize = expressions.Count;

            for (int i = 0; i < expressions.Count; i++)
            {
                SerializedProperty elementProperty = settings.GetArrayElementAtIndex(i);
                SerializedProperty expressionProperty = elementProperty.FindPropertyRelative(nameof(BlendshapeSetting.expression));
                SerializedProperty blendShapeProperty = elementProperty.FindPropertyRelative(nameof(BlendshapeSetting.blendShapeName));
                SerializedProperty maxValueProperty = elementProperty.FindPropertyRelative(nameof(BlendshapeSetting.maxValue));

                if ((UnifiedExpression)expressionProperty.intValue != expressions[i])
                {
                    expressionProperty.intValue = (int)expressions[i];

                    if (!current.ContainsKey(expressions[i]))
                    {
                        blendShapeProperty.stringValue = expressions[i].ToString();
                        maxValueProperty.floatValue = 100.0f;
                    }
                    else
                    {
                        blendShapeProperty.stringValue = current[expressions[i]].blendShapeName;
                        maxValueProperty.floatValue = current[expressions[i]].maxValue;
                    }

                    hasChanges = true;
                }
                else
                {
                    if (!current.ContainsKey(expressions[i]))
                    {
                        blendShapeProperty.stringValue = expressions[i].ToString();
                        maxValueProperty.floatValue = 100.0f;
                        hasChanges = true;
                    }
                    else
                    {
                        if (blendShapeProperty.stringValue != current[expressions[i]].blendShapeName)
                        {
                            blendShapeProperty.stringValue = current[expressions[i]].blendShapeName;
                            hasChanges = true;
                        }

                        if (!Mathf.Approximately(maxValueProperty.floatValue, current[expressions[i]].maxValue))
                        {
                            maxValueProperty.floatValue = current[expressions[i]].maxValue;
                            hasChanges = true;
                        }
                    }
                }
            }

            if (hasChanges)
            {
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static string ValidateBlendshape(UnifiedExpression expression, string blendshapeName)
        {
            return string.IsNullOrWhiteSpace(blendshapeName) ? expression.ToString() : blendshapeName;
        }

        private static float ValidateMaxValue(float maxValue)
        {
            return maxValue is < 0.0f or > 100.0f ? 100.0f : maxValue;
        }
    }
}

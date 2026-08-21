using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace USTL.FaceTracking.Editor
{
    internal static class BlendShapeSettingInitializer
    {
        internal static void EnsureInitialized(SerializedObject serializedObject)
        {
            if (serializedObject.targetObject is not USTLFaceTracking)
            {
                throw new ArgumentException($"Expected targetObject of type {typeof(USTLFaceTracking).FullName}, but got {serializedObject.targetObject.GetType().FullName}.");
            }

            serializedObject.Update();

            IReadOnlyList<UnifiedExpression> expressions = EnumUtility.GetAllElements<UnifiedExpression>();

            SerializedProperty settings = serializedObject.FindProperty(nameof(USTLFaceTracking.blendShapeSettings));
            Dictionary<UnifiedExpression, BlendShapeSetting> current = new(settings.arraySize);
            for (int i = 0; i < settings.arraySize; i++)
            {
                SerializedProperty element = settings.GetArrayElementAtIndex(i);
                UnifiedExpression expression = (UnifiedExpression)element.FindPropertyRelative(nameof(BlendShapeSetting.expression)).intValue;
                if (!expressions.Contains(expression))
                {
                    continue;
                }

                string blendShape = element.FindPropertyRelative(nameof(BlendShapeSetting.blendShapeName)).stringValue;
                float maxValue = element.FindPropertyRelative(nameof(BlendShapeSetting.maxValue)).floatValue;
                BlendShapeSetting setting = new()
                {
                    expression = expression,
                    blendShapeName = ValidateBlendShape(expression, blendShape),
                    maxValue = ValidateMaxValue(maxValue),
                };
                current[expression] = setting;
            }

            bool hasChanges = settings.arraySize != expressions.Count;
            settings.arraySize = expressions.Count;

            for (int i = 0; i < expressions.Count; i++)
            {
                SerializedProperty elementProperty = settings.GetArrayElementAtIndex(i);
                SerializedProperty expressionProperty = elementProperty.FindPropertyRelative(nameof(BlendShapeSetting.expression));
                SerializedProperty blendShapeProperty = elementProperty.FindPropertyRelative(nameof(BlendShapeSetting.blendShapeName));
                SerializedProperty maxValueProperty = elementProperty.FindPropertyRelative(nameof(BlendShapeSetting.maxValue));

                if ((UnifiedExpression)expressionProperty.intValue != expressions[i])
                {
                    expressionProperty.intValue = (int)expressions[i];

                    if (!current.ContainsKey(expressions[i]))
                    {
                        blendShapeProperty.stringValue = expressions[i].ToString();
                        maxValueProperty.floatValue = BlendshapeUtility.DefaultMaxValue;
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
                        maxValueProperty.floatValue = BlendshapeUtility.DefaultMaxValue;
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

        private static string ValidateBlendShape(UnifiedExpression expression, string blendShapeName)
        {
            return string.IsNullOrWhiteSpace(blendShapeName) ? expression.ToString() : blendShapeName;
        }

        private static float ValidateMaxValue(float maxValue)
        {
            return BlendshapeUtility.ClampMaxValue(maxValue);
        }
    }
}

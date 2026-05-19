using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace USTL.FaceTracking.Editor
{
    internal sealed partial class USTLFaceTrackingEditor
    {
        private MultiColumnListView CreateBlendShapeAssignmentView()
        {
            MultiColumnListView view = new()
            {
                bindingPath = nameof(USTLFaceTracking.blendShapeAssignments),
                itemsSource = _blendShapeAssignmentItems,
                fixedItemHeight = FaceTrackingInspectorStyles.BlendShapeAssignmentItemHeight,
                showAddRemoveFooter = false,
                showBoundCollectionSize = false,
                showBorder = true,
                reorderable = false,
                selectionType = SelectionType.None,
                showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
                virtualizationMethod = CollectionVirtualizationMethod.FixedHeight,
            };

            view.columns.Add(CreateExpressionColumn());
            view.columns.Add(CreateBlendShapeHardwareSupportColumn());
            view.columns.Add(CreateBlendShapeColumn());
            view.columns.Add(CreateMaxValueColumn());
            view.style.width = Length.Percent(100);
            view.style.flexGrow = 0;
            return view;
        }

        private Column CreateExpressionColumn()
        {
            return new Column
            {
                title = T("column.unified_expression", "Unified Expression"),
                width = FaceTrackingInspectorStyles.BlendShapeAssignmentExpressionColumnWidth,
                minWidth = FaceTrackingInspectorStyles.BlendShapeAssignmentExpressionColumnWidth,
                maxWidth = FaceTrackingInspectorStyles.BlendShapeAssignmentExpressionColumnWidth,
                stretchable = false,
                resizable = false,
                makeCell = CreateExpressionCell,
                bindCell = BindExpressionCell,
            };
        }

        private Column CreateBlendShapeHardwareSupportColumn()
        {
            return new Column
            {
                title = T("column.hardware_support", "Hardware Support"),
                width = FaceTrackingInspectorStyles.BlendShapeAssignmentSupportColumnWidth,
                minWidth = FaceTrackingInspectorStyles.BlendShapeAssignmentSupportColumnWidth,
                maxWidth = FaceTrackingInspectorStyles.BlendShapeAssignmentSupportColumnWidth,
                stretchable = false,
                resizable = false,
                makeHeader = CreateStatusIndicatorHeader,
                bindHeader = header => BindStatusIndicatorHeader(header, T("column.hardware_support_short", "HW"), T("column.hardware_support", "Hardware Support")),
                makeCell = CreateAssignmentStatusCell,
                bindCell = BindBlendShapeHardwareSupportCell,
            };
        }

        private Column CreateBlendShapeColumn()
        {
            return new Column
            {
                title = T("column.blend_shape", "Blend Shape"),
                stretchable = true,
                makeCell = CreateBlendShapeCell,
                bindCell = BindBlendShapeCell,
            };
        }

        private Column CreateMaxValueColumn()
        {
            return new Column
            {
                title = T("column.max_value", "Max Value"),
                width = FaceTrackingInspectorStyles.BlendShapeAssignmentValueColumnWidth,
                minWidth = FaceTrackingInspectorStyles.BlendShapeAssignmentValueColumnWidth,
                maxWidth = FaceTrackingInspectorStyles.BlendShapeAssignmentValueColumnWidth,
                stretchable = false,
                resizable = false,
                makeCell = CreateMaxValueCell,
                bindCell = BindMaxValueCell,
            };
        }

        private static VisualElement CreateExpressionCell()
        {
            Label label = new();
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            FaceTrackingInspectorStyles.ApplyBlendShapeAssignmentCellBase(label);
            return label;
        }

        private static VisualElement CreateAssignmentStatusCell()
        {
            Label label = new();
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            FaceTrackingInspectorStyles.ApplyBlendShapeAssignmentCellBase(label);
            return label;
        }

        private VisualElement CreateBlendShapeCell()
        {
            VisualElement innerRoot = new();
            FaceTrackingInspectorStyles.ApplyBlendShapeAssignmentCellBase(innerRoot);
            innerRoot.Add(CreateBlendShapeNameField());
            return innerRoot;
        }

        private static VisualElement CreateMaxValueCell()
        {
            VisualElement innerRoot = new();
            FaceTrackingInspectorStyles.ApplyBlendShapeAssignmentCellBase(innerRoot);
            innerRoot.Add(CreateMaxValueField());
            return innerRoot;
        }

        private DropdownField CreateBlendShapeNameField()
        {
            DropdownField field = new()
            {
                name = BlendShapeNameFieldName,
            };
            FaceTrackingInspectorStyles.ApplyEmbeddedField(field);
            field.RegisterValueChangedCallback(OnBlendShapeNameChanged);
            return field;
        }

        private static FloatField CreateMaxValueField()
        {
            FloatField field = new()
            {
                name = MaxValueFieldName,
            };
            FaceTrackingInspectorStyles.ApplyEmbeddedField(field);
            return field;
        }

        private void BindExpressionCell(VisualElement element, int index)
        {
            if (element is not Label label)
            {
                return;
            }

            SerializedProperty blendShapeAssignment = GetBlendShapeAssignmentElementAt(index);
            label.text = BlendShapeAssignmentDisplay.FormatTitle(blendShapeAssignment);
            label.tooltip = label.text;
            FaceTrackingInspectorStyles.ApplyBlendShapeAssignmentCell(label, index);
        }

        private void BindBlendShapeHardwareSupportCell(VisualElement element, int index)
        {
            if (element is not Label label)
            {
                return;
            }

            SerializedProperty blendShapeAssignment = GetBlendShapeAssignmentElementAt(index);
            UnifiedExpression expression = GetAssignmentExpression(blendShapeAssignment);
            FaceTrackingHardwareProfile trackingHardwareProfiles = GetHardwareProfiles();
            ExpressionAvailabilityResult result = HardwareSupportDisplay.GetExpressionAvailability(trackingHardwareProfiles, expression);

            label.text = StatusIndicatorText;
            label.tooltip = HardwareSupportDisplay.FormatExpressionAvailabilityTooltip(trackingHardwareProfiles, expression, result);
            FaceTrackingInspectorStyles.ApplyBlendShapeAssignmentCell(label, index);
            FaceTrackingInspectorStyles.ApplyHardwareKeyAvailabilityStatusText(label, result.Status);
        }

        private void BindBlendShapeCell(VisualElement element, int index)
        {
            SerializedProperty blendShapeAssignment = GetBlendShapeAssignmentElementAt(index);

            BindBlendShapeNameField(element, blendShapeAssignment, index);
            FaceTrackingInspectorStyles.ApplyBlendShapeAssignmentCell(element, index);
        }

        private void BindBlendShapeNameField(VisualElement element, SerializedProperty blendShapeAssignment, int index)
        {
            DropdownField dropdownField = element.Q<DropdownField>(BlendShapeNameFieldName);
            if (dropdownField == null)
            {
                return;
            }

            SerializedProperty blendShapeNameProperty = blendShapeAssignment.FindPropertyRelative(nameof(BlendShapeAssignment.blendShapeName));
            UnifiedExpression expression = GetAssignmentExpression(blendShapeAssignment);
            OutputFormatExpressionUsageResult usage = GetOutputFormatExpressionUsage(expression);
            bool isUsed = usage.Status == OutputFormatUsageStatus.Emitted;
            List<string> choices = _blendShapeNameChoices.GetChoicesForValue(blendShapeNameProperty.stringValue);
            dropdownField.Unbind();
            dropdownField.userData = index;
            dropdownField.choices = choices;
            dropdownField.SetEnabled(isUsed && choices.Count > 1);
            dropdownField.tooltip = FormatOutputFormatUsageTooltip(expression, usage);
            dropdownField.BindProperty(blendShapeNameProperty);
            ApplyBlendShapeNameFieldValidationStyle(dropdownField, blendShapeNameProperty.stringValue);
        }

        private void BindMaxValueCell(VisualElement element, int index)
        {
            SerializedProperty blendShapeAssignment = GetBlendShapeAssignmentElementAt(index);
            BindMaxValueField(element, blendShapeAssignment, index);
            FaceTrackingInspectorStyles.ApplyBlendShapeAssignmentCell(element, index);
        }

        private void BindMaxValueField(VisualElement element, SerializedProperty blendShapeAssignment, int index)
        {
            FloatField field = element.Q<FloatField>(MaxValueFieldName);
            if (field == null)
            {
                return;
            }

            SerializedProperty valueProperty = blendShapeAssignment.FindPropertyRelative(nameof(BlendShapeAssignment.maxValue));
            UnifiedExpression expression = GetAssignmentExpression(blendShapeAssignment);
            OutputFormatExpressionUsageResult usage = GetOutputFormatExpressionUsage(expression);
            float clampedValue = BlendShapeAssignmentDisplay.ClampValue(valueProperty.floatValue);
            if (!Mathf.Approximately(valueProperty.floatValue, clampedValue))
            {
                valueProperty.floatValue = clampedValue;
                serializedObject.ApplyModifiedProperties();
            }

            field.UnregisterValueChangedCallback(OnMaxValueChanged);
            field.userData = index;
            field.SetValueWithoutNotify(clampedValue);
            field.SetEnabled(usage.Status == OutputFormatUsageStatus.Emitted);
            field.tooltip = FormatOutputFormatUsageTooltip(expression, usage);
            field.RegisterValueChangedCallback(OnMaxValueChanged);
        }

        private SerializedProperty GetBlendShapeAssignmentElementAt(int index)
        {
            return serializedObject.FindProperty(nameof(USTLFaceTracking.blendShapeAssignments)).GetArrayElementAtIndex(index);
        }

        private static UnifiedExpression GetAssignmentExpression(SerializedProperty blendShapeAssignment)
        {
            SerializedProperty expressionProperty = blendShapeAssignment.FindPropertyRelative(nameof(BlendShapeAssignment.expression));
            return (UnifiedExpression)expressionProperty.intValue;
        }

        private void RefreshBlendShapeAssignmentItems()
        {
            SerializedProperty blendShapeAssignments = serializedObject.FindProperty(nameof(USTLFaceTracking.blendShapeAssignments));
            _blendShapeAssignmentItems.Clear();
            for (int i = 0; i < blendShapeAssignments.arraySize; i++)
            {
                _blendShapeAssignmentItems.Add(i);
            }

            if (_blendShapeAssignmentView != null)
            {
                _blendShapeAssignmentView.itemsSource = _blendShapeAssignmentItems;
                _blendShapeAssignmentView.Rebuild();
            }
        }

        private void RefreshBlendShapeAssignmentViewForBlendShapeMesh(bool assignExpectedNames)
        {
            bool meshChanged = RefreshBlendShapeNameChoices();
            if (assignExpectedNames && meshChanged)
            {
                AssignMissingExpectedBlendShapeNames();
            }

            _blendShapeAssignmentView?.Rebuild();
            RefreshSyncParameterUsageLabel();
        }

        private void RefreshBlendShapeAssignmentViewIfBlendShapeMeshChanged()
        {
            if (!RefreshBlendShapeNameChoices())
            {
                return;
            }

            _blendShapeAssignmentView?.Rebuild();
            RefreshSyncParameterUsageLabel();
        }

        private bool RefreshBlendShapeNameChoices()
        {
            return _blendShapeNameChoices.Refresh(serializedObject);
        }

        private void OnBlendShapeNameChanged(ChangeEvent<string> evt)
        {
            if (evt.currentTarget is DropdownField dropdownField)
            {
                ApplyBlendShapeNameFieldValidationStyle(dropdownField, evt.newValue);
                UpdateBlendShapeName(dropdownField, evt.newValue);
                RefreshSyncParameterUsageLabel();
            }
        }

        private void UpdateBlendShapeName(DropdownField dropdownField, string blendShapeName)
        {
            if (dropdownField.userData is not int index)
            {
                return;
            }

            serializedObject.Update();
            SerializedProperty blendShapeAssignments = serializedObject.FindProperty(nameof(USTLFaceTracking.blendShapeAssignments));
            if (index < 0 || index >= blendShapeAssignments.arraySize)
            {
                return;
            }

            SerializedProperty blendShapeAssignment = blendShapeAssignments.GetArrayElementAtIndex(index);
            SerializedProperty blendShapeNameProperty = blendShapeAssignment.FindPropertyRelative(nameof(BlendShapeAssignment.blendShapeName));
            if (blendShapeNameProperty.stringValue == blendShapeName)
            {
                return;
            }

            blendShapeNameProperty.stringValue = blendShapeName;
            serializedObject.ApplyModifiedProperties();
        }

        private void OnMaxValueChanged(ChangeEvent<float> evt)
        {
            if (evt.currentTarget is not FloatField field || field.userData is not int index)
            {
                return;
            }

            float clampedValue = BlendShapeAssignmentDisplay.ClampValue(evt.newValue);
            if (!Mathf.Approximately(field.value, clampedValue))
            {
                field.SetValueWithoutNotify(clampedValue);
            }

            serializedObject.Update();

            SerializedProperty blendShapeAssignment = GetBlendShapeAssignmentElementAt(index);
            SerializedProperty valueProperty = blendShapeAssignment.FindPropertyRelative(nameof(BlendShapeAssignment.maxValue));
            if (Mathf.Approximately(valueProperty.floatValue, clampedValue))
            {
                return;
            }

            valueProperty.floatValue = clampedValue;
            serializedObject.ApplyModifiedProperties();
        }

        private void ApplyBlendShapeNameFieldValidationStyle(DropdownField dropdownField, string blendShapeName)
        {
            StyleColor textColor = FaceTrackingInspectorStyles.GetBlendShapeNameTextColor(_blendShapeNameChoices.IsInvalid(blendShapeName));

            dropdownField.style.color = textColor;

            TextElement textElement = dropdownField.Q<TextElement>(className: FaceTrackingInspectorStyles.PopupFieldTextUssClassName);
            if (textElement != null)
            {
                textElement.style.color = textColor;
            }
        }

        private void AssignMissingExpectedBlendShapeNames()
        {
            if (!_blendShapeNameChoices.HasBlendShapeNames)
            {
                return;
            }

            serializedObject.Update();
            SerializedProperty blendShapeAssignments = serializedObject.FindProperty(nameof(USTLFaceTracking.blendShapeAssignments));
            bool hasChanges = false;

            for (int i = 0; i < blendShapeAssignments.arraySize; i++)
            {
                SerializedProperty element = blendShapeAssignments.GetArrayElementAtIndex(i);
                SerializedProperty expressionProperty = element.FindPropertyRelative(nameof(BlendShapeAssignment.expression));
                SerializedProperty blendShapeNameProperty = element.FindPropertyRelative(nameof(BlendShapeAssignment.blendShapeName));
                string expectedName = ((UnifiedExpression)expressionProperty.intValue).ToString();
                if (!string.IsNullOrEmpty(blendShapeNameProperty.stringValue) || !_blendShapeNameChoices.Contains(expectedName))
                {
                    continue;
                }

                blendShapeNameProperty.stringValue = expectedName;
                hasChanges = true;
            }

            if (hasChanges)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}

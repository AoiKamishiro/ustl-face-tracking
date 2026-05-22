using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace USTL.FaceTracking.Editor
{
    internal sealed class BlendShapeSettingView : MultiColumnListView
    {
        private const int ItemHeight = 32;
        private const int ExpressionColumnWidth = 148;
        private const int SupportColumnWidth = 40;
        private const int ValueColumnWidth = 112;
        private const int CellHorizontalPadding = 6;
        private const int CellVerticalPadding = 4;
        private const string ExpressionLabelName = "expression";
        private const string HardwareUsageLabelName = "hardware-usage";
        private const string BlendShapeNameFieldName = "blend-shape-name";
        private const string MaxValueFieldName = "max-value";

        internal BlendShapeSettingView(Action<Label, int> bindExpressionCell, Action<Label, int> bindHardwareSupportCell, Action<DropdownField, int> bindBlendShapeCell, Action<RangeFloatField, int> bindMaxValueCell)
        {
            fixedItemHeight = ItemHeight;
            showAddRemoveFooter = false;
            showBoundCollectionSize = false;
            showBorder = true;
            reorderable = false;
            selectionType = SelectionType.None;
            showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly;
            virtualizationMethod = CollectionVirtualizationMethod.FixedHeight;

            columns.Add(CreateExpressionColumn(bindExpressionCell));
            columns.Add(CreateHardwareSupportColumn(bindHardwareSupportCell));
            columns.Add(CreateBlendShapeColumn(bindBlendShapeCell));
            columns.Add(CreateMaxValueColumn(bindMaxValueCell));
            style.width = Length.Percent(100);
            style.flexGrow = 0;
        }


        internal string Column0Title
        {
            get => columns[0].title;
            set => columns[0].title = value;
        }

        internal string Column1Title
        {
            get => columns[1].title;
            set => columns[1].title = value;
        }

        internal string Column2Title
        {
            get => columns[2].title;
            set => columns[2].title = value;
        }

        internal string Column3Title
        {
            get => columns[3].title;
            set => columns[3].title = value;
        }

        internal event Action<int> OnAssignmentChanged;
        internal event Action<int> OnMaxValueChanged;


        private static Column CreateExpressionColumn(Action<Label, int> bindCell)
        {
            return new Column
            {
                title = "Unified Expression",
                width = ExpressionColumnWidth,
                minWidth = ExpressionColumnWidth,
                maxWidth = ExpressionColumnWidth,
                stretchable = false,
                resizable = false,
                makeCell = CreateExpressionCell,
                bindCell = (elem, index) =>
                {
                    Label label = elem.Q<Label>(ExpressionLabelName);
                    bindCell(label, index);
                },
            };
        }

        private static Column CreateHardwareSupportColumn(Action<Label, int> bindCell)
        {
            return new Column
            {
                title = "Hardware Support",
                width = SupportColumnWidth,
                minWidth = SupportColumnWidth,
                maxWidth = SupportColumnWidth,
                stretchable = false,
                resizable = false,
                makeCell = CreateAssignmentStatusCell,
                bindCell = (elem, index) =>
                {
                    Label label = elem.Q<Label>(HardwareUsageLabelName);
                    bindCell(label, index);
                },
            };
        }

        private Column CreateBlendShapeColumn(Action<DropdownField, int> bindCell)
        {
            return new Column
            {
                title = "Blend Shape",
                stretchable = true,
                makeCell = CreateBlendShapeCell,
                bindCell = (elem, index) =>
                {
                    DropdownField field = elem.Q<DropdownField>(BlendShapeNameFieldName);
                    field.UnregisterValueChangedCallback(HandleAssignmentChanged);
                    bindCell(field, index);
                    field.userData = index;
                    field.RegisterValueChangedCallback(HandleAssignmentChanged);
                },
            };
        }

        private Column CreateMaxValueColumn(Action<RangeFloatField, int> bindCell)
        {
            return new Column
            {
                title = "Max Value",
                width = ValueColumnWidth,
                minWidth = ValueColumnWidth,
                maxWidth = ValueColumnWidth,
                stretchable = false,
                resizable = false,
                makeCell = CreateMaxValueCell,
                bindCell = (elem, index) =>
                {
                    RangeFloatField field = elem.Q<RangeFloatField>(MaxValueFieldName);
                    field.UnregisterValueChangedCallback(HandleMaxValueChanged);
                    bindCell(field, index);
                    field.userData = index;
                    field.RegisterValueChangedCallback(HandleMaxValueChanged);
                },
            };
        }

        private static VisualElement CreateExpressionCell()
        {
            VisualElement innerRoot = new()
            {
                style =
                {
                    justifyContent = Justify.Center,
                },
            };
            ApplyColumnCellStyle(innerRoot);

            Label label = new()
            {
                name = ExpressionLabelName,
                text = "Expression",
                style =
                {
                    unityTextAlign = TextAnchor.MiddleLeft,
                },
            };
            innerRoot.Add(label);
            return innerRoot;
        }

        private static VisualElement CreateAssignmentStatusCell()
        {
            VisualElement innerRoot = new()
            {
                style =
                {
                    justifyContent = Justify.Center,
                },
            };
            ApplyColumnCellStyle(innerRoot);

            Label label = new()
            {
                name = HardwareUsageLabelName,
                text = "●",
                style =
                {
                    unityTextAlign = TextAnchor.MiddleCenter,
                    unityFontStyleAndWeight = FontStyle.Bold,
                },
            };
            innerRoot.Add(label);
            return innerRoot;
        }

        private static VisualElement CreateBlendShapeCell()
        {
            VisualElement innerRoot = new();
            ApplyColumnCellStyle(innerRoot);

            DropdownField field = new()
            {
                name = BlendShapeNameFieldName,
                style =
                {
                    flexGrow = 1,
                },
            };

            innerRoot.Add(field);
            return innerRoot;
        }

        private static VisualElement CreateMaxValueCell()
        {
            VisualElement innerRoot = new();
            ApplyColumnCellStyle(innerRoot);

            RangeFloatField field = new()
            {
                name = MaxValueFieldName,
                minValue = 0.0f,
                maxValue = 100.0f,
                style =
                {
                    flexGrow = 1,
                },
            };

            innerRoot.Add(field);
            return innerRoot;
        }

        private void HandleAssignmentChanged(ChangeEvent<string> evt)
        {
            if (evt.currentTarget is not DropdownField { userData: int index, })
            {
                return;
            }

            schedule.Execute(() => OnAssignmentChanged?.Invoke(index));
        }

        private void HandleMaxValueChanged(ChangeEvent<float> evt)
        {
            if (evt.currentTarget is not RangeFloatField { userData: int index, })
            {
                return;
            }

            schedule.Execute(() => OnMaxValueChanged?.Invoke(index));
        }

        private static void ApplyColumnCellStyle(VisualElement element)
        {
            element.style.paddingTop = CellVerticalPadding;
            element.style.paddingRight = CellHorizontalPadding;
            element.style.paddingBottom = CellVerticalPadding;
            element.style.paddingLeft = CellHorizontalPadding;
            element.style.flexGrow = 1;
        }
    }
}

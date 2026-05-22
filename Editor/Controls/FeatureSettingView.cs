using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace USTL.FaceTracking.Editor
{
    internal sealed class FeatureSettingView : MultiColumnListView, ILocalization
    {
        private const int ItemHeight = 28;
        private const int FeatureColumnWidth = 148;
        private const int SupportColumnWidth = 40;
        private const int SyncModeColumnWidth = 112;
        private const int CellHorizontalPadding = 6;
        private const int CellVerticalPadding = 2;
        private const string FeatureLabelName = "feature";
        private const string HardwareUsageLabelName = "hardware-usage";
        private const string OutputFormatDropdownName = "output-format";
        private const string SyncModeDropdownName = "sync-mode";

        internal FeatureSettingView(Action<LocalizationLabel, int> bindFeatureCell, Action<Label, int> bindHardwareSupportCell, Action<LocalizationDropdownField, int> bindOutputFormatCell, Action<EnumField, int> bindSyncModeCell)
        {
            fixedItemHeight = ItemHeight;
            showAddRemoveFooter = false;
            showBoundCollectionSize = false;
            showBorder = true;
            reorderable = false;
            selectionType = SelectionType.None;
            showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly;
            virtualizationMethod = CollectionVirtualizationMethod.FixedHeight;

            columns.Add(CreateFeatureColumn(bindFeatureCell));
            columns.Add(CreateHardwareSupportColumn(bindHardwareSupportCell));
            columns.Add(CreateOutputFormatColumn(bindOutputFormatCell));
            columns.Add(CreateSyncModeColumn(bindSyncModeCell));
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

        public Action OnLangChanged { get; set; }

        internal event Action<int> OnOutputFormatChanged;
        internal event Action<int> OnSyncModeChanged;


        private static Column CreateFeatureColumn(Action<LocalizationLabel, int> bindCell)
        {
            return new Column
            {
                title = "Feature",
                width = FeatureColumnWidth,
                minWidth = FeatureColumnWidth,
                maxWidth = FeatureColumnWidth,
                stretchable = false,
                resizable = false,
                makeCell = CreateFeatureCell,
                bindCell = (elem, index) =>
                {
                    LocalizationLabel label = elem.Q<LocalizationLabel>(FeatureLabelName);
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
                makeCell = CreateHardwareSupportCell,
                bindCell = (elem, index) =>
                {
                    Label label = elem.Q<Label>(HardwareUsageLabelName);
                    bindCell(label, index);
                },
            };
        }

        private Column CreateOutputFormatColumn(Action<LocalizationDropdownField, int> bindCell)
        {
            return new Column
            {
                title = "Output Format",
                stretchable = true,
                makeCell = CreateOutputFormatCell,
                bindCell = (elem, index) =>
                {
                    LocalizationDropdownField field = elem.Q<LocalizationDropdownField>(OutputFormatDropdownName);
                    field.UnregisterValueChangedCallback(HandleOutputFormatChanged);
                    bindCell(field, index);
                    field.userData = index;
                    field.RegisterValueChangedCallback(HandleOutputFormatChanged);
                },
            };
        }

        private Column CreateSyncModeColumn(Action<EnumField, int> bindCell)
        {
            return new Column
            {
                title = "Sync Mode",
                width = SyncModeColumnWidth,
                minWidth = SyncModeColumnWidth,
                maxWidth = SyncModeColumnWidth,
                stretchable = false,
                resizable = false,
                makeCell = CreateSyncModeCell,
                bindCell = (elem, index) =>
                {
                    EnumField field = elem.Q<EnumField>(SyncModeDropdownName);
                    field.UnregisterValueChangedCallback(HandleSyncModeChanged);
                    bindCell(field, index);
                    field.userData = index;
                    field.RegisterValueChangedCallback(HandleSyncModeChanged);
                },
            };
        }

        private static VisualElement CreateFeatureCell()
        {
            VisualElement innerRoot = new()
            {
                style =
                {
                    justifyContent = Justify.Center,
                },
            };
            ApplyColumnCellStyle(innerRoot);

            LocalizationLabel label = new()
            {
                name = FeatureLabelName,
                text = "Feature",
                style =
                {
                    unityTextAlign = TextAnchor.MiddleLeft,
                },
            };
            innerRoot.Add(label);
            return innerRoot;
        }

        private static VisualElement CreateHardwareSupportCell()
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

        private static VisualElement CreateOutputFormatCell()
        {
            VisualElement innerRoot = new();
            ApplyColumnCellStyle(innerRoot);

            LocalizationDropdownField field = new()
            {
                name = OutputFormatDropdownName,
                style =
                {
                    flexGrow = 1,
                },
            };

            innerRoot.Add(field);
            return innerRoot;
        }

        private static VisualElement CreateSyncModeCell()
        {
            VisualElement innerRoot = new();
            ApplyColumnCellStyle(innerRoot);

            EnumField field = new(ParameterSyncMode.LocalOnly)
            {
                name = SyncModeDropdownName,
                style =
                {
                    flexGrow = 1,
                },
            };

            innerRoot.Add(field);
            return innerRoot;
        }

        private void HandleOutputFormatChanged(ChangeEvent<string> evt)
        {
            if (evt.currentTarget is not DropdownField { userData: int index, })
            {
                return;
            }

            schedule.Execute(() => OnOutputFormatChanged?.Invoke(index));
        }

        private void HandleSyncModeChanged(ChangeEvent<Enum> evt)
        {
            if (evt.currentTarget is not EnumField { userData: int index, })
            {
                return;
            }

            schedule.Execute(() => OnSyncModeChanged?.Invoke(index));
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

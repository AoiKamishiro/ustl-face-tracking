using UnityEngine.UIElements;

namespace USTL.Core.Editor
{
    public class MultiColumnListViewLocalizer : IElementLocalizer
    {
        private const string COLUMN_POSTFIX = "__column";

        bool IElementLocalizer.Localize(VisualElement element, string key)
        {
            if (element is not MultiColumnListView view)
            {
                return false;
            }

            string tooltip = USTLLocalizer.GetLocalizedString($"{key}{USTLLocalizer.TooltipPostfix}");

            bool changed = false;

            if (!string.IsNullOrEmpty(tooltip))
            {
                view.tooltip = tooltip;
                changed = true;
            }

            for (int i = 0; i < view.columns.Count; i++)
            {
                Column column = view.columns[i];
                string columnKey = $"{key}{COLUMN_POSTFIX}{i}";
                string columnText = USTLLocalizer.GetLocalizedString(columnKey);
                if (!string.IsNullOrEmpty(columnText))
                {
                    column.title = columnText;
                    changed = true;
                }
            }

            return changed;
        }
    }
}

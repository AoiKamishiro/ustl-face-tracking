using UnityEngine.UIElements;

namespace USTL.Core.Editor
{
    public class FoldoutLocalizer : IElementLocalizer
    {
        bool IElementLocalizer.Localize(VisualElement element, string key)
        {
            if (element is not Foldout foldout)
            {
                return false;
            }

            string text = USTLLocalizer.GetLocalizedString(key);
            string tooltip = USTLLocalizer.GetLocalizedString($"{key}{USTLLocalizer.TooltipPostfix}");

            bool changed = false;

            if (!string.IsNullOrEmpty(text))
            {
                foldout.text = text;
                changed = true;
            }

            if (!string.IsNullOrEmpty(tooltip))
            {
                foldout.tooltip = tooltip;
                changed = true;
            }

            return changed;
        }
    }
}

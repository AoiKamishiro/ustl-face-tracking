using UnityEngine.UIElements;

namespace USTL.Core.Editor
{
    public class TextElementLocalizer : IElementLocalizer
    {
        bool IElementLocalizer.Localize(VisualElement element, string key)
        {
            if (element is not TextElement label)
            {
                return false;
            }

            string text = USTLLocalizer.GetLocalizedString(key);
            string tooltip = USTLLocalizer.GetLocalizedString($"{key}{USTLLocalizer.TooltipPostfix}");

            bool changed = false;

            if (!string.IsNullOrEmpty(text))
            {
                label.text = text;
                changed = true;
            }

            if (!string.IsNullOrEmpty(tooltip))
            {
                label.tooltip = tooltip;
                changed = true;
            }

            return changed;
        }
    }
}

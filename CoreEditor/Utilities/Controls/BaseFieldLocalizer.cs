using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace USTL.Core.Editor
{
    public class BaseFieldLocalizer : IElementLocalizer
    {
        private static readonly Dictionary<Type, bool> TypeTable = new();

        bool IElementLocalizer.Localize(VisualElement element, string key)
        {
            Type type = element.GetType();
            if (!TypeTable.ContainsKey(type))
            {
                TypeTable[type] = IsSubclassOfOpenGeneric(type, typeof(BaseField<>));
            }

            if (!TypeTable[type])
            {
                return false;
            }

            string text = USTLLocalizer.GetLocalizedString(key);
            string tooltip = USTLLocalizer.GetLocalizedString($"{key}{USTLLocalizer.TooltipPostfix}");

            bool changed = false;

            if (!string.IsNullOrEmpty(text))
            {
                TextElement label = element.Q<TextElement>(className: BaseField<object>.labelUssClassName);
                if (label != null)
                {
                    label.text = text;
                    changed = true;
                }
            }

            if (!string.IsNullOrEmpty(tooltip))
            {
                element.tooltip = tooltip;
                changed = true;
            }

            if (element is DropdownField field)
            {
                TextElement popupText = field.Q<TextElement>(className: BasePopupField<string, string>.textUssClassName);
                popupText.text = field.formatSelectedValueCallback?.Invoke(field.value);
            }

            return changed;
        }

        private static bool IsSubclassOfOpenGeneric(Type type, Type openGenericType)
        {
            for (Type current = type; current != null && current != typeof(object); current = current.BaseType)
            {
                Type candidate = current.IsGenericType ? current.GetGenericTypeDefinition() : current;

                if (candidate == openGenericType)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

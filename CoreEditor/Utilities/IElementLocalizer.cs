using UnityEngine.UIElements;

namespace USTL.Core.Editor
{
    public interface IElementLocalizer
    {
        protected internal bool Localize(VisualElement element, string key);
    }
}

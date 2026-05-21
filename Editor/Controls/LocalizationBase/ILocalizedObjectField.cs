using System;
using UnityEditor.UIElements;

namespace USTL.FaceTracking.Editor
{
    public class ILocalizedObjectField : ObjectField, ILocalization
    {
        public Action OnLangChanged { get; set; }
    }
}

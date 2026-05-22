using System;
using UnityEditor.UIElements;

namespace USTL.FaceTracking.Editor
{
    public class LocalizedObjectField : ObjectField, ILocalization
    {
        public Action OnLangChanged { get; set; }
    }
}

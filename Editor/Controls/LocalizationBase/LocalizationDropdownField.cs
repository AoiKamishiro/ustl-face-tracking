using System;
using UnityEngine.UIElements;

namespace USTL.FaceTracking.Editor
{
    public class LocalizationDropdownField : DropdownField, ILocalization
    {
        public Action OnLangChanged { get; set; }
    }
}

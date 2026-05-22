using System;
using UnityEngine.UIElements;

namespace USTL.FaceTracking.Editor
{
    public class LocalizationLabel : Label, ILocalization
    {
        public Action OnLangChanged { get; set; }
    }
}

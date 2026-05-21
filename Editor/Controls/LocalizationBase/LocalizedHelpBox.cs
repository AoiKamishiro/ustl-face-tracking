using System;
using UnityEngine.UIElements;

namespace USTL.FaceTracking.Editor
{
    public class LocalizedHelpBox : HelpBox, ILocalization
    {
        public Action OnLangChanged { get; set; }
    }
}

using System;
using UnityEngine.UIElements;

namespace USTL.FaceTracking.Editor
{
    public class LocalizedFoldout : Foldout, ILocalization
    {
        public Action OnLangChanged { get; set; }
    }
}

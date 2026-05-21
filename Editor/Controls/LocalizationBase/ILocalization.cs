using System;

namespace USTL.FaceTracking.Editor
{
    public interface ILocalization
    {
        Action OnLangChanged { get; set; }
    }
}

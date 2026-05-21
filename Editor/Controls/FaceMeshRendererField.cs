using UnityEngine;

namespace USTL.FaceTracking.Editor
{
    internal sealed class FaceMeshRendererField : ILocalizedObjectField
    {
        internal FaceMeshRendererField()
        {
            objectType = typeof(SkinnedMeshRenderer);
            allowSceneObjects = true;
        }
    }
}

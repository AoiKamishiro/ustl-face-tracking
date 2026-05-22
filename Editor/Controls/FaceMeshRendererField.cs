using UnityEngine;

namespace USTL.FaceTracking.Editor
{
    internal sealed class FaceMeshRendererField : LocalizedObjectField
    {
        internal FaceMeshRendererField()
        {
            objectType = typeof(SkinnedMeshRenderer);
            allowSceneObjects = true;
        }
    }
}

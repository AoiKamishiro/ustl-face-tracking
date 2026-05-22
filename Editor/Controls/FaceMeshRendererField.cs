using UnityEditor.UIElements;
using UnityEngine;

namespace USTL.FaceTracking.Editor
{
    internal sealed class FaceMeshRendererField : ObjectField
    {
        internal FaceMeshRendererField()
        {
            objectType = typeof(SkinnedMeshRenderer);
            allowSceneObjects = true;
        }
    }
}

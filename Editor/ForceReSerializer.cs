using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace USTL.FaceTracking.Editor.Editor
{
    [InitializeOnLoad]
    public static class ForceReSerializer
    {
        private const string path = "Packages/jp.co.u-stella.facetracking";

        static ForceReSerializer()
        {
            IEnumerable<string> list = AssetDatabase.GetAllAssetPaths().Where(c => c.StartsWith(path));
            AssetDatabase.ForceReserializeAssets(list);
        }
    }
}

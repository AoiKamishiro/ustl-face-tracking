using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace USTL.FaceTracking.Editor.Editor
{
    [InitializeOnLoad]
    public static class ForceReSerializer
    {
        private const string PATH = "Packages/jp.co.u-stella.facetracking";

        static ForceReSerializer()
        {
            IEnumerable<string> list = AssetDatabase.GetAllAssetPaths().Where(c => c.StartsWith(PATH));
            AssetDatabase.ForceReserializeAssets(list);
        }
    }
}

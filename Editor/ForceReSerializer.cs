using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace USTL.FaceTracking.Editor
{
    [InitializeOnLoad]
    public static class ForceReserializer
    {
        private const string PATH = "Packages/jp.co.u-stella.facetracking";

        static ForceReserializer()
        {
            IEnumerable<string> list = AssetDatabase.GetAllAssetPaths().Where(c => c.StartsWith(PATH));
            AssetDatabase.ForceReserializeAssets(list);
        }
    }
}

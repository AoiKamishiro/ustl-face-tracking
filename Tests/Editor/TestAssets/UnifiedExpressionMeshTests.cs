using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace USTL.FaceTracking.Editor.Tests
{
    public sealed class UnifiedExpressionMeshTests
    {
        private const string UNIFIED_EXPRESSION_MESH_GUID = "c685687290a384d3aae25b8bf1fb69dc";

        [Test]
        public void MeshAsset_Exists()
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(UNIFIED_EXPRESSION_MESH_GUID);

            Assert.That(assetPath, Is.Not.Empty);
            Assert.That(AssetDatabase.LoadAssetAtPath<Mesh>(assetPath), Is.Not.Null);
        }

        [Test]
        public void MeshAsset_BlendShapesContainEveryUnifiedExpressionExceptNoneAndNoOthers()
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(UNIFIED_EXPRESSION_MESH_GUID);
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);

            IReadOnlyList<string> expectedBlendShapeNames = EnumUtility.GetAllElements<UnifiedExpression>().Select(x => x.ToString()).ToList();
            IReadOnlyList<string> actualBlendShapeNames = MeshUtility.GetBlendShapeNames(mesh);

            Assert.That(actualBlendShapeNames.Count, Is.EqualTo(expectedBlendShapeNames.Count));
            Assert.That(actualBlendShapeNames, Is.Unique);
            Assert.That(actualBlendShapeNames, Is.EquivalentTo(expectedBlendShapeNames));
        }
    }
}

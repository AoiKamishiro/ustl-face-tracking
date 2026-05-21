using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace USTL.FaceTracking.Editor
{
    internal sealed class BlendShapeNameChoices
    {
        private readonly List<string> _choices = new();
        private int _currentBlendShapeCount = -1;
        private Mesh _currentBlendShapeMesh;

        internal bool HasBlendShapeNames => _choices.Count > 1;

        private bool HasCompleteChoices => _choices.Count == _currentBlendShapeCount + 1;

        internal bool Refresh(SerializedObject serializedObject)
        {
            Mesh mesh = GetCurrentBlendShapeMesh(serializedObject);
            int blendShapeCount = mesh ? mesh.blendShapeCount : 0;
            if (_currentBlendShapeMesh == mesh && _currentBlendShapeCount == blendShapeCount && HasCompleteChoices)
            {
                return false;
            }

            _currentBlendShapeMesh = mesh;
            _currentBlendShapeCount = blendShapeCount;

            _choices.Clear();
            _choices.Add(string.Empty);
            _choices.AddRange(FaceTrackingEditorUtility.GetBlendShapeNames(mesh));
            return true;
        }

        internal bool Refresh(Mesh mesh)
        {
            int blendShapeCount = mesh ? mesh.blendShapeCount : 0;
            if (_currentBlendShapeMesh == mesh && _currentBlendShapeCount == blendShapeCount && HasCompleteChoices)
            {
                return false;
            }

            _currentBlendShapeMesh = mesh;
            _currentBlendShapeCount = blendShapeCount;

            _choices.Clear();
            _choices.Add(string.Empty);
            _choices.AddRange(FaceTrackingEditorUtility.GetBlendShapeNames(mesh));
            return true;
        }

        internal List<string> GetChoicesForValue(string currentValue)
        {
            if (string.IsNullOrEmpty(currentValue) || _choices.Contains(currentValue))
            {
                return _choices;
            }

            List<string> choices = new(_choices.Count + 1);
            choices.AddRange(_choices);
            choices.Add(currentValue);
            return choices;
        }

        internal bool Contains(string blendShapeName)
        {
            return _choices.Contains(blendShapeName);
        }

        internal bool IsInvalid(string blendShapeName)
        {
            return _currentBlendShapeCount >= 0 && HasCompleteChoices && !string.IsNullOrEmpty(blendShapeName) && !_choices.Contains(blendShapeName);
        }

        private static Mesh GetCurrentBlendShapeMesh(SerializedObject serializedObject)
        {
            serializedObject.Update();

            SerializedProperty faceMeshRendererProperty = serializedObject.FindProperty(nameof(USTLFaceTracking.faceMeshRenderer));
            if (faceMeshRendererProperty.objectReferenceValue is not SkinnedMeshRenderer faceMeshRenderer || !faceMeshRenderer)
            {
                return null;
            }

            return faceMeshRenderer.sharedMesh;
        }
    }
}

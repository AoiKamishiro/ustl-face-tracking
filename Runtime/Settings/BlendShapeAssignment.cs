using System;
using UnityEngine;

namespace USTL.FaceTracking
{
    [Serializable]
    internal sealed class BlendShapeAssignment
    {
        [SerializeField] internal UnifiedExpression expression;
        [SerializeField] internal string blendShapeName;
        [SerializeField] internal float maxValue;
    }
}

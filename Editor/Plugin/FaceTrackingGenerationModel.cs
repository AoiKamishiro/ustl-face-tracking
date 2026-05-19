using System.Collections.Generic;
using UnityEngine;

namespace USTL.FaceTracking.Editor
{
    internal readonly struct SelectedParameterSetting
    {
        internal SelectedParameterSetting(VRCFTParameter parameter, ParameterSyncMode syncMode)
        {
            Parameter = parameter;
            SyncMode = syncMode;
        }

        internal VRCFTParameter Parameter { get; }
        internal ParameterSyncMode SyncMode { get; }
    }

    internal readonly struct BlendShapeTarget
    {
        internal BlendShapeTarget(string name, float value, WeightCurveType curveType)
        {
            Name = name;
            Value = value;
            CurveType = curveType;
        }

        internal string Name { get; }
        internal float Value { get; }
        internal WeightCurveType CurveType { get; }
    }

    internal readonly struct ParameterSample
    {
        internal ParameterSample(float value, string name)
        {
            Value = value;
            Name = name;
        }

        internal float Value { get; }
        internal string Name { get; }
    }

    internal readonly struct SyncParameterUsage
    {
        internal SyncParameterUsage(int consumedBits, int consumedParameterCount, int expectedParameterCount)
        {
            ConsumedBits = consumedBits;
            ConsumedParameterCount = consumedParameterCount;
            ExpectedParameterCount = expectedParameterCount;
        }

        internal int ConsumedBits { get; }
        internal int ConsumedParameterCount { get; }
        internal int ExpectedParameterCount { get; }
        internal int UnassignedParameterCount => ExpectedParameterCount - ConsumedParameterCount;
    }

    internal static class SyncParameterUsageCalculator
    {
        private const int FloatSyncBitCount = 8;

        internal static SyncParameterUsage Calculate(USTLFaceTracking source)
        {
            return source == null ? default : Calculate(source, SelectedParameterCollector.Collect(source));
        }

        internal static SyncParameterUsage Calculate(USTLFaceTracking source, IReadOnlyList<SelectedParameterSetting> selectedParameters)
        {
            List<SelectedParameterSetting> targetBackedParameters = GetTargetBackedParameters(source, selectedParameters);
            int consumedBits = 0;
            int consumedParameterCount = 0;
            int expectedParameterCount = 0;

            if (selectedParameters != null)
            {
                foreach (SelectedParameterSetting setting in selectedParameters)
                {
                    expectedParameterCount += GetConsumedParameterCount(setting);
                }
            }

            foreach (SelectedParameterSetting setting in targetBackedParameters)
            {
                consumedBits += GetConsumedBitCount(setting);
                consumedParameterCount += GetConsumedParameterCount(setting);
            }

            return new SyncParameterUsage(consumedBits, consumedParameterCount, expectedParameterCount);
        }

        internal static List<SelectedParameterSetting> GetTargetBackedParameters(USTLFaceTracking source, IReadOnlyList<SelectedParameterSetting> selectedParameters)
        {
            List<SelectedParameterSetting> parameters = new();
            if (source == null || selectedParameters == null || !TryGetFaceMesh(source, out Mesh faceMesh))
            {
                return parameters;
            }

            Dictionary<UnifiedExpression, BlendShapeAssignment> assignments = FaceTrackingBlendShapeTargetResolver.BuildAssignmentMap(source.blendShapeAssignments);

            foreach (SelectedParameterSetting setting in selectedParameters)
            {
                if (!VRCFTParameterDefinition.All.TryGetValue(setting.Parameter, out VRCFTParameterDefinition definition))
                {
                    continue;
                }

                if (FaceTrackingBlendShapeTargetResolver.HasValidBlendShapeTarget(definition, assignments, faceMesh))
                {
                    parameters.Add(setting);
                }
            }

            return parameters;
        }

        internal static int GetConsumedBitCount(SelectedParameterSetting setting)
        {
            if (!VRCFTParameterDefinition.All.TryGetValue(setting.Parameter, out VRCFTParameterDefinition definition))
            {
                return 0;
            }

            return GetConsumedBitCount(setting.SyncMode, definition.Range);
        }

        internal static int GetConsumedParameterCount(SelectedParameterSetting setting)
        {
            if (!VRCFTParameterDefinition.All.TryGetValue(setting.Parameter, out VRCFTParameterDefinition definition))
            {
                return 0;
            }

            return GetConsumedParameterCount(setting.SyncMode, definition.Range);
        }

        private static int GetConsumedBitCount(ParameterSyncMode syncMode, ParameterRangeKind range)
        {
            if (syncMode == ParameterSyncMode.Float8)
            {
                return FloatSyncBitCount;
            }

            if (!ParameterSyncModeUtility.IsBinary(syncMode))
            {
                return 0;
            }

            int bitCount = ParameterSyncModeUtility.GetBinaryBitCount(syncMode);
            return range == ParameterRangeKind.Signed ? bitCount + 1 : bitCount;
        }

        private static int GetConsumedParameterCount(ParameterSyncMode syncMode, ParameterRangeKind range)
        {
            if (syncMode == ParameterSyncMode.Float8)
            {
                return 1;
            }

            if (!ParameterSyncModeUtility.IsBinary(syncMode))
            {
                return 0;
            }

            int bitCount = ParameterSyncModeUtility.GetBinaryBitCount(syncMode);
            return range == ParameterRangeKind.Signed ? bitCount + 1 : bitCount;
        }

        private static bool TryGetFaceMesh(USTLFaceTracking source, out Mesh faceMesh)
        {
            faceMesh = null;
            if (!source || !source.faceMeshRenderer)
            {
                return false;
            }

            faceMesh = source.faceMeshRenderer.sharedMesh;
            return faceMesh;
        }
    }

    internal static class FaceTrackingBlendShapeTargetResolver
    {
        internal static Dictionary<UnifiedExpression, BlendShapeAssignment> BuildAssignmentMap(BlendShapeAssignment[] blendShapeAssignments)
        {
            Dictionary<UnifiedExpression, BlendShapeAssignment> assignments = new();
            if (blendShapeAssignments == null)
            {
                return assignments;
            }

            foreach (BlendShapeAssignment assignment in blendShapeAssignments)
            {
                if (assignment == null || assignment.expression == UnifiedExpression.None)
                {
                    continue;
                }

                assignments[assignment.expression] = assignment;
            }

            return assignments;
        }

        internal static List<BlendShapeTarget> BuildBlendShapeTargets(VRCFTParameterDefinition definition, IReadOnlyDictionary<UnifiedExpression, BlendShapeAssignment> assignments, Mesh faceMesh)
        {
            List<BlendShapeTarget> targets = new();
            if (definition == null || assignments == null || !faceMesh)
            {
                return targets;
            }

            foreach (ExpressionWeightTarget expressionTarget in definition.ExpressionTargets)
            {
                if (TryGetValidAssignment(expressionTarget.Expression, assignments, faceMesh, out BlendShapeAssignment assignment))
                {
                    targets.Add(new BlendShapeTarget(assignment.blendShapeName, assignment.maxValue, expressionTarget.Type));
                }
            }

            return targets;
        }

        internal static bool HasValidBlendShapeTarget(VRCFTParameterDefinition definition, IReadOnlyDictionary<UnifiedExpression, BlendShapeAssignment> assignments, Mesh faceMesh)
        {
            if (definition == null || assignments == null || !faceMesh)
            {
                return false;
            }

            foreach (ExpressionWeightTarget expressionTarget in definition.ExpressionTargets)
            {
                if (TryGetValidAssignment(expressionTarget.Expression, assignments, faceMesh, out _))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryGetValidAssignment(UnifiedExpression expression, IReadOnlyDictionary<UnifiedExpression, BlendShapeAssignment> assignments, Mesh faceMesh, out BlendShapeAssignment assignment)
        {
            assignment = null;
            return assignments.TryGetValue(expression, out assignment) && assignment != null && !string.IsNullOrWhiteSpace(assignment.blendShapeName) && faceMesh.GetBlendShapeIndex(assignment.blendShapeName) >= 0;
        }
    }
}

namespace USTL.FaceTracking
{
    internal readonly struct ExpressionWeightTarget
    {
        internal UnifiedExpression Expression { get; }
        internal WeightCurveType Type { get; }

        internal ExpressionWeightTarget(UnifiedExpression expression, WeightCurveType type)
        {
            Expression = expression;
            Type = type;
        }
    }
}

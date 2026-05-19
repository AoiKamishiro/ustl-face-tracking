using System.Collections.Generic;

namespace USTL.FaceTracking.Editor
{
    internal readonly struct OutputFormatExpressionUsageResult
    {
        internal OutputFormatExpressionUsageResult(OutputFormatUsageStatus status, IReadOnlyList<VRCFTParameter> parameters)
        {
            Status = status;
            Parameters = parameters;
        }

        internal OutputFormatUsageStatus Status { get; }
        internal IReadOnlyList<VRCFTParameter> Parameters { get; }
    }
}

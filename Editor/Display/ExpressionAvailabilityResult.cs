using System.Collections.Generic;

namespace USTL.FaceTracking.Editor
{
    internal readonly struct ExpressionAvailabilityResult
    {
        internal ExpressionAvailabilityResult(HardwareKeyAvailabilityStatus status, IReadOnlyList<VRCFTParameter> parameters)
        {
            Status = status;
            Parameters = parameters;
        }

        internal HardwareKeyAvailabilityStatus Status { get; }
        internal IReadOnlyList<VRCFTParameter> Parameters { get; }
    }
}

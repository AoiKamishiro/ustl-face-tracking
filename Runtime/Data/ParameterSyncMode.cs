using UnityEngine;

namespace USTL.FaceTracking
{
    internal enum ParameterSyncMode
    {
        [InspectorName("Disabled")] None,
        [InspectorName("Local Only")] LocalOnly,
        [InspectorName("Float (8-bit)")] Float8,
        [InspectorName("Binary (1-bit)")] Binary1Bit,
        [InspectorName("Binary (2-bit)")] Binary2Bit,
        [InspectorName("Binary (3-bit)")] Binary3Bit,
        [InspectorName("Binary (4-bit)")] Binary4Bit,
    }
}

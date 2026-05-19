using UnityEngine;

namespace USTL.FaceTracking.Editor
{
    internal static class BinaryParameterEncoding
    {
        internal static int GetMagnitude(float value, int bitCount)
        {
            if (bitCount <= 0)
            {
                return 0;
            }

            int maxMagnitude = (1 << bitCount) - 1;
            return Mathf.Clamp(Mathf.RoundToInt(Mathf.Abs(value) * maxMagnitude), 0, maxMagnitude);
        }

        internal static bool IsSignedParameter(VRCFTParameter parameter)
        {
            return VRCFTParameterDefinition.All.TryGetValue(parameter, out VRCFTParameterDefinition definition) && definition.Range == ParameterRangeKind.Signed;
        }
    }
}

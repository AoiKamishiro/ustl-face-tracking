namespace USTL.FaceTracking
{
    internal sealed class VRCFTParameterOutputFormat
    {
        internal VRCFTParameterOutputFormat(string displayName, params VRCFTParameter[] parameters) : this(displayName, null, parameters)
        {
        }

        internal VRCFTParameterOutputFormat(string displayName, string description, params VRCFTParameter[] parameters)
        {
            DisplayName = displayName;
            Description = description;
            Parameters = parameters;
        }

        internal string DisplayName { get; }
        internal string Description { get; }
        internal VRCFTParameter[] Parameters { get; }
    }
}

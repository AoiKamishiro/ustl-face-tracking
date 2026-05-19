using System.Collections.Generic;

namespace USTL.FaceTracking.Editor
{
    internal static class OutputFormatUsageDisplay
    {
        internal static string FormatStatus(OutputFormatUsageStatus status)
        {
            return status switch
            {
                OutputFormatUsageStatus.Emitted => T("output_format_usage.Emitted", "Emitted"),
                OutputFormatUsageStatus.NotEmitted => T("output_format_usage.NotEmitted", "Not emitted"),
                _ => string.Empty,
            };
        }

        internal static string FormatTooltip(UnifiedExpression expression, OutputFormatExpressionUsageResult usage)
        {
            if (usage.Status == OutputFormatUsageStatus.Emitted)
            {
                return string.Format(T("output_format_usage.Emitted.tooltip", "{0} is emitted by the selected output format through: {1}"), expression, FormatParameterList(usage.Parameters));
            }

            return string.Format(T("output_format_usage.NotEmitted.tooltip", "{0} is not emitted by the selected output format."), expression);
        }

        private static string FormatParameterList(IReadOnlyList<VRCFTParameter> parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                return string.Empty;
            }

            List<string> values = new();
            foreach (VRCFTParameter parameter in parameters)
            {
                values.Add($"{parameter}");
            }

            return string.Join(", ", values);
        }

        private static string T(string key, string fallback)
        {
            return FaceTrackingEditorText.Get(key, fallback);
        }
    }
}

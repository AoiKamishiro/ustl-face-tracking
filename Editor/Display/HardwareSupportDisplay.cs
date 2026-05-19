using System.Collections.Generic;

namespace USTL.FaceTracking.Editor
{
    internal static class HardwareSupportDisplay
    {
        internal static IReadOnlyList<HardwareSupportProfile> Profiles => HardwareSupportData.Profiles;

        internal static List<string> GetProfileChoices()
        {
            List<string> choices = new()
            {
                FormatNoProfile(),
            };

            foreach (HardwareSupportProfile profile in Profiles)
            {
                choices.Add(FormatProfile(profile));
            }

            return choices;
        }

        internal static int FindProfile(string displayName)
        {
            foreach (HardwareSupportProfile profile in Profiles)
            {
                if (FormatProfile(profile) == displayName)
                {
                    return profile.Flag;
                }
            }

            return 0;
        }

        internal static string FormatNoProfile()
        {
            return T("hardware.None", "No Tracking Hardware");
        }

        internal static string FormatProfile(HardwareSupportProfile profile)
        {
            return HardwareSupportData.GetProfileDisplayName(profile);
        }

        internal static bool HasProfile(int profiles, HardwareSupportProfile profile)
        {
            return profile.Flag != 0 && (profiles & profile.Flag) == profile.Flag;
        }

        internal static string FormatSelectedProfiles(int profiles)
        {
            List<string> selectedProfiles = GetSelectedProfileNames(profiles);
            if (selectedProfiles.Count == 0)
            {
                return FormatNoProfile();
            }

            if (selectedProfiles.Count <= 2)
            {
                return string.Join(", ", selectedProfiles);
            }

            return string.Format(T("hardware.selected_count", "{0} tracking hardware devices selected"), selectedProfiles.Count);
        }

        private static List<string> GetSelectedProfileNames(int profiles)
        {
            List<string> selectedProfiles = new();
            foreach (HardwareSupportProfile profile in Profiles)
            {
                if (HasProfile(profiles, profile))
                {
                    selectedProfiles.Add(FormatProfile(profile));
                }
            }

            return selectedProfiles;
        }

        internal static string FormatStatus(HardwareSupportStatus status)
        {
            return status switch
            {
                HardwareSupportStatus.Full => T("support.Full", "Full"),
                HardwareSupportStatus.Converted => T("support.Converted", "Converted"),
                HardwareSupportStatus.Unsupported => T("support.Unsupported", "Unsupported"),
                HardwareSupportStatus.Unknown => T("support.Unknown", "Unknown"),
                _ => string.Empty,
            };
        }

        internal static string FormatKeyAvailabilityStatus(HardwareKeyAvailabilityStatus status)
        {
            return status switch
            {
                HardwareKeyAvailabilityStatus.Available => T("hardware_key.Available", "Available"),
                HardwareKeyAvailabilityStatus.Unavailable => T("hardware_key.Unavailable", "Unavailable"),
                HardwareKeyAvailabilityStatus.Unknown => T("hardware_key.Unknown", "Unknown"),
                _ => string.Empty,
            };
        }

        internal static ExpressionAvailabilityResult GetExpressionAvailability(int profiles, UnifiedExpression expression)
        {
            IReadOnlyList<VRCFTParameter> parameters = GetExpressionParameters(expression);
            if (profiles == 0)
            {
                return new ExpressionAvailabilityResult(HardwareKeyAvailabilityStatus.Unknown, parameters);
            }

            if (parameters.Count == 0)
            {
                return new ExpressionAvailabilityResult(HardwareKeyAvailabilityStatus.Unavailable, parameters);
            }

            HardwareSupportStatus status = GetExpressionStatus(profiles, expression);
            return new ExpressionAvailabilityResult(ToKeyAvailabilityStatus(status), parameters);
        }

        internal static IReadOnlyList<VRCFTParameter> GetExpressionParameters(UnifiedExpression expression)
        {
            return HardwareSupportData.GetExpressionParameters(expression);
        }

        internal static HardwareSupportStatus GetExpressionStatus(int profiles, UnifiedExpression expression)
        {
            if (profiles == 0)
            {
                return HardwareSupportStatus.Unknown;
            }

            HardwareSupportStatus status = HardwareSupportStatus.Unsupported;
            foreach (HardwareSupportProfile profile in Profiles)
            {
                if (!HasProfile(profiles, profile))
                {
                    continue;
                }

                status = BestStatus(status, HardwareSupportData.GetExpressionStatus(profile, expression));
                if (status == HardwareSupportStatus.Full)
                {
                    return status;
                }
            }

            return status;
        }

        internal static string FormatStatusTooltip(int profiles, string featureName, string outputFormatName, VRCFTParameterOutputFormat outputFormat, HardwareSupportStatus status)
        {
            string profileName = FormatSelectedProfiles(profiles);
            string explanation = FormatStatusExplanation(status);
            string parameterBreakdown = FormatParameterBreakdown(profiles, outputFormat);

            return string.IsNullOrEmpty(parameterBreakdown) ? $"{profileName} / {featureName} / {outputFormatName}\n{FormatStatus(status)}\n\n{explanation}" : $"{profileName} / {featureName} / {outputFormatName}\n{FormatStatus(status)}\n\n{explanation}\n\n{parameterBreakdown}";
        }

        internal static string FormatExpressionAvailabilityTooltip(int profiles, UnifiedExpression expression, ExpressionAvailabilityResult result)
        {
            string profileName = FormatSelectedProfiles(profiles);
            string explanation = FormatKeyAvailabilityExplanation(result.Status);
            string parameterBreakdown = FormatExpressionParameterBreakdown(profiles, result.Parameters);

            return string.IsNullOrEmpty(parameterBreakdown) ? $"{profileName} / {expression}\n{FormatKeyAvailabilityStatus(result.Status)}\n\n{explanation}" : $"{profileName} / {expression}\n{FormatKeyAvailabilityStatus(result.Status)}\n\n{explanation}\n\n{parameterBreakdown}";
        }

        internal static HardwareSupportStatus GetOutputFormatStatus(int profiles, VRCFTParameterOutputFormat outputFormat)
        {
            if (profiles == 0 || outputFormat == null || outputFormat.Parameters.Length == 0)
            {
                return HardwareSupportStatus.Unknown;
            }

            bool hasFull = false;
            bool hasConverted = false;
            bool hasUnknown = false;
            bool hasUnsupported = false;

            foreach (VRCFTParameter parameter in outputFormat.Parameters)
            {
                switch (GetParameterStatus(profiles, parameter))
                {
                    case HardwareSupportStatus.Full:
                        hasFull = true;
                        break;
                    case HardwareSupportStatus.Converted:
                        hasConverted = true;
                        break;
                    case HardwareSupportStatus.Unknown:
                        hasUnknown = true;
                        break;
                    case HardwareSupportStatus.Unsupported:
                        hasUnsupported = true;
                        break;
                }
            }

            if (hasFull && !hasConverted && !hasUnknown && !hasUnsupported)
            {
                return HardwareSupportStatus.Full;
            }

            if (hasFull || hasConverted)
            {
                return HardwareSupportStatus.Converted;
            }

            return hasUnknown ? HardwareSupportStatus.Unknown : HardwareSupportStatus.Unsupported;
        }

        internal static string FormatStatusTooltip(HardwareSupportProfile profile, string featureName, HardwareSupportStatus status)
        {
            string profileName = FormatProfile(profile);
            string explanation = FormatStatusExplanation(status);

            return $"{profileName} / {featureName}\n{FormatStatus(status)}\n\n{explanation}";
        }

        private static string FormatStatusExplanation(HardwareSupportStatus status)
        {
            return status switch
            {
                HardwareSupportStatus.Full => T("support.Full.tooltip", "The selected hardware can provide this signal directly."),
                HardwareSupportStatus.Converted => T("support.Converted.tooltip", "The selected hardware can provide this signal, but the value may be composite, emulated, shared between sides or axes, or converted by this module."),
                HardwareSupportStatus.Unsupported => T("support.Unsupported.tooltip", "The report does not list this signal as supported by the selected hardware."),
                HardwareSupportStatus.Unknown => T("support.Unknown.tooltip", "The reviewed sources did not confirm whether this signal is supported by the selected hardware."),
                _ => string.Empty,
            };
        }

        private static HardwareKeyAvailabilityStatus ToKeyAvailabilityStatus(HardwareSupportStatus status)
        {
            return status switch
            {
                HardwareSupportStatus.Full => HardwareKeyAvailabilityStatus.Available,
                HardwareSupportStatus.Converted => HardwareKeyAvailabilityStatus.Available,
                HardwareSupportStatus.Unknown => HardwareKeyAvailabilityStatus.Unknown,
                _ => HardwareKeyAvailabilityStatus.Unavailable,
            };
        }

        private static string FormatKeyAvailabilityExplanation(HardwareKeyAvailabilityStatus status)
        {
            return status switch
            {
                HardwareKeyAvailabilityStatus.Available => T("hardware_key.Available.tooltip", "The selected hardware can emit this Unified Expression."),
                HardwareKeyAvailabilityStatus.Unavailable => T("hardware_key.Unavailable.tooltip", "The selected hardware cannot emit this Unified Expression."),
                HardwareKeyAvailabilityStatus.Unknown => T("hardware_key.Unknown.tooltip", "The reviewed sources did not confirm whether the selected hardware can emit this Unified Expression."),
                _ => string.Empty,
            };
        }

        private static string FormatParameterBreakdown(int profiles, VRCFTParameterOutputFormat outputFormat)
        {
            if (outputFormat == null || outputFormat.Parameters.Length == 0)
            {
                return string.Empty;
            }

            List<string> entries = new();
            foreach (VRCFTParameter parameter in outputFormat.Parameters)
            {
                HardwareSupportStatus status = GetParameterStatus(profiles, parameter);
                if (status == HardwareSupportStatus.Full)
                {
                    continue;
                }

                entries.Add($"{parameter}: {FormatStatus(status)}");
            }

            if (entries.Count == 0)
            {
                return string.Empty;
            }

            return $"{T("tooltip.parameter_breakdown", "Not fully supported parameters")}:\n{string.Join(", ", entries)}";
        }

        private static string FormatExpressionParameterBreakdown(int profiles, IReadOnlyList<VRCFTParameter> parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                return T("tooltip.expression_no_parameters", "No VRCFT parameter in this package emits this Unified Expression.");
            }

            List<string> entries = new();
            foreach (VRCFTParameter parameter in parameters)
            {
                entries.Add($"{parameter}: {FormatStatus(GetParameterStatus(profiles, parameter))}");
            }

            return $"{T("tooltip.expression_parameters", "VRCFT parameters that can emit this expression")}:\n{string.Join(", ", entries)}";
        }

        internal static HardwareSupportStatus GetParameterStatus(int profiles, VRCFTParameter parameter)
        {
            if (profiles == 0)
            {
                return HardwareSupportStatus.Unknown;
            }

            HardwareSupportStatus status = HardwareSupportStatus.Unsupported;
            foreach (HardwareSupportProfile profile in Profiles)
            {
                if (!HasProfile(profiles, profile))
                {
                    continue;
                }

                status = BestStatus(status, GetSingleProfileParameterStatus(profile, parameter));
                if (status == HardwareSupportStatus.Full)
                {
                    return status;
                }
            }

            return status;
        }

        private static HardwareSupportStatus BestStatus(HardwareSupportStatus current, HardwareSupportStatus candidate)
        {
            return StatusRank(candidate) > StatusRank(current) ? candidate : current;
        }

        private static int StatusRank(HardwareSupportStatus status)
        {
            return status switch
            {
                HardwareSupportStatus.Full => 3,
                HardwareSupportStatus.Converted => 2,
                HardwareSupportStatus.Unknown => 1,
                _ => 0,
            };
        }

        private static HardwareSupportStatus GetSingleProfileParameterStatus(HardwareSupportProfile profile, VRCFTParameter parameter)
        {
            return HardwareSupportData.GetParameterStatus(profile, parameter);
        }

        private static string T(string key, string fallback)
        {
            return FaceTrackingEditorText.Get(key, fallback);
        }
    }
}

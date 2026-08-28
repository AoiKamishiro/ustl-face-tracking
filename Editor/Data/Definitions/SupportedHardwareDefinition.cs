using System;
using System.Collections.Generic;

namespace USTL.FaceTracking.Editor
{
    internal partial class SupportedHardwareDefinition
    {
        private readonly HashSet<UnifiedExpression> _converted;
        private readonly HashSet<UnifiedExpression> _full;
        private readonly HashSet<UnifiedExpression> _unknown;

        private SupportedHardwareDefinition(SupportedHardwares hardware, string displayName, int id, IReadOnlyList<HardwareSupportSource> sources, IEnumerable<UnifiedExpression> full, IEnumerable<UnifiedExpression> converted, IEnumerable<UnifiedExpression> unknown)
        {
            Hardware = hardware;
            DisplayName = displayName;
            Id = id;
            Sources = sources ?? Array.Empty<HardwareSupportSource>();
            FullExpressions = ToArray(full);
            ConvertedExpressions = ToArray(converted);
            UnknownExpressions = ToArray(unknown);
            _full = new HashSet<UnifiedExpression>(FullExpressions);
            _converted = new HashSet<UnifiedExpression>(ConvertedExpressions);
            _unknown = new HashSet<UnifiedExpression>(UnknownExpressions);
        }

        internal SupportedHardwares Hardware { get; }

        internal string DisplayName { get; }

        internal int Id { get; }

        internal IReadOnlyList<HardwareSupportSource> Sources { get; }

        internal IReadOnlyList<UnifiedExpression> FullExpressions { get; }

        internal IReadOnlyList<UnifiedExpression> ConvertedExpressions { get; }

        internal IReadOnlyList<UnifiedExpression> UnknownExpressions { get; }

        internal HardwareSupportStatus GetStatus(UnifiedExpression expression)
        {
            if (_full.Contains(expression))
            {
                return HardwareSupportStatus.Full;
            }

            if (_converted.Contains(expression))
            {
                return HardwareSupportStatus.Converted;
            }

            return _unknown.Contains(expression) ? HardwareSupportStatus.Unknown : HardwareSupportStatus.Unsupported;
        }

        internal HardwareSupportStatus GetStatus(VRCFTParameter parameter)
        {
            if (Hardware == SupportedHardwares.None)
            {
                return HardwareSupportStatus.Unknown;
            }

            if (!VRCFTParameterDefinition.All.TryGetValue(parameter, out VRCFTParameterDefinition definition) || definition.ExpressionTargets.Count == 0)
            {
                return HardwareSupportStatus.Unsupported;
            }

            bool hasFull = false;
            bool hasConverted = false;
            bool hasUnknown = false;
            bool hasUnsupported = false;
            List<UnifiedExpression> expressions = new();

            foreach (ExpressionWeightTarget target in definition.ExpressionTargets)
            {
                if (target.Expression == UnifiedExpression.None || expressions.Contains(target.Expression))
                {
                    continue;
                }

                expressions.Add(target.Expression);
                switch (GetStatus(target.Expression))
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

            if (expressions.Count == 0)
            {
                return HardwareSupportStatus.Unsupported;
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

        internal HardwareSupportStatus GetStatus(VRCFTParameterSet set)
        {
            if (Hardware == SupportedHardwares.None || set == null || set.Parameters.Count == 0)
            {
                return HardwareSupportStatus.Unknown;
            }

            bool hasFull = false;
            bool hasConverted = false;
            bool hasUnknown = false;
            bool hasUnsupported = false;

            foreach (VRCFTParameter parameter in set.Parameters)
            {
                switch (GetStatus(parameter))
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

        private static UnifiedExpression[] ToArray(IEnumerable<UnifiedExpression> expressions)
        {
            if (expressions == null)
            {
                return Array.Empty<UnifiedExpression>();
            }

            List<UnifiedExpression> result = new();
            foreach (UnifiedExpression expression in expressions)
            {
                result.Add(expression);
            }

            return result.ToArray();
        }
    }

    internal enum HardwareSupportStatus
    {
        Full,
        Converted,
        Unsupported,
        Unknown,
    }

    internal sealed class HardwareSupportSource
    {
        internal HardwareSupportSource(string title, string url, string note)
        {
            Title = title;
            Url = url;
            Note = note;
        }

        internal string Title { get; }

        internal string Url { get; }

        internal string Note { get; }
    }
}

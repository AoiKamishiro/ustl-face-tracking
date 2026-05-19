using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace USTL.FaceTracking.Editor
{
    internal static class HardwareSupportData
    {
        private const string ProfileAssetsDirectoryGuid = "acb8333e55094b2caa2065288be3ac3e";

        private static readonly VRCFTParameter[] EmptyParameters = Array.Empty<VRCFTParameter>();

        private static readonly Lazy<IReadOnlyList<HardwareSupportProfileJson>> ProfileJsons = new(LoadProfileJsons);

        private static readonly Lazy<IReadOnlyList<HardwareSupportProfileRecord>> ProfileRecords = new(BuildProfileRecords);

        private static readonly Lazy<IReadOnlyDictionary<FaceTrackingHardwareProfile, HardwareSupportProfileRecord>> ProfileRecordsByProfile = new(BuildProfileRecordLookup);

        private static readonly Lazy<IReadOnlyList<FaceTrackingHardwareProfile>> ProfileOrder = new(BuildProfileOrder);

        private static readonly Lazy<IReadOnlyDictionary<FaceTrackingHardwareProfile, HardwareSupportProfileDefinition>> Definitions = new(BuildDefinitions);

        private static readonly IReadOnlyDictionary<UnifiedExpression, VRCFTParameter[]> ExpressionParameters = BuildExpressionParameters();

        internal static IReadOnlyList<FaceTrackingHardwareProfile> Profiles => ProfileOrder.Value;

        internal static string GetProfileDisplayName(FaceTrackingHardwareProfile profile)
        {
            return ProfileRecordsByProfile.Value.TryGetValue(profile, out HardwareSupportProfileRecord record) ? record.DisplayName : $"{profile}";
        }

        internal static HardwareSupportStatus GetExpressionStatus(FaceTrackingHardwareProfile profile, UnifiedExpression expression)
        {
            return Definitions.Value.TryGetValue(profile, out HardwareSupportProfileDefinition definition) ? definition.GetStatus(expression) : HardwareSupportStatus.Unsupported;
        }

        internal static HardwareSupportStatus GetParameterStatus(FaceTrackingHardwareProfile profile, VRCFTParameter parameter)
        {
            if (!VRCFTParameterDefinition.All.TryGetValue(parameter, out VRCFTParameterDefinition definition) || definition.ExpressionTargets.Length == 0)
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
                switch (GetExpressionStatus(profile, target.Expression))
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

        internal static IReadOnlyList<VRCFTParameter> GetExpressionParameters(UnifiedExpression expression)
        {
            return ExpressionParameters.GetValueOrDefault(expression, EmptyParameters);
        }

        private static IReadOnlyDictionary<FaceTrackingHardwareProfile, HardwareSupportProfileDefinition> BuildDefinitions()
        {
            Dictionary<FaceTrackingHardwareProfile, HardwareSupportProfileDefinition> definitions = new();

            foreach (HardwareSupportProfileRecord record in ProfileRecords.Value)
            {
                definitions[record.Profile] = record.Definition;
            }

            return definitions;
        }

        private static IReadOnlyList<HardwareSupportProfileJson> LoadProfileJsons()
        {
            string profileAssetsPath = GetProfileAssetsPath();
            string[] profileAssetGuids = AssetDatabase.FindAssets("t:TextAsset", new[] { profileAssetsPath, });
            List<string> profileAssetPaths = new();
            foreach (string guid in profileAssetGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith(".json", StringComparison.Ordinal))
                {
                    profileAssetPaths.Add(path);
                }
            }

            profileAssetPaths.Sort(StringComparer.Ordinal);
            List<HardwareSupportProfileJson> profiles = new();
            foreach (string path in profileAssetPaths)
            {
                profiles.Add(LoadJsonAsset<HardwareSupportProfileJson>(path, "hardware support profile JSON"));
            }

            if (profiles.Count == 0)
            {
                throw new InvalidOperationException($"Hardware support profile JSON files were not found: {profileAssetsPath}");
            }

            return profiles;
        }

        private static string GetProfileAssetsPath()
        {
            string path = AssetDatabase.GUIDToAssetPath(ProfileAssetsDirectoryGuid);
            if (string.IsNullOrEmpty(path))
            {
                throw new InvalidOperationException($"Hardware support profile directory was not found: {ProfileAssetsDirectoryGuid}");
            }

            return path;
        }

        private static T LoadJsonAsset<T>(string path, string description) where T : class
        {
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            if (asset == null)
            {
                throw new InvalidOperationException($"{description} was not found: {path}");
            }

            try
            {
                T data = JsonUtility.FromJson<T>(asset.text);
                if (data == null)
                {
                    throw new InvalidOperationException($"{description} is empty.");
                }

                return data;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException($"Failed to parse {description}: {path}", exception);
            }
        }

        private static IReadOnlyList<HardwareSupportProfileRecord> BuildProfileRecords()
        {
            List<HardwareSupportProfileRecord> records = new();
            HashSet<FaceTrackingHardwareProfile> profiles = new();
            HashSet<int> displayOrders = new();

            foreach (HardwareSupportProfileJson profileJson in ProfileJsons.Value)
            {
                if (profileJson == null)
                {
                    continue;
                }

                FaceTrackingHardwareProfile profile = ParseProfile(profileJson.profile);

                if (profile == FaceTrackingHardwareProfile.None)
                {
                    throw new InvalidOperationException("Hardware support definition cannot use the None profile.");
                }

                if (string.IsNullOrWhiteSpace(profileJson.displayName))
                {
                    throw new InvalidOperationException($"Hardware support definition has no display name: {profile}");
                }

                if (profileJson.sources == null || profileJson.sources.Length == 0)
                {
                    throw new InvalidOperationException($"Hardware support definition has no source: {profile}");
                }

                if (!profiles.Add(profile))
                {
                    throw new InvalidOperationException($"Duplicate hardware support definition: {profile}");
                }

                if (!displayOrders.Add(profileJson.displayOrder))
                {
                    throw new InvalidOperationException($"Duplicate hardware support display order: {profileJson.displayOrder}");
                }

                HardwareSupportProfileDefinition definition = new(ResolveProfileExpressions(profileJson, "full", profileJson.full), ResolveProfileExpressions(profileJson, "converted", profileJson.converted), ResolveProfileExpressions(profileJson, "unknown", profileJson.unknown));

                records.Add(new HardwareSupportProfileRecord(profile, profileJson.displayName, profileJson.displayOrder, definition));
            }

            if (records.Count == 0)
            {
                throw new InvalidOperationException("Hardware support JSON does not define any profiles.");
            }

            records.Sort((left, right) =>
            {
                int order = left.DisplayOrder.CompareTo(right.DisplayOrder);
                return order != 0 ? order : string.CompareOrdinal(left.DisplayName, right.DisplayName);
            });

            return records;
        }

        private static IReadOnlyDictionary<FaceTrackingHardwareProfile, HardwareSupportProfileRecord> BuildProfileRecordLookup()
        {
            Dictionary<FaceTrackingHardwareProfile, HardwareSupportProfileRecord> records = new();

            foreach (HardwareSupportProfileRecord record in ProfileRecords.Value)
            {
                records[record.Profile] = record;
            }

            return records;
        }

        private static IReadOnlyList<FaceTrackingHardwareProfile> BuildProfileOrder()
        {
            List<FaceTrackingHardwareProfile> profiles = new();
            foreach (HardwareSupportProfileRecord record in ProfileRecords.Value)
            {
                profiles.Add(record.Profile);
            }

            return profiles;
        }

        private static UnifiedExpression[] ResolveProfileExpressions(HardwareSupportProfileJson profile, string statusName, string[] expressionNames)
        {
            List<UnifiedExpression> expressions = new();
            string context = $"profile '{profile.profile}' {statusName}";

            AddExpressions(expressions, ResolveExpressionNames(expressionNames, context));
            return expressions.ToArray();
        }

        private static UnifiedExpression[] ResolveExpressionNames(string[] expressionNames, string context)
        {
            List<UnifiedExpression> expressions = new();
            foreach (string expressionName in expressionNames ?? Array.Empty<string>())
            {
                AddExpression(expressions, ParseExpression(expressionName, context));
            }

            return expressions.ToArray();
        }

        private static void AddExpressions(List<UnifiedExpression> expressions, IEnumerable<UnifiedExpression> source)
        {
            foreach (UnifiedExpression expression in source)
            {
                AddExpression(expressions, expression);
            }
        }

        private static void AddExpression(List<UnifiedExpression> expressions, UnifiedExpression expression)
        {
            if (!expressions.Contains(expression))
            {
                expressions.Add(expression);
            }
        }

        private static FaceTrackingHardwareProfile ParseProfile(string profileName)
        {
            if (string.IsNullOrWhiteSpace(profileName))
            {
                throw new InvalidOperationException("Hardware support definition has no profile.");
            }

            if (!Enum.TryParse(profileName.Trim(), out FaceTrackingHardwareProfile profile))
            {
                throw new InvalidOperationException($"Unknown hardware support profile: {profileName}");
            }

            return profile;
        }

        private static UnifiedExpression ParseExpression(string expressionName, string context)
        {
            if (string.IsNullOrWhiteSpace(expressionName))
            {
                throw new InvalidOperationException($"Hardware support {context} contains an empty expression name.");
            }

            if (!Enum.TryParse(expressionName.Trim(), out UnifiedExpression expression) || expression == UnifiedExpression.None)
            {
                throw new InvalidOperationException($"Unknown hardware support expression '{expressionName}' in {context}.");
            }

            return expression;
        }

        private static IReadOnlyDictionary<UnifiedExpression, VRCFTParameter[]> BuildExpressionParameters()
        {
            Dictionary<UnifiedExpression, List<VRCFTParameter>> byExpression = new();
            foreach (KeyValuePair<VRCFTParameter, VRCFTParameterDefinition> item in VRCFTParameterDefinition.All)
            {
                foreach (ExpressionWeightTarget target in item.Value.ExpressionTargets)
                {
                    AddExpressionParameter(byExpression, target.Expression, item.Key);
                }
            }

            Dictionary<UnifiedExpression, VRCFTParameter[]> result = new();
            foreach (KeyValuePair<UnifiedExpression, List<VRCFTParameter>> item in byExpression)
            {
                result[item.Key] = item.Value.ToArray();
            }

            return result;
        }

        private static void AddExpressionParameter(Dictionary<UnifiedExpression, List<VRCFTParameter>> byExpression, UnifiedExpression expression, VRCFTParameter parameter)
        {
            if (expression == UnifiedExpression.None)
            {
                return;
            }

            if (!byExpression.TryGetValue(expression, out List<VRCFTParameter> parameters))
            {
                parameters = new List<VRCFTParameter>();
                byExpression[expression] = parameters;
            }

            if (!parameters.Contains(parameter))
            {
                parameters.Add(parameter);
            }
        }

        [Serializable]
        private sealed class HardwareSupportProfileJson
        {
            public string profile;
            public string displayName;
            public int displayOrder;
            public HardwareSupportSourceJson[] sources;
            public string[] full;
            public string[] converted;
            public string[] unknown;
        }

        [Serializable]
        private sealed class HardwareSupportSourceJson
        {
            public string title;
            public string url;
            public string note;
        }

        private sealed class HardwareSupportProfileRecord
        {
            internal HardwareSupportProfileRecord(FaceTrackingHardwareProfile profile, string displayName, int displayOrder, HardwareSupportProfileDefinition definition)
            {
                Profile = profile;
                DisplayName = displayName;
                DisplayOrder = displayOrder;
                Definition = definition;
            }

            internal FaceTrackingHardwareProfile Profile { get; }

            internal string DisplayName { get; }

            internal int DisplayOrder { get; }

            internal HardwareSupportProfileDefinition Definition { get; }
        }
    }

    internal sealed class HardwareSupportProfileDefinition
    {
        private readonly HashSet<UnifiedExpression> _converted;
        private readonly HashSet<UnifiedExpression> _full;
        private readonly HashSet<UnifiedExpression> _unknown;

        internal HardwareSupportProfileDefinition(IEnumerable<UnifiedExpression> full, IEnumerable<UnifiedExpression> converted, IEnumerable<UnifiedExpression> unknown)
        {
            _full = new HashSet<UnifiedExpression>(full ?? Array.Empty<UnifiedExpression>());
            _converted = new HashSet<UnifiedExpression>(converted ?? Array.Empty<UnifiedExpression>());
            _unknown = new HashSet<UnifiedExpression>(unknown ?? Array.Empty<UnifiedExpression>());
        }

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
    }
}

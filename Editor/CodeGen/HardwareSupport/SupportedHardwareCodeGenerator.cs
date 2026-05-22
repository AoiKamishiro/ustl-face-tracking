using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace USTL.FaceTracking.Editor
{
    internal static class HardwareSupportCodeGenerator
    {
        private const string ProfileAssetsDirectoryGuid = "acb8333e55094b2caa2065288be3ac3e";
        private const string ProfilesDirectorySuffix = "/Editor/Generation/HardwareSupport/Profiles";
        private const string RuntimeGeneratedRelativePath = "Runtime/Data/SupportedHardwares.generated.cs";
        private const string EditorGeneratedRelativePath = "Editor/Data/Definitions/SupportedHardwareDefinition.generated.cs";
        private const string MenuPath = "Tools/U-Stella/Face Tracking/Reload Hardware Data";
        private const int MaxId = 30;

        private static readonly UTF8Encoding Utf8NoBom = new(false);

        [MenuItem(MenuPath)]
        internal static void Generate()
        {
            IReadOnlyList<HardwareSupportProfileJson> profiles = LoadProfileJsons();
            string packageRoot = GetPackageRoot();

            WriteIfChanged(Path.Combine(packageRoot, RuntimeGeneratedRelativePath), GenerateSupportedHardwares(profiles));
            WriteIfChanged(Path.Combine(packageRoot, EditorGeneratedRelativePath), GenerateProfileDefinitions(profiles));
            AssetDatabase.Refresh();
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
            HashSet<string> profileNames = new(StringComparer.Ordinal);
            HashSet<int> profileIds = new();
            foreach (string path in profileAssetPaths)
            {
                HardwareSupportProfileJson profile = LoadJsonAsset<HardwareSupportProfileJson>(path, "hardware support profile JSON");
                profile.AssetPath = path;
                ValidateProfile(profile, profileNames, profileIds);
                profile.full = ResolveExpressionNames(profile.full, profile, "full");
                profile.converted = ResolveExpressionNames(profile.converted, profile, "converted");
                profile.unknown = ResolveExpressionNames(profile.unknown, profile, "unknown");
                profiles.Add(profile);
            }

            if (profiles.Count == 0)
            {
                throw new InvalidOperationException($"Hardware support profile JSON files were not found: {profileAssetsPath}");
            }

            profiles.Sort((left, right) =>
            {
                int order = left.id.CompareTo(right.id);
                return order != 0 ? order : string.CompareOrdinal(left.displayName, right.displayName);
            });
            return profiles;
        }

        private static void ValidateProfile(HardwareSupportProfileJson profile, HashSet<string> profileNames, HashSet<int> profileIds)
        {
            string profileName = ValidateProfileName(profile.profile, profile.AssetPath);
            profile.profile = profileName;

            if (!IsValidIdentifier(profileName))
            {
                throw new InvalidOperationException($"Hardware support profile '{profileName}' is not a valid C# identifier: {profile.AssetPath}");
            }

            profile.id = ValidateProfileId(profile.id, profileName, profile.AssetPath);

            if (string.IsNullOrWhiteSpace(profile.displayName))
            {
                throw new InvalidOperationException($"Hardware support definition has no display name: {profile.AssetPath}");
            }

            profile.displayName = profile.displayName.Trim();

            if (profile.sources == null || profile.sources.Length == 0)
            {
                throw new InvalidOperationException($"Hardware support definition has no source: {profile.AssetPath}");
            }

            if (!profileNames.Add(profileName))
            {
                throw new InvalidOperationException($"Duplicate hardware support definition: {profileName}");
            }

            if (!profileIds.Add(profile.id))
            {
                throw new InvalidOperationException($"Duplicate hardware support profile id: {profile.id}");
            }
        }

        private static string ValidateProfileName(string profileName, string assetPath)
        {
            if (string.IsNullOrWhiteSpace(profileName))
            {
                throw new InvalidOperationException($"Hardware support definition has no profile: {assetPath}");
            }

            string trimmed = profileName.Trim();
            if (trimmed == "None")
            {
                throw new InvalidOperationException($"Hardware support definition cannot use the None profile: {assetPath}");
            }

            return trimmed;
        }

        private static int ValidateProfileId(int id, string profileName, string assetPath)
        {
            if (id < 0 || id > MaxId)
            {
                throw new InvalidOperationException($"Hardware support profile '{profileName}' has an invalid id: {id}. Use 0 through {MaxId}. Asset: {assetPath}");
            }

            return id;
        }

        private static string[] ResolveExpressionNames(string[] expressionNames, HardwareSupportProfileJson profile, string statusName)
        {
            List<string> expressions = new();
            foreach (string expressionName in expressionNames ?? Array.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(expressionName))
                {
                    throw new InvalidOperationException($"Hardware support profile '{profile.profile}' {statusName} contains an empty expression name: {profile.AssetPath}");
                }

                if (!Enum.TryParse(expressionName.Trim(), out UnifiedExpression expression) || expression == UnifiedExpression.None)
                {
                    throw new InvalidOperationException($"Unknown hardware support expression '{expressionName}' in profile '{profile.profile}' {statusName}: {profile.AssetPath}");
                }

                string resolvedName = expression.ToString();
                if (!expressions.Contains(resolvedName))
                {
                    expressions.Add(resolvedName);
                }
            }

            return expressions.ToArray();
        }

        private static string GenerateSupportedHardwares(IReadOnlyList<HardwareSupportProfileJson> profiles)
        {
            StringBuilder builder = new();
            builder.AppendLine("// <auto-generated />");
            builder.AppendLine("// Generated by HardwareSupportCodeGenerator. Do not edit manually.");
            builder.AppendLine("using System;");
            builder.AppendLine("using UnityEngine;");
            builder.AppendLine();
            builder.AppendLine("namespace USTL.FaceTracking");
            builder.AppendLine("{");
            builder.AppendLine("    [Flags]");
            builder.AppendLine("    internal enum SupportedHardwares");
            builder.AppendLine("    {");
            builder.AppendLine("        None = 0,");
            foreach (HardwareSupportProfileJson profile in profiles)
            {
                builder.AppendLine($"        [InspectorName({Quote(profile.displayName)})]");
                builder.AppendLine($"        {profile.profile} = 1 << {profile.id},");
            }

            builder.AppendLine("    }");
            builder.AppendLine("}");
            return builder.ToString();
        }

        private static string GenerateProfileDefinitions(IReadOnlyList<HardwareSupportProfileJson> profiles)
        {
            StringBuilder builder = new();
            builder.AppendLine("// <auto-generated />");
            builder.AppendLine("// Generated by HardwareSupportCodeGenerator. Do not edit manually.");
            builder.AppendLine("using System;");
            builder.AppendLine("using System.Collections.Generic;");
            builder.AppendLine("using USTL.FaceTracking;");
            builder.AppendLine();
            builder.AppendLine("namespace USTL.FaceTracking.Editor");
            builder.AppendLine("{");
            builder.AppendLine("    internal sealed partial class HardwareSupportProfileDefinition");
            builder.AppendLine("    {");
            builder.AppendLine("        internal static readonly IReadOnlyDictionary<SupportedHardwares, HardwareSupportProfileDefinition> All = new Dictionary<SupportedHardwares, HardwareSupportProfileDefinition>");
            builder.AppendLine("        {");

            foreach (HardwareSupportProfileJson profile in profiles)
            {
                builder.AppendLine($"            [SupportedHardwares.{profile.profile}] = new HardwareSupportProfileDefinition(");
                builder.AppendLine($"                SupportedHardwares.{profile.profile},");
                builder.AppendLine($"                {Quote(profile.displayName)},");
                builder.AppendLine($"                {profile.id},");
                AppendSourcesArgument(builder, profile.sources, true);
                AppendExpressionArgument(builder, profile.full, true);
                AppendExpressionArgument(builder, profile.converted, true);
                AppendExpressionArgument(builder, profile.unknown, false);
                builder.AppendLine("            ),");
            }

            builder.AppendLine("        };");
            builder.AppendLine("    }");
            builder.AppendLine("}");
            return builder.ToString();
        }

        private static void AppendSourcesArgument(StringBuilder builder, HardwareSupportSourceJson[] sources, bool appendComma)
        {
            if (sources == null || sources.Length == 0)
            {
                builder.AppendLine($"                Array.Empty<HardwareSupportSource>(){(appendComma ? "," : string.Empty)}");
                return;
            }

            builder.AppendLine("                new[]");
            builder.AppendLine("                {");
            foreach (HardwareSupportSourceJson source in sources)
            {
                builder.AppendLine($"                    new HardwareSupportSource({Quote(source.title)}, {Quote(source.url)}, {Quote(source.note)}),");
            }

            builder.AppendLine($"                }}{(appendComma ? "," : string.Empty)}");
        }

        private static void AppendExpressionArgument(StringBuilder builder, string[] expressions, bool appendComma)
        {
            if (expressions == null || expressions.Length == 0)
            {
                builder.AppendLine($"                Array.Empty<UnifiedExpression>(){(appendComma ? "," : string.Empty)}");
                return;
            }

            builder.AppendLine("                new[]");
            builder.AppendLine("                {");
            foreach (string expression in expressions)
            {
                builder.AppendLine($"                    UnifiedExpression.{expression},");
            }

            builder.AppendLine($"                }}{(appendComma ? "," : string.Empty)}");
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

        private static string GetPackageRoot()
        {
            string profileAssetsPath = GetProfileAssetsPath().Replace('\\', '/');
            if (!profileAssetsPath.EndsWith(ProfilesDirectorySuffix, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Hardware support profile directory is not under the expected package path: {profileAssetsPath}");
            }

            return profileAssetsPath[..^ProfilesDirectorySuffix.Length];
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

        private static void WriteIfChanged(string path, string content)
        {
            string normalizedContent = content.Replace("\r\n", "\n");
            if (File.Exists(path))
            {
                string existing = File.ReadAllText(path).Replace("\r\n", "\n");
                if (existing == normalizedContent)
                {
                    return;
                }
            }

            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, normalizedContent, Utf8NoBom);
        }

        private static string Quote(string value)
        {
            if (value == null)
            {
                return "null";
            }

            return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n") + "\"";
        }

        private static bool IsValidIdentifier(string value)
        {
            if (string.IsNullOrEmpty(value) || (!char.IsLetter(value[0]) && value[0] != '_'))
            {
                return false;
            }

            for (int i = 1; i < value.Length; i++)
            {
                if (!char.IsLetterOrDigit(value[i]) && value[i] != '_')
                {
                    return false;
                }
            }

            return true;
        }

        [Serializable]
        private sealed class HardwareSupportProfileJson
        {
            public string profile;
            public string displayName;
            public int id = -1;
            public HardwareSupportSourceJson[] sources;
            public string[] full;
            public string[] converted;
            public string[] unknown;
            [NonSerialized] public string AssetPath;
        }

        [Serializable]
        private sealed class HardwareSupportSourceJson
        {
            public string title;
            public string url;
            public string note;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace USTL.FaceTracking.Editor.Tests
{
    public sealed class LocalizationJsonTests
    {
        private const string LocalizationDirectoryGuid = "70bcbe71ef624f019298c40923e4e04c";
        private const string PackageRoot = "Packages/jp.co.u-stella.facetracking";
        private const string EditorScriptsDirectory = PackageRoot + "/Editor";
        private const string FallbackLocalizationDirectory = PackageRoot + "/Editor/Localization";
        private const string LocalizationKeyPrefix = "ustl.facetracking.editor.";

        private static readonly Regex LiteralLocalizationCallRegex = new(@"(?:FaceTrackingEditorText\.Get|(?<![\w.])T)\s*\(\s*""((?:\\.|[^""\\])*)""", RegexOptions.Compiled);

        [Test]
        public void LocalizationJsonKeys_MatchEditorLocalizationUsage()
        {
            LocalizationKeySet keySet = CollectEditorLocalizationKeys();
            Dictionary<string, Dictionary<string, string>> tables = LoadLocalizationTables();
            HashSet<string> requiredKeys = CollectRequiredKeys(keySet, tables.Values);
            List<string> errors = new();

            foreach (KeyValuePair<string, Dictionary<string, string>> table in tables)
            {
                AddMissingRequiredKeyErrors(errors, table.Key, table.Value.Keys, requiredKeys);
                AddUnusedKeyErrors(errors, table.Key, table.Value.Keys, keySet.AcceptedKeys);
            }

            Assert.That(errors, Is.Empty, string.Join("\n", errors));
        }

        private static LocalizationKeySet CollectEditorLocalizationKeys()
        {
            HashSet<string> requiredKeys = new(StringComparer.Ordinal);
            HashSet<string> acceptedKeys = new(StringComparer.Ordinal);

            AddLiteralLocalizationCallKeys(requiredKeys, acceptedKeys);
            AddFeatureSettingKeys(requiredKeys, acceptedKeys);
            AddParameterSyncModeKeys(requiredKeys, acceptedKeys);

            return new LocalizationKeySet(requiredKeys, acceptedKeys);
        }

        private static void AddLiteralLocalizationCallKeys(HashSet<string> requiredKeys, HashSet<string> acceptedKeys)
        {
            foreach (string path in GetEditorMonoScriptPaths())
            {
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                Assert.That(script, Is.Not.Null, $"Editor script was not found: {path}");

                foreach (Match match in LiteralLocalizationCallRegex.Matches(script.text))
                {
                    AddRequiredKey(requiredKeys, acceptedKeys, DecodeStringLiteral(match.Groups[1].Value));
                }
            }
        }

        private static void AddFeatureSettingKeys(HashSet<string> requiredKeys, HashSet<string> acceptedKeys)
        {
            foreach (FaceTrackingFeatureDefinition featureDefinition in FaceTrackingFeatureDefinition.All.Values)
            {
                AddRequiredKey(requiredKeys, acceptedKeys, $"feature.{featureDefinition.Feature}");

                for (int i = 0; i < featureDefinition.OutputFormats.Count; i++)
                {
                    VRCFTParameterOutputFormat outputFormat = featureDefinition.OutputFormats[i];
                    AddRequiredKey(requiredKeys, acceptedKeys, $"output_format.{featureDefinition.Feature}.{i}");

                    string tooltipKey = ToFullKey($"output_format.{featureDefinition.Feature}.{i}.tooltip");
                    acceptedKeys.Add(tooltipKey);
                    if (!string.IsNullOrWhiteSpace(outputFormat.Description))
                    {
                        requiredKeys.Add(tooltipKey);
                    }
                }
            }
        }

        private static void AddParameterSyncModeKeys(HashSet<string> requiredKeys, HashSet<string> acceptedKeys)
        {
            foreach (ParameterSyncMode syncMode in Enum.GetValues(typeof(ParameterSyncMode)))
            {
                AddRequiredKey(requiredKeys, acceptedKeys, $"sync_mode.{syncMode}");
            }
        }

        private static List<string> GetEditorMonoScriptPaths()
        {
            List<string> paths = new();
            foreach (string guid in AssetDatabase.FindAssets("t:MonoScript", new[] { EditorScriptsDirectory, }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.StartsWith(EditorScriptsDirectory, StringComparison.Ordinal) && path.EndsWith(".cs", StringComparison.Ordinal))
                {
                    paths.Add(path);
                }
            }

            paths.Sort(StringComparer.Ordinal);
            return paths;
        }

        private static List<string> GetLocalizationJsonPaths()
        {
            string localizationDirectory = AssetDatabase.GUIDToAssetPath(LocalizationDirectoryGuid);
            if (string.IsNullOrEmpty(localizationDirectory))
            {
                localizationDirectory = FallbackLocalizationDirectory;
            }

            List<string> paths = new();
            foreach (string guid in AssetDatabase.FindAssets("t:TextAsset", new[] { localizationDirectory, }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith(".json", StringComparison.Ordinal))
                {
                    paths.Add(path);
                }
            }

            paths.Sort(StringComparer.Ordinal);
            Assert.That(paths, Is.Not.Empty, $"Localization JSON directory was not found or empty: {localizationDirectory}");
            return paths;
        }

        private static Dictionary<string, Dictionary<string, string>> LoadLocalizationTables()
        {
            Dictionary<string, Dictionary<string, string>> tables = new(StringComparer.Ordinal);
            foreach (string path in GetLocalizationJsonPaths())
            {
                tables.Add(path, LoadLocalizationTable(path));
            }

            return tables;
        }

        private static Dictionary<string, string> LoadLocalizationTable(string path)
        {
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            Assert.That(asset, Is.Not.Null, $"Localization JSON was not found: {path}");

            JObject json = JObject.Load(new JsonTextReader(new StringReader(asset.text)), new JsonLoadSettings
            {
                DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error,
            });

            Dictionary<string, string> table = new(StringComparer.Ordinal);
            foreach (JProperty property in json.Properties())
            {
                Assert.That(property.Value.Type, Is.EqualTo(JTokenType.String), $"{path}: '{property.Name}' must be a string value.");
                table.Add(property.Name, property.Value.Value<string>());
            }

            return table;
        }

        private static HashSet<string> CollectRequiredKeys(LocalizationKeySet keySet, IEnumerable<Dictionary<string, string>> tables)
        {
            HashSet<string> requiredKeys = new(keySet.RequiredKeys, StringComparer.Ordinal);
            foreach (Dictionary<string, string> table in tables)
            {
                foreach (string key in table.Keys)
                {
                    if (keySet.AcceptedKeys.Contains(key))
                    {
                        requiredKeys.Add(key);
                    }
                }
            }

            return requiredKeys;
        }

        private static void AddMissingRequiredKeyErrors(List<string> errors, string path, IEnumerable<string> actualKeys, HashSet<string> requiredKeys)
        {
            HashSet<string> actualKeySet = new(actualKeys, StringComparer.Ordinal);
            foreach (string requiredKey in requiredKeys)
            {
                if (!actualKeySet.Contains(requiredKey))
                {
                    errors.Add($"{path}: missing localization key '{requiredKey}'.");
                }
            }
        }

        private static void AddUnusedKeyErrors(List<string> errors, string path, IEnumerable<string> actualKeys, HashSet<string> acceptedKeys)
        {
            foreach (string actualKey in actualKeys)
            {
                if (!acceptedKeys.Contains(actualKey))
                {
                    errors.Add($"{path}: contains unused localization key '{actualKey}'.");
                }
            }
        }

        private static void AddRequiredKey(HashSet<string> requiredKeys, HashSet<string> acceptedKeys, string shortKey)
        {
            string key = ToFullKey(shortKey);
            requiredKeys.Add(key);
            acceptedKeys.Add(key);
        }

        private static string ToFullKey(string shortKey)
        {
            return shortKey.StartsWith(LocalizationKeyPrefix, StringComparison.Ordinal) ? shortKey : LocalizationKeyPrefix + shortKey;
        }

        private static string DecodeStringLiteral(string value)
        {
            return value.Replace("\\\"", "\"").Replace("\\\\", "\\");
        }

        private readonly struct LocalizationKeySet
        {
            internal LocalizationKeySet(HashSet<string> requiredKeys, HashSet<string> acceptedKeys)
            {
                RequiredKeys = requiredKeys;
                AcceptedKeys = acceptedKeys;
            }

            internal HashSet<string> RequiredKeys { get; }
            internal HashSet<string> AcceptedKeys { get; }
        }
    }
}

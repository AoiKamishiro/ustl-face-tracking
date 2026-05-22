using System;
using System.Collections.Generic;
using System.IO;
using nadena.dev.ndmf.localization;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace USTL.FaceTracking.Editor
{
    [InitializeOnLoad]
    internal static class LocalizationUtility
    {
        private const string LocalizationDirectoryGuid = "70bcbe71ef624f019298c40923e4e04c";

        private const string FallbackLocalizationDirectory = "Packages/jp.co.u-stella.facetracking/Editor/Localization";

        private static readonly string[] SupportedLanguages =
        {
            "en-US",
            "ja-JP",
        };

        static LocalizationUtility()
        {
            L = new Localizer("en-US", LoadLocalizations);
        }

        private static Localizer L { get; }

        private static string LocalizationDirectory
        {
            get
            {
                string path = AssetDatabase.GUIDToAssetPath(LocalizationDirectoryGuid);
                return string.IsNullOrEmpty(path) ? FallbackLocalizationDirectory : path;
            }
        }

        internal static void Localize(VisualElement root)
        {
            L.LocalizeUIElements(root);
        }

        internal static string S(string key, string fallback)
        {
            return L.TryGetLocalizedString(key, out string value) ? value : fallback;
        }

        private static List<(string, Func<string, string>)> LoadLocalizations()
        {
            List<(string, Func<string, string>)> languages = new();
            foreach (string language in SupportedLanguages)
            {
                languages.Add((language, LoadLanguage(language)));
            }

            return languages;
        }

        private static Func<string, string> LoadLanguage(string language)
        {
            string filename = Path.Combine(LocalizationDirectory, $"{language}.json");

            try
            {
                string json = File.ReadAllText(filename);
                Dictionary<string, string> table = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                return key => table != null && table.TryGetValue(key, out string value) ? value : null;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load localization file {filename}");
                Debug.LogException(e);
                return _ => null;
            }
        }
    }
}

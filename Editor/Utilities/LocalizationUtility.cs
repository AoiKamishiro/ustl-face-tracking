using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        internal const string TranslationClassName = "ustl-tr";
        internal const string ColumnTranslationClassName = "ustl-tr-columns";

        private const string LocalizationDirectoryGuid = "70bcbe71ef624f019298c40923e4e04c";

        private const string FallbackLocalizationDirectory = "Packages/jp.co.u-stella.facetracking/Editor/Localization";

        private const string EditorLocalizationKeyPrefix = "ustl.facetracking.editor.";

        private static readonly string[] SupportedLanguages =
        {
            "en-US",
            "ja-JP",
        };

        private static readonly Dictionary<Type, PropertyInfo> LocalizedTextProperties = new();
        private static readonly ConditionalWeakTable<VisualElement, LocalizedElementState> LocalizedElementStates = new();
        private static readonly ConditionalWeakTable<MultiColumnListView, LocalizedColumnsState> LocalizedColumnStates = new();

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
            WalkTree(root);
            LanguagePrefs.ApplyFontPreferences(root);
        }

        internal static string S(string key, string fallback)
        {
            return L.TryGetLocalizedString(key, out string value) ? value : fallback;
        }

        internal static string EditorKey(string key)
        {
            return key.StartsWith(EditorLocalizationKeyPrefix, StringComparison.Ordinal) ? key : EditorLocalizationKeyPrefix + key;
        }

        private static void WalkTree(VisualElement element)
        {
            if (element.ClassListContains(TranslationClassName))
            {
                LocalizeElement(element);
            }

            if (element is MultiColumnListView listView && element.ClassListContains(ColumnTranslationClassName))
            {
                LocalizeColumns(listView);
            }

            foreach (VisualElement child in element.Children())
            {
                WalkTree(child);
            }
        }

        private static void LocalizeElement(VisualElement element)
        {
            if (!LocalizedElementStates.TryGetValue(element, out LocalizedElementState state))
            {
                state = CreateState(element);
                if (state == null)
                {
                    return;
                }

                LocalizedElementStates.Add(element, state);
                LanguagePrefs.RegisterLanguageChangeCallback(element, _ => state.Apply());
            }

            state.Apply();
        }

        private static void LocalizeColumns(MultiColumnListView listView)
        {
            if (!LocalizedColumnStates.TryGetValue(listView, out LocalizedColumnsState state))
            {
                state = CreateColumnsState(listView);
                if (state == null)
                {
                    return;
                }

                LocalizedColumnStates.Add(listView, state);
                LanguagePrefs.RegisterLanguageChangeCallback(listView, _ => state.Apply());
            }

            state.Apply();
        }

        private static LocalizedElementState CreateState(VisualElement element)
        {
            PropertyInfo textProperty = GetLocalizedTextProperty(element.GetType());
            string key = textProperty?.GetValue(element) as string;
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            string tooltipKey = string.IsNullOrEmpty(element.tooltip) ? key + ".tooltip" : element.tooltip;
            return new LocalizedElementState(element, textProperty, key, tooltipKey);
        }

        private static LocalizedColumnsState CreateColumnsState(MultiColumnListView listView)
        {
            List<LocalizedColumn> columns = new();
            foreach (Column column in listView.columns)
            {
                if (!string.IsNullOrEmpty(column.title))
                {
                    columns.Add(new LocalizedColumn(column, column.title));
                }
            }

            return columns.Count > 0 ? new LocalizedColumnsState(listView, columns) : null;
        }

        private static PropertyInfo GetLocalizedTextProperty(Type type)
        {
            if (LocalizedTextProperties.TryGetValue(type, out PropertyInfo property))
            {
                return property;
            }

            property = GetWritableStringProperty(type, "text") ?? GetWritableStringProperty(type, "label");
            LocalizedTextProperties[type] = property;
            return property;
        }

        private static PropertyInfo GetWritableStringProperty(Type type, string propertyName)
        {
            PropertyInfo property = type.GetProperty(propertyName);
            return property?.PropertyType == typeof(string) && property.GetMethod != null && property.SetMethod != null ? property : null;
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

        private sealed class LocalizedElementState
        {
            private readonly VisualElement _element;
            private readonly string _key;
            private readonly PropertyInfo _textProperty;
            private readonly string _tooltipKey;

            public LocalizedElementState(VisualElement element, PropertyInfo textProperty, string key, string tooltipKey)
            {
                _element = element;
                _textProperty = textProperty;
                _key = key;
                _tooltipKey = tooltipKey;
            }

            public void Apply()
            {
                _textProperty.SetValue(_element, L.GetLocalizedString(_key));
                _element.tooltip = !string.IsNullOrEmpty(_tooltipKey) && L.TryGetLocalizedString(_tooltipKey, out string tooltip) ? tooltip : null;
            }
        }

        private readonly struct LocalizedColumn
        {
            public LocalizedColumn(Column column, string key)
            {
                Column = column;
                Key = key;
            }

            public Column Column { get; }
            public string Key { get; }
        }

        private sealed class LocalizedColumnsState
        {
            private readonly List<LocalizedColumn> _columns;
            private readonly MultiColumnListView _listView;

            public LocalizedColumnsState(MultiColumnListView listView, List<LocalizedColumn> columns)
            {
                _listView = listView;
                _columns = columns;
            }

            public void Apply()
            {
                foreach (LocalizedColumn column in _columns)
                {
                    column.Column.title = L.GetLocalizedString(column.Key);
                }

                _listView.Rebuild();
            }
        }
    }
}

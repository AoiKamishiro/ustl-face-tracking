using System;
using System.Collections.Generic;
using nadena.dev.ndmf.localization;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace USTL.Core.Editor
{
    public static class USTLLocalizer
    {
        public const string TrClassName = "ustl-tr";
        public const string TrClassNamePrefix = "ustl-tr_";
        public const string TooltipPostfix = "__tooltip";

        private static readonly Dictionary<string, List<Dictionary<string, string>>> LangDicts = new()
        {
            ["en-us"] = new List<Dictionary<string, string>>(),
            ["ja-jp"] = new List<Dictionary<string, string>>(),
        };

        private static readonly List<IElementLocalizer> Localizers = new();

        private static List<Dictionary<string, string>> CurrentLocalizationAsset
        {
            get
            {
                string lang = LanguagePrefs.Language.ToLower();
                if (LangDicts.TryGetValue(lang, out List<Dictionary<string, string>> dicts))
                {
                    return dicts;
                }

                return LangDicts["en-us"];
            }
        }

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            foreach (Type loc in TypeCache.GetTypesDerivedFrom<IPackageLocalizationSetting>())
            {
                Register((IPackageLocalizationSetting)Activator.CreateInstance(loc));
            }

            foreach (Type loc in TypeCache.GetTypesDerivedFrom<IElementLocalizer>())
            {
                Register((IElementLocalizer)Activator.CreateInstance(loc));
            }
        }

        private static void Register(IPackageLocalizationSetting localizationSetting)
        {
            string path = AssetDatabase.GUIDToAssetPath(localizationSetting.LocalizationDirectoryGuid);
            TextAsset enUs = AssetDatabase.LoadAssetAtPath<TextAsset>($"{path}/en-US.json");
            TextAsset jaJp = AssetDatabase.LoadAssetAtPath<TextAsset>($"{path}/ja-JP.json");
            if (enUs)
            {
                LangDicts["en-us"].Add(JsonConvert.DeserializeObject<Dictionary<string, string>>(enUs.text));
            }

            if (jaJp)
            {
                LangDicts["ja-jp"].Add(JsonConvert.DeserializeObject<Dictionary<string, string>>(jaJp.text));
            }
        }

        internal static void Register(IElementLocalizer localizer)
        {
            bool contains = false;
            foreach (IElementLocalizer loc in Localizers)
            {
                if (loc.GetType() == localizer.GetType())
                {
                    contains = true;
                    break;
                }
            }

            if (!contains)
            {
                Localizers.Add(localizer);
            }
        }

        public static void Localize(VisualElement root)
        {
            root.Query(className: TrClassName).ForEach(element =>
            {
                string key = GetLocalizeKeyClass(element);
                foreach (IElementLocalizer loc in Localizers)
                {
                    bool localized = loc.Localize(element, key);
                    if (localized)
                    {
                        break;
                    }
                }
            });
        }

        public static string GetLocalizedString(string key)
        {
            foreach (Dictionary<string, string> dict in CurrentLocalizationAsset)
            {
                if (dict.TryGetValue(key, out string value))
                {
                    return value;
                }
            }

            return string.Empty;
        }

        public static void RemoveLocalizeClass(VisualElement root)
        {
            root.RemoveFromClassList(TrClassName);
            IEnumerable<string> currentEnumerable = root.GetClasses();
            List<string> current = new(2);
            foreach (string @class in currentEnumerable)
            {
                if (@class.StartsWith(TrClassNamePrefix))
                {
                    current.Add(@class);
                }
            }

            foreach (string @class in current)
            {
                root.RemoveFromClassList(@class);
            }
        }

        public static void AddLocalizeClass(VisualElement root, string @class = "")
        {
            root.AddToClassList(TrClassName);
            root.AddToClassList(@class);
        }

        public static string GetLocalizeKeyClass(VisualElement element)
        {
            foreach (string @class in element.GetClasses())
            {
                if (@class.StartsWith(TrClassNamePrefix))
                {
                    return @class;
                }
            }

            return string.Empty;
        }
    }
}

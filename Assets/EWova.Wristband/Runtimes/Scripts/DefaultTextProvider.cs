using EWova.Localization;

using System;
using System.Collections.Generic;
using System.Globalization;

using UnityEngine;

namespace EWova.Wristband
{
    public enum LocalizationLang
    {
        auto, en, zh_Hant, zh, ja, ko, vi
    }

    public class DefaultTextProvider : ITextProvider
    {
        private Dictionary<LocalizationLang, Dictionary<string, string>> _cache =
            new Dictionary<LocalizationLang, Dictionary<string, string>>();

        private DefaultTextProvider() { }

        public LocalizationLang CurrentSetting { get; set; } = LocalizationLang.auto;

        private static string GetCode(LocalizationLang lang)
        {
            return lang switch
            {
                LocalizationLang.en => "en",
                LocalizationLang.zh => "zh",
                LocalizationLang.zh_Hant => "zh-Hant",
                LocalizationLang.ja => "ja",
                LocalizationLang.ko => "ko",
                LocalizationLang.vi => "vi",
                _ => null
            };
        }

        public string GetLocalizedString(string key)
        {
            return GetLocalizedString(key, CurrentSetting);
        }

        public static DefaultTextProvider LoadFromFile(string filePath)
        {
            var loadObj = Resources.Load<TextAsset>(filePath);

            if (loadObj == null)
            {
                Logger.Err($"Localization file not found at path: {filePath}");
                return null;
            }

            string content = loadObj.text;

            if (string.IsNullOrEmpty(content))
            {
                Logger.Err($"Failed to load localization file at path: {filePath}");
                return null;
            }
            DefaultTextProvider provider = new DefaultTextProvider();
            provider.LoadFromTsv(content);
            return provider;
        }

        private void LoadFromTsv(string tsvContent)
        {
            _cache.Clear();
            string[] lines = tsvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0) return;

            string[] headers = lines[0].Split('\t');
            Dictionary<int, LocalizationLang> columnMap = new Dictionary<int, LocalizationLang>();

            foreach (LocalizationLang lang in Enum.GetValues(typeof(LocalizationLang)))
            {
                if (lang == LocalizationLang.auto) continue;
                _cache[lang] = new Dictionary<string, string>();
            }

            for (int i = 1; i < headers.Length; i++)
            {
                string header = headers[i].Trim();
                foreach (LocalizationLang lang in Enum.GetValues(typeof(LocalizationLang)))
                {
                    if (lang == LocalizationLang.auto) continue;
                    if (header.Equals(GetCode(lang), StringComparison.OrdinalIgnoreCase))
                    {
                        columnMap[i] = lang;
                        break;
                    }
                }
            }

            for (int i = 1; i < lines.Length; i++)
            {
                string[] cells = lines[i].Split('\t');
                if (cells.Length == 0) continue;

                string key = cells[0].Trim();
                if (string.IsNullOrEmpty(key)) continue;

                foreach (var entry in columnMap)
                {
                    int colIdx = entry.Key;
                    LocalizationLang lang = entry.Value;

                    if (cells.Length > colIdx)
                    {
                        string value = cells[colIdx].Trim().Replace("\\n", "\n");
                        _cache[lang][key] = value;
                    }
                }
            }
        }

        public string GetLocalizedString(string key, LocalizationLang targetLang = LocalizationLang.auto)
        {
            if (targetLang == LocalizationLang.auto)
            {
                targetLang = GetSystemLanguage();
            }

            if (_cache.TryGetValue(targetLang, out var langDict))
            {
                if (langDict.TryGetValue(key, out string value))
                    return value;
            }

            if (targetLang != LocalizationLang.en && _cache.ContainsKey(LocalizationLang.en))
            {
                if (_cache[LocalizationLang.en].TryGetValue(key, out string enValue))
                    return enValue;
            }

            return $"[{key}]";
        }

        private LocalizationLang GetSystemLanguage()
        {
            CultureInfo currentCulture = CultureInfo.CurrentUICulture;
            var output = currentCulture.TwoLetterISOLanguageName switch
            {
                "zh" => currentCulture.Name.Contains("Hant", StringComparison.OrdinalIgnoreCase)
                || currentCulture.Name.Contains("TW", StringComparison.OrdinalIgnoreCase)
                || currentCulture.Name.Contains("HK", StringComparison.OrdinalIgnoreCase)
                    ? LocalizationLang.zh_Hant
                    : LocalizationLang.zh,
                "ja" => LocalizationLang.ja,
                "ko" => LocalizationLang.ko,
                "vi" => LocalizationLang.vi,
                _ => LocalizationLang.en
            };
            Logger.Info($"System language detected: {currentCulture.Name}, using localization: {output}");
            return output;
        }

    }
}

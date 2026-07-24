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
        private readonly Dictionary<LocalizationLang, Dictionary<string, string>> _cache = new();
        private LocalizationLang _currentSetting = LocalizationLang.auto;

        private DefaultTextProvider() { }

        public LocalizationLang CurrentSetting
        {
            get => _currentSetting;
            set
            {
                if (value == LocalizationLang.auto)
                    value = GetSystemLanguage();

                if (value != _currentSetting)
                {
                    _currentSetting = value;
                    OnLanguageChanged?.Invoke(this);
                }
            }
        }
        public event Action<ITextProvider> OnLanguageChanged;

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
            return GetLocalizedStringInternal(key, CurrentSetting);
        }

        public string GetLocalizedString(string key, LocalizationLang targetLang)
        {
            if (targetLang == LocalizationLang.auto)
                targetLang = GetSystemLanguage();

            return GetLocalizedStringInternal(key, targetLang);
        }

        private string GetLocalizedStringInternal(string key, LocalizationLang targetLang)
        {
            if (_cache.TryGetValue(targetLang, out var langDict))
            {
                if (langDict.TryGetValue(key, out string value))
                    return value;
            }

            // Fallback to English if the target language is not found
            if (targetLang != LocalizationLang.en && _cache.ContainsKey(LocalizationLang.en))
            {
                if (_cache[LocalizationLang.en].TryGetValue(key, out string enValue))
                    return enValue;
            }

            // If the key is not found in any language, return the key itself wrapped in brackets
            return $"[{key}]";
        }

        public static DefaultTextProvider LoadFromFile(string filePath)
        {
            var loadObj = Resources.Load<TextAsset>(filePath);

            if (loadObj == null)
            {
                if (Logger.ErrorEnabled)
                    Logger.Err($"Localization file not found at path: {filePath}");
                return null;
            }

            string content = loadObj.text;

            if (string.IsNullOrEmpty(content))
            {
                if (Logger.ErrorEnabled)
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
            if (Logger.InfoEnabled)
                Logger.Info($"System language detected: {currentCulture.Name}, using localization: {output}");
            return output;
        }

    }
}

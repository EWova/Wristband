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
        private readonly Dictionary<string, Dictionary<string, string>> _cache = new(StringComparer.OrdinalIgnoreCase);
        private LocalizationLang _currentSetting = LocalizationLang.auto;
        private string _currentLanguageCode = "en";

        private DefaultTextProvider() { }

        public LocalizationLang CurrentSetting
        {
            get => _currentSetting;
            set
            {
                if (value == LocalizationLang.auto)
                    value = GetSystemLanguage();

                _currentSetting = value;
                SetLanguageCodeInternal(GetCode(value) ?? "en");
            }
        }

        /// <summary>目前實際查表使用的語系代碼（含透過 <see cref="SetLanguageByCode"/> 設定、不在 <see cref="LocalizationLang"/> 裡的自訂語系）。</summary>
        public string CurrentLanguageCode => _currentLanguageCode;

        /// <summary>
        /// 供第三方在不修改此 package 的情況下切換到 <see cref="LocalizationLang"/> 未收錄的語系。
        /// 搭配 <see cref="MergeTsv"/> 匯入的自訂語系欄位使用，例如 SetLanguageByCode("th")。
        /// </summary>
        public void SetLanguageByCode(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode))
                return;
            SetLanguageCodeInternal(languageCode);
        }

        private void SetLanguageCodeInternal(string code)
        {
            if (!string.Equals(_currentLanguageCode, code, StringComparison.OrdinalIgnoreCase))
            {
                _currentLanguageCode = code;
                OnLanguageChanged?.Invoke(this);
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
            return GetLocalizedStringInternal(key, _currentLanguageCode);
        }

        public string GetLocalizedString(string key, LocalizationLang targetLang)
        {
            if (targetLang == LocalizationLang.auto)
                targetLang = GetSystemLanguage();

            return GetLocalizedStringInternal(key, GetCode(targetLang) ?? "en");
        }

        /// <summary>供第三方以任意語系代碼（不需要在 <see cref="LocalizationLang"/> 裡）查表。</summary>
        public string GetLocalizedString(string key, string languageCode)
        {
            return GetLocalizedStringInternal(key, languageCode);
        }

        private string GetLocalizedStringInternal(string key, string languageCode)
        {
            if (_cache.TryGetValue(languageCode, out var langDict))
            {
                if (langDict.TryGetValue(key, out string value))
                    return value;
            }

            // Fallback to English if the target language is not found
            if (!string.Equals(languageCode, "en", StringComparison.OrdinalIgnoreCase) && _cache.TryGetValue("en", out var enDict))
            {
                if (enDict.TryGetValue(key, out string enValue))
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
            provider.LoadFromTsv(content, clear: true);
            return provider;
        }

        /// <summary>
        /// 供第三方在不修改此 package 的情況下，把額外的翻譯（可包含全新語系欄位，例如 "th"）合併進現有資料。
        /// 不會清掉既有翻譯，同 key/語系會被覆蓋。搭配 <see cref="SetLanguageByCode"/> 切換到新語系。
        /// </summary>
        public void MergeTsv(string tsvContent)
        {
            LoadFromTsv(tsvContent, clear: false);
        }

        private void LoadFromTsv(string tsvContent, bool clear)
        {
            if (clear)
                _cache.Clear();

            string[] lines = tsvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0) return;

            string[] headers = lines[0].Split('\t');
            Dictionary<int, string> columnMap = new Dictionary<int, string>();

            // 任何 header 文字都視為一個語系代碼，不限於 LocalizationLang 已知的欄位，
            // 這樣第三方就能透過額外的 TSV 新增此 package 完全不認得的語系。
            for (int i = 1; i < headers.Length; i++)
            {
                string code = headers[i].Trim();
                if (!string.IsNullOrEmpty(code))
                    columnMap[i] = code;
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
                    string code = entry.Value;

                    if (cells.Length > colIdx)
                    {
                        if (!_cache.TryGetValue(code, out var langDict))
                            _cache[code] = langDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                        string value = cells[colIdx].Trim().Replace("\\n", "\n");
                        langDict[key] = value;
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

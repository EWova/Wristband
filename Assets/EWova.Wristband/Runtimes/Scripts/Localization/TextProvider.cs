using System.Collections.Generic;

using UnityEngine;

namespace EWova.Localization
{
    public static class TextProvider
    {
        static TextProvider()
        {
            Providers = new List<ITextProvider>();
        }

        public readonly static List<ITextProvider> Providers;
        public static string GetLocalizedString(string key)
        {
            if (Providers == null || Providers.Count == 0)
            {
                return $"[{key}]";
            }

            foreach (var provider in Providers)
            {
                string value = provider.GetLocalizedString(key);
                if (!string.IsNullOrEmpty(value) && !value.StartsWith("["))
                {
                    return value;
                }
            }
            return $"[{key}]";
        }
    }
}

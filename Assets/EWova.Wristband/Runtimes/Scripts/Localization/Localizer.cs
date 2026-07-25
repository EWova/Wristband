using UnityEngine;

namespace EWova.Localization
{
    public class Localizer : MonoBehaviour
    {
        public void DoLocalizeUpdate(ITextProvider TextProvider)
        {
            if (TextProvider == null)
            {
                UnityEngine.Debug.LogWarning("TextProvider is not set. Cannot update localization.");
                return;
            }

            ILocalizeUpdater[] localizeUpdater = GetComponentsInChildren<ILocalizeUpdater>(true);
            if (localizeUpdater == null || localizeUpdater.Length == 0)
                return;

            foreach (var updater in localizeUpdater)
            {
                string value = TextProvider.GetLocalizedString(updater.Key);
                updater.OnLocalizeUpdated(value);
            }
        }
    }
}

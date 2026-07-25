using UnityEngine;
namespace EWova.Localization
{
    public interface ITextProvider
    {
        string GetLocalizedString(string key);
    }
}

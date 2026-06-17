namespace EWova.Localization
{
    public interface ILocalizeUpdater
    {
        string Key { get; }
        void OnLocalizeUpdated(string value);
    }
}

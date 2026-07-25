using UnityEngine;
using UnityEngine.Events;

namespace EWova.Localization
{
    public class LocalizeStringEvent : MonoBehaviour, ILocalizeUpdater
    {
        [SerializeField] private string _key;
        public string Key => _key;

        public UnityEvent<string> OnUpdate = new UnityEvent<string>();

        public void OnLocalizeUpdated(string value)
        {
            OnUpdate.Invoke(value);
        }

        public void AddListener(UnityAction<string> call)
        {
            OnUpdate.AddListener(call);
        }

        public void RemoveListener(UnityAction<string> call)
        {
            OnUpdate.RemoveListener(call);
        }
    }
}

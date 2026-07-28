using System;

using UnityEngine;
using UnityEngine.Events;

namespace EWova.Localization
{
    // IL2CPP 需要具體（非泛型）子類別才能可靠產生 UnityEvent<string> 的 AOT 呼叫/序列化程式碼，
    // 直接用 UnityEvent<string> 欄位在部分 IL2CPP 平台上可能無法觸發 Inspector 掛的 persistent listener。
    [Serializable]
    public class StringUnityEvent : UnityEvent<string> { }

    public class LocalizeStringEvent : MonoBehaviour, ILocalizeUpdater
    {
        [SerializeField] private string _key;
        public string Key => _key;

        public StringUnityEvent OnUpdate = new StringUnityEvent();

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

using Cysharp.Threading.Tasks;

using System;

using UnityEngine;

namespace EWova.Wristband
{
    public abstract class BaseBTN : MonoBehaviour
    {
        public virtual string LabelKey => "{Label}";
        public virtual string FeatureKey => null;

        [SerializeField] private CircleButtonElement _circleButtonElement;
        private Wristband _wristband;

        protected CircleButtonElement CircleButtonElement => _circleButtonElement;
        [SerializeField] protected Wristband WristbandController => _wristband;
        protected WApiClient ApiClient => _wristband != null ? _wristband.ApiClient : null;

        protected bool BaseShow { get; set; } = true;
        protected bool BaseEnabled { get; set; } = true;
        protected string BaseDisabledReasonKey { get; set; } = null;

        private void OnValidate()
        {
            if (_circleButtonElement == null)
                _circleButtonElement = GetComponent<CircleButtonElement>();
            if (_wristband == null)
                _wristband = GetComponentInParent<Wristband>();
        }

        private void Awake()
        {
            if (_circleButtonElement == null)
                _circleButtonElement = GetComponent<CircleButtonElement>();
            if (_wristband == null)
                _wristband = GetComponentInParent<Wristband>();
            _circleButtonElement.OnClick += ProcessClickInternal;
            _wristband.OnButtonInvoke += SyncState;
        }
        private void Start()
        {
            UniTask.Void(async () =>
            {
                _loadProcess = new();
                try
                {
                    await Load(_loadProcess);
                    SyncState();
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                    _loadProcess.SetFailed();
                }
            });
        }
        private Action _release;
        private void OnDestroy()
        {
            if (_circleButtonElement != null)
                _circleButtonElement.OnClick -= ProcessClickInternal;
            if (_wristband != null)
                _wristband.OnButtonInvoke -= SyncState;

            _release?.Invoke();
            _release = null;
        }
        private LoadProcess _loadProcess;
        protected virtual void Update()
        {
            if (_loadProcess != null)
            {
                _circleButtonElement.Progress = _loadProcess.Progress;
                _circleButtonElement.Button.interactable = !_loadProcess.Failed;

                if (_loadProcess.IsCompleted)
                    _loadProcess = null;
            }
        }

        protected class LoadProcess
        {
            public float Progress;
            public bool Failed;
            public bool IsCompleted;
            public void SetComplete()
            {
                Progress = 1f;
                IsCompleted = true;
            }
            public void SetFailed()
            {
                Failed = true;
                IsCompleted = true;
            }
        }

        protected virtual UniTask Load(LoadProcess loadProcess)
        {
            if (!string.IsNullOrEmpty(LabelKey))
                _circleButtonElement.LabelTMP.text = GetLocalizedString(LabelKey);

            WristbandController.LocalizeTextProvider.OnLanguageChanged += OnLanguageChanged;
            _release += () => WristbandController.LocalizeTextProvider.OnLanguageChanged -= OnLanguageChanged;

            return UniTask.CompletedTask;
        }
        protected abstract UniTask ProcessClick();

        protected virtual void UpdateBaseState() { }
        protected virtual void OnLanguageChanged(Localization.ITextProvider textProvider)
        {
            if (!string.IsNullOrEmpty(LabelKey))
                _circleButtonElement.LabelTMP.text = GetLocalizedString(LabelKey);
        }
        protected string GetLocalizedString(string key)
        {
            return WristbandController.LocalizeTextProvider.GetLocalizedString(key);
        }

        protected void SyncState()
        {
            UpdateBaseState();

            bool featureVisible = true;
            bool featureEnabled = true;
            string featureReason = null;

            if (FeatureKey != null)
            {
                if (WristbandController.TryGetFeature(FeatureKey, out var feature))
                {
                    featureVisible = feature.visible;
                    featureEnabled = feature.enabled;
                    featureReason = feature.disabledReason;
                }
                else
                {
                    featureVisible = false;
                }
            }

            CircleButtonElement.Show = BaseShow && featureVisible;
            CircleButtonElement.IsFeatureEnabled = BaseEnabled && featureEnabled;
            CircleButtonElement.DisabledReasonKey = featureReason ?? BaseDisabledReasonKey;
        }

        protected void ResyncAllBTNState()
        {
            WristbandController.OnButtonInvoke.Invoke();
        }

#if UNITY_6000_0_OR_NEWER
        [HideInCallstack]
#endif
        internal void ProcessClickInternal()
        {
            UniTask.Void(async () =>
            {
                try
                {
                    if (Logger.InfoEnabled)
                        Logger.Info($"{LabelKey} button clicked.");
                    await ProcessClick();
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                }
                finally
                {
                    _circleButtonElement.Progress = 1f;
                }
            });
        }
    }
}

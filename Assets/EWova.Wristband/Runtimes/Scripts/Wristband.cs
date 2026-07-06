using EWova.Localization;

using System;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EWova.Wristband
{
    public class Wristband : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Serializable]
        class FeatureGroup
        {
            public string Flag;
            public CircleButtonElement Element;
        }

        public Button MainMenuBTN;
        public CanvasGroup ChildMenuCanvasGroup;
        public Animator Animator;
        public Localizer Localizer;
        [SerializeField] private FeatureGroup[] _featureGroups;
        public LocalizationLang LocalizationLang = LocalizationLang.auto;

        public RectTransform LearningPortfolioFrame;

        private bool _isMenuOpen = false;
        private float _openingIdleTime = 0f;
        private float _animT = 0;
        private bool _isUIHovering = false;
        private static readonly int OpeningHash = Animator.StringToHash("Opening");
        private float _softAnimT = 0f;
        private LocalizationLang _currentLang = LocalizationLang.auto;
        internal Action OnButtonInvoke;

        public WApiClient ApiClient { get; private set; }
        public string LastScreenshotUrl { get; set; }

        public DefaultTextProvider LocalizeTextProvider { get; private set; }

        public void LoadFeatures(string flags)
        {
            var keys = flags.Split(',');
            var states = new ApiModels.FeatureState[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                states[i] = new ApiModels.FeatureState
                {
                    key = keys[i].Trim(),
                    visible = true,
                    enabled = true
                };
            }
            LoadFeatures(states);
        }

        public void LoadFeatures(ApiModels.FeatureState[] features)
        {
            foreach (var group in _featureGroups)
                group.Element.gameObject.SetActive(false);

            foreach (var state in features)
            {
                if (string.IsNullOrEmpty(state.key)) continue;

                if (!WristbandCapabilities.Supported.Contains(state.key))
                {
                    if (Logger.InfoEnabled)
                        Logger.Info($"Feature '{state.key}' skipped: not supported by this installation.");
                    continue;
                }

                var group = Array.Find(_featureGroups,
                    g => string.Equals(g.Flag, state.key, StringComparison.InvariantCultureIgnoreCase));

                if (group == null)
                {
                    if (Logger.WarnEnabled)
                        Logger.Warn($"Feature key '{state.key}' not found in FeatureGroups.");
                    continue;
                }

                group.Element.gameObject.SetActive(state.visible);

                if (state.visible)
                {
                    group.Element.IsFeatureEnabled = state.enabled;
                    group.Element.DisabledReasonKey = state.disabledReason;
                    if (Logger.InfoEnabled)
                        Logger.Info($"Feature '{state.key}': visible={state.visible}, enabled={state.enabled}, reason='{state.disabledReason}'");
                }
            }
        }

        private void Awake()
        {
            ApiClient = new WApiClient();
            InitializeLocalization();

            foreach (var group in _featureGroups)
            {
                group.Element.gameObject.SetActive(false);
            }

            MainMenuBTN.onClick.AddListener(() =>
            {
                _isMenuOpen = !_isMenuOpen;
                OnButtonInvoke?.Invoke();
                if (Logger.InfoEnabled)
                    Logger.Info($"Main menu button clicked. Menu is now {(_isMenuOpen ? "open" : "closed")}.");
            });
        }

        private void InitializeLocalization()
        {
            try
            {
                LocalizeTextProvider = DefaultTextProvider.LoadFromFile("Localization/Wristband");
                if (Logger.InfoEnabled)
                    Logger.Info("Localization file loaded successfully.");
                Localizer.DoLocalizeUpdate(LocalizeTextProvider);
            }
            catch (Exception ex)
            {
                if (Logger.ErrorEnabled)
                    Logger.Err($"Failed to load localization:");
                UnityEngine.Debug.LogException(ex);
            }
        }

        private void Update()
        {
            if (_isUIHovering)
            {
                _openingIdleTime = 0f;
            }

            _animT = _isMenuOpen
                ? Mathf.Min(_animT + Time.deltaTime, 1f)
                : Mathf.Max(_animT - Time.deltaTime, 0f);

            float easedT = _isMenuOpen
                ? EaseOutExpo(_animT)
                : EaseInExpo(_animT);

            _softAnimT = Mathf.Lerp(_softAnimT, easedT, Time.deltaTime * 20);
            Animator.Play(OpeningHash, 0, _softAnimT);

            if (_isMenuOpen)
            {
                _openingIdleTime += Time.deltaTime;

                if (_openingIdleTime > 5f)
                {
                    if (Logger.InfoEnabled)
                        Logger.Info("Idle time exceeded 5 seconds. Closing menu.");
                    _isMenuOpen = false;
                    _openingIdleTime = 0f;
                }
            }
            else
            {
                _openingIdleTime = 0f;
            }

            if (_currentLang != LocalizationLang)
            {
                _currentLang = LocalizationLang;
                LocalizeTextProvider.CurrentSetting = LocalizationLang;
                Localizer.DoLocalizeUpdate(LocalizeTextProvider);
                if (Logger.InfoEnabled)
                    Logger.Info($"Localization language set to {LocalizationLang}");
            }
        }

        private void OnDestroy()
        {
            ApiClient?.Dispose();
        }

        private static float EaseInExpo(float t) { return t == 0f ? 0f : Mathf.Pow(4f, 10f * (t - 1f)); }
        private static float EaseOutExpo(float t) { return t == 1f ? 1f : 1f - Mathf.Pow(4f, -10f * t); }
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            _isUIHovering = true;
        }
        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            _isUIHovering = false;
        }
    }
}
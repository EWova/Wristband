using EWova.LearningPortfolio;
using EWova.Localization;

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EWova.Wristband
{
    public class ScreenshotObject
    {
        public readonly Texture2D Texture;
        public readonly string Url;

        public ScreenshotObject(Texture2D texture, string url)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException("URL cannot be null or empty.", nameof(url));
            Texture = texture;
            Url = url;
        }
    }
    public class Wristband : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Button MainMenuBTN;
        public GameObject ChildMenuRoot;
        public Animator Animator;
        public Localizer Localizer;
        public LocalizationLang LocalizationLang = LocalizationLang.auto;

        public RectTransform LearningPortfolioFrame;

        public float MenuTransitionValue => _softAnimT;

        private bool _isMenuOpen = false;
        private bool t_isMenuOpen = false;
        private float _openingIdleTime = 0f;
        private float _animT = 0;
        private bool _isUIHovering = false;
        private static readonly int OpeningHash = Animator.StringToHash("Opening");
        private float _softAnimT = 0f;
        private LocalizationLang _currentLang = LocalizationLang.auto;
        internal Action OnButtonInvoke;
        [SerializeField] internal AlertUI AlertUI;

        public WApiClient ApiClient { get; private set; }
        public ScreenshotObject LastScreenshot
        {
            get => _lastScreenshot;
            set
            {
                if (_lastScreenshot != value)
                {
                    var old = _lastScreenshot;
                    _lastScreenshot = value;

                    if (old != null)
                        Destroy(old.Texture);
                }
            }
        }
        private ScreenshotObject _lastScreenshot;

        public DefaultTextProvider LocalizeTextProvider { get; private set; }

        private readonly Dictionary<string, ApiModels.Feature> _features = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>供各 BaseBTN 依自己的 FeatureKey 向此拉取目前的顯示/啟用狀態。</summary>
        public bool TryGetFeature(string key, out ApiModels.Feature feature)
        {
            return _features.TryGetValue(key, out feature);
        }

        public void LoadFeatures(string flags)
        {
            var keys = flags.Split(',');
            var states = new ApiModels.Feature[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                states[i] = new ApiModels.Feature
                {
                    key = keys[i].Trim(),
                    visible = true,
                    enabled = true
                };
            }
            LoadFeatures(states);
        }

        public void LoadFeatures(ApiModels.Feature[] features)
        {
            _features.Clear();

            foreach (var state in features)
            {
                if (string.IsNullOrEmpty(state.key)) continue;

                if (!WristbandCapabilities.Supported.Contains(state.key))
                {
                    if (Logger.InfoEnabled)
                        Logger.Info($"Feature '{state.key}' skipped: not supported by this installation.");
                    continue;
                }

                _features[state.key] = state;

                if (Logger.InfoEnabled)
                    Logger.Info($"Feature '{state.key}': visible={state.visible}, enabled={state.enabled}, reason='{state.disabledReason}'");
            }

            // 廣播讓底下所有 BaseBTN 各自向 TryGetFeature 拉取最新狀態，而非由這裡直接推送。
            OnButtonInvoke?.Invoke();
        }

        private void Awake()
        {
            ApiClient = new WApiClient();
            ApiClient.LoggerLevel = LogLevel.Full;
            InitializeLocalization();

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
                LocalizeTextProvider.CurrentSetting = _currentLang;
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
        private void Start()
        {
            AlertUI.Close();
        }
        private void Update()
        {
            if (_isUIHovering || AlertUI.IsOpen)
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
                if (!t_isMenuOpen)
                {
                    t_isMenuOpen = true;
                }

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
                if (t_isMenuOpen)
                {
                    AlertUI.Close();
                    t_isMenuOpen = false;
                }

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
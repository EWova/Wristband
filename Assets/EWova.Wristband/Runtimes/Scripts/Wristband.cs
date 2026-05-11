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
        [SerializeField] private FeatureGroup[] _featureGroups;

        private bool _isMenuOpen = false;
        private float _openingIdleTime = 0f;
        private float _animT = 0;
        private bool _isUIHovering = false;
        private CircleButtonElement[] _circleButtonElements;
        private static readonly int OpeningHash = Animator.StringToHash("Opening");
        private float _softAnimT = 0f;

        public void LoadFlag(string flags)
        {
            foreach (var group in _featureGroups)
            {
                bool isActive = flags.Contains(group.Flag, StringComparison.InvariantCultureIgnoreCase);
                group.Element.gameObject.SetActive(isActive);
                Logger.Info($"Setting feature '{group.Flag}' to {(isActive ? "active" : "inactive")} based on flags.");
            }
        }

        private void Awake()
        {
            foreach (var group in _featureGroups)
            {
                group.Element.gameObject.SetActive(false);
            }

            MainMenuBTN.onClick.AddListener(() =>
            {
                _isMenuOpen = !_isMenuOpen;
                Logger.Info($"Main menu button clicked. Menu is now {(_isMenuOpen ? "open" : "closed")}.");
            });
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
                    Logger.Info("Idle time exceeded 5 seconds. Closing menu.");
                    _isMenuOpen = false;
                    _openingIdleTime = 0f;
                }
            }
            else
            {
                _openingIdleTime = 0f;
            }
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
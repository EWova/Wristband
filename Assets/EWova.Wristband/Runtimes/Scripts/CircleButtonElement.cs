using System;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace EWova.Wristband
{
    public class CircleButtonElement : MonoBehaviour
    {
        public Image LoadReminderFillImage;
        public Button Button;
        public TextMeshProUGUI LabelTMP;

        public Action OnClick;

        private bool _isDone = false;
        private float _progress = 0f;
        private bool _isFeatureEnabled = true;

        private bool _isDirty = true;

        /// <summary>後端 feature flag 控制的啟用狀態。false 時按鈕灰掉，視覺層另行處理。</summary>
        public bool IsFeatureEnabled
        {
            get => _isFeatureEnabled;
            set
            {
                if (_isFeatureEnabled != value)
                {
                    _isFeatureEnabled = value;
                    _isDirty = true;
                }
            }
        }

        /// <summary>停用原因的 localization key（對應 Wristband.tsv）。</summary>
        public string DisabledReasonKey { get; set; }

        public bool Show
        {
            get => gameObject.activeSelf;
            set => gameObject.SetActive(value);
        }

        public bool IsDone
        {
            get => _isDone;
            set
            {
                if (_isDone != value)
                {
                    _isDone = value;
                    _isDirty = true;
                }
            }
        }

        public float Progress
        {
            get => _progress;
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    _isDirty = true;
                }
            }
        }

        private void Awake()
        {
            Button.interactable = false;
            LoadReminderFillImage.fillAmount = 1.0f;
            Button.onClick.AddListener(ProcessClick);
        }

        private void Update()
        {
            if (!_isDirty)
                return;
            Button.interactable = IsDone && IsFeatureEnabled;
            if (IsDone)
                LoadReminderFillImage.fillAmount = 0f;
            else
                LoadReminderFillImage.fillAmount = Remap(float.IsNaN(Progress) ? 0f : Progress, 0f, 1f, 1f, 0.1f); // 從1到0.1，避免完全填滿時看不出來

            _isDirty = false;
        }

        public void ProcessClick()
        {
            if (!IsDone)
            {
                Logger.Warn("按鈕功能載入中，尚無法點擊");
                return;
            }

            try
            {
                OnClick?.Invoke();
            }
            catch (Exception ex)
            {
                Logger.Err("按鈕點擊事件發生錯誤");
                UnityEngine.Debug.LogException(ex);
            }
        }

        private static float Remap(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
    }
}
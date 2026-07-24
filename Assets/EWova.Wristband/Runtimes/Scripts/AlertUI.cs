using System;
using System.Threading;

using Cysharp.Threading.Tasks;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace EWova.Wristband
{
    public enum SubmitType
    {
        Close,
        Sub,
        Main
    }

    public struct SubmitResult
    {
        public SubmitType Type;
        public readonly bool IsOk => Type == SubmitType.Sub || Type == SubmitType.Main;
        public readonly bool IsClose => Type == SubmitType.Close;

        public static implicit operator SubmitResult(SubmitType type) => new SubmitResult { Type = type };
        public static implicit operator SubmitType(SubmitResult result) => result.Type;
        public static implicit operator bool(SubmitResult result) => result.IsOk;
    }

    public class AlertUI : MonoBehaviour
    {
        public struct AlertData
        {
            public string Message;
            public string SubSubmitMessage;
            public string MainSubmitMessage;
            public Texture2D ShowTexture;
            public Action SubSubmit;
            public Action MainSubmit;
            public Action Cancel;

            /// <summary>
            /// 是否關閉關閉按鈕，請當心使用，並配合 Close() 方法使用，否則可能造成介面無法關閉的情況
            /// </summary>
            public bool IsHideCloseButton;
        }

        public Button SubSubmitBTN;
        public TextMeshProUGUI SubSubmitTMP;
        public Button MainSubmitBTN;
        public TextMeshProUGUI MainSubmitTMP;
        public Image ImagePreview;
        public Button CloseBTN;
        public TextMeshProUGUI MessageTMP;

        private Action _onSubSubmit;
        private Action _onMainSubmit;
        private Action _onCancel;

        private UniTaskCompletionSource<SubmitResult> _openTask = null;
        internal bool IsOpen => gameObject.activeInHierarchy;

        private void Awake()
        {
            SubSubmitBTN.onClick.AddListener(OnSubmitBTN);
            MainSubmitBTN.onClick.AddListener(OnSubmit2BTN);
            CloseBTN.onClick.AddListener(OnCloseBTN);
        }

        private Sprite t_cache = null;
        public UniTask<SubmitResult> OpenAsync(AlertData data)
        {
            if (_openTask != null)
                CloseInternal(SubmitType.Close);
            else
                Reset();

            _openTask = new();

            _onSubSubmit = () =>
            {
                CloseInternal(SubmitType.Sub);
                try { data.SubSubmit?.Invoke(); }
                catch (Exception ex) { Debug.LogError($"Error in OnSubmit: {ex}"); }
            };

            _onMainSubmit = () =>
            {
                CloseInternal(SubmitType.Main);
                try { data.MainSubmit?.Invoke(); }
                catch (Exception ex) { Debug.LogError($"Error in OnSubmit2: {ex}"); }
            };

            _onCancel = () =>
            {
                CloseInternal(SubmitType.Close);
                try { data.Cancel?.Invoke(); }
                catch (Exception ex) { Debug.LogError($"Error in OnCancel: {ex}"); }
            };

            MessageTMP.text = data.Message;

            SubSubmitTMP.text = data.SubSubmitMessage;
            SubSubmitTMP.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(data.SubSubmitMessage));

            MainSubmitTMP.text = data.MainSubmitMessage;
            MainSubmitTMP.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(data.MainSubmitMessage));

            CloseBTN.gameObject.SetActive(!data.IsHideCloseButton);

            if (t_cache != null)
            {
                Destroy(t_cache);
                t_cache = null;
            }

            if (data.ShowTexture != null)
            {
                t_cache = Sprite.Create(data.ShowTexture, new Rect(0, 0, data.ShowTexture.width, data.ShowTexture.height), new Vector2(0.5f, 0.5f));
                ImagePreview.sprite = t_cache;
                ImagePreview.material.SetInt("_ToLinear", !data.ShowTexture.isDataSRGB ? 1 : 0);
                ImagePreview.gameObject.SetActive(true);
            }
            else
            {
                ImagePreview.gameObject.SetActive(false);
            }

            gameObject.SetActive(true);

            return _openTask.Task;
        }

        public void Close()
        {
            CloseInternal(null);
        }

        private void CloseInternal(SubmitType? submitType)
        {
            var openTask = _openTask;
            _openTask = null;

            gameObject.SetActive(false);
            Reset();

            if (submitType != null)
                openTask?.TrySetResult(submitType.Value);
        }

        private void OnSubmitBTN()
        {
            _onSubSubmit?.Invoke();
        }
        private void OnSubmit2BTN()
        {
            _onMainSubmit?.Invoke();
        }
        private void OnCloseBTN()
        {
            _onCancel?.Invoke();
        }

        public void Reset()
        {
            if (t_cache != null)
            {
                Destroy(t_cache);
                t_cache = null;
            }

            _onSubSubmit = null;
            _onMainSubmit = null;
            _onCancel = null;
        }

        private void OnDestroy()
        {
            Reset();
        }
    }
}
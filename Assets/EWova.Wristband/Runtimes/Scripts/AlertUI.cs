using System;
using System.Threading;

using Cysharp.Threading.Tasks;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace EWova.Wristband
{
    public class AlertUI : MonoBehaviour
    {
        public struct AlertData
        {
            public string Message;
            public string SubmitBTNMessage;
            public Action Submit;
            public Action Cancel;

            public readonly void Validate()
            {
                if (string.IsNullOrEmpty(Message))
                    throw new ArgumentException("Message cannot be null or empty");
            }
        }

        public Button SubmitBTN;
        public Button CloseBTN;
        public TextMeshProUGUI MessageTMP;
        public TextMeshProUGUI SubmitTMP;

        public Action OnSubmit;
        public Action OnCancel;

        private CancellationTokenSource _openCts = null;
        private UniTaskCompletionSource<bool> _openTask = null;
        private bool _isClosing = false;

        private void Awake()
        {
            SubmitBTN.onClick.AddListener(OnSubmitBTN);
            CloseBTN.onClick.AddListener(OnCloseBTN);
        }

        public void Open(AlertData data)
        {
            Reset();

            data.Validate();

            OnSubmit = data.Submit;
            OnCancel = data.Cancel;

            SubmitBTN.gameObject.SetActive(data.Submit != null);
            MessageTMP.text = data.Message;
            SubmitTMP.text = data.SubmitBTNMessage ?? "OK";
            gameObject.SetActive(true);
        }

        public UniTask<bool> OpenAsync(AlertData data)
        {
            Reset();

            data.Validate();

            _openTask = new UniTaskCompletionSource<bool>();
            _openCts = new CancellationTokenSource();

            var token = _openCts.Token;
            token.Register(() =>
            {
                _openTask?.TrySetResult(false);
            });

            OnSubmit = () =>
            {
                try { data.Submit?.Invoke(); }
                catch (Exception ex) { Debug.LogError($"Error in OnSubmit: {ex}"); }
                finally { _openTask?.TrySetResult(true); }
            };

            OnCancel = () =>
            {
                try { data.Cancel?.Invoke(); }
                catch (Exception ex) { Debug.LogError($"Error in OnCancel: {ex}"); }
                finally { _openTask?.TrySetResult(false); }
            };

            SubmitBTN.gameObject.SetActive(data.Submit != null);
            MessageTMP.text = data.Message;
            SubmitTMP.text = data.SubmitBTNMessage ?? "OK";
            gameObject.SetActive(true);

            return _openTask.Task;
        }

        public void Close()
        {
            if (_isClosing) return;
            _isClosing = true;

            try
            {
                gameObject.SetActive(false);
                Reset();
            }
            finally
            {
                _isClosing = false;
            }
        }

        private void OnSubmitBTN()
        {
            OnSubmit?.Invoke();
            Close();
        }

        private void OnCloseBTN()
        {
            OnCancel?.Invoke();
            Close();
        }

        public void Reset()
        {
            if (_openCts != null)
            {
                _openCts.Cancel();
                _openCts.Dispose();
                _openCts = null;
            }

            if (_openTask != null)
            {
                _openTask.TrySetResult(false);
                _openTask = null;
            }

            OnSubmit = null;
            OnCancel = null;
        }

        private void OnDestroy()
        {
            Reset();
        }
    }
}
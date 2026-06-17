using Cysharp.Threading.Tasks;

using UnityEngine;

namespace EWova.Wristband
{
    public abstract class BaseBTN : MonoBehaviour
    {
        public virtual string LabelKey => "{Label}";
        public virtual string DescriptionKey => "{Description}";

        private CircleButtonElement _circleButtonElement;
        private void OnValidate()
        {
            if (_circleButtonElement == null)
                _circleButtonElement = GetComponent<CircleButtonElement>();
        }

        private void Awake()
        {
            _circleButtonElement.OnClick += ProcessClickInternal;
        }
        private LoadProcess _loadProcess;
        private void Start()
        {
            Load();
        }
        private void Update()
        {
            if (_loadProcess != null)
            {
                _circleButtonElement.Progress = _loadProcess.Progress;
                _circleButtonElement.Button.interactable = !_loadProcess.Failed;
                _circleButtonElement.IsDone = _loadProcess.IsCompleted;

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
        protected abstract UniTask Load(LoadProcess loadProcess);
        protected abstract UniTask ProcessClick();

        internal void Load()
        {
            UniTask.Void(async () =>
            {
                _loadProcess = new();
                try
                {
                    await Load(_loadProcess);
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                    _loadProcess.SetFailed();
                }
            });
        }
        internal void ProcessClickInternal()
        {
            UniTask.Void(async () =>
            {
                try
                {
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
                    _circleButtonElement.IsDone = true;
                }
            });
        }
    }
}

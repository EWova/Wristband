using UnityEngine;

namespace EWova.Wristband
{
    public abstract class BaseBTN : MonoBehaviour
    {
        public virtual string Label => "{Label}";
        public virtual string Description => "{Description}";

        private CircleButtonElement _circleButtonElement;
        private void OnValidate()
        {
            if (_circleButtonElement == null)
                _circleButtonElement = GetComponent<CircleButtonElement>();
        }

        private void Awake()
        {
            _circleButtonElement.IsDone = true;
            _circleButtonElement.Progress = 1f;
            _circleButtonElement.Label = Label;
            _circleButtonElement.Description = Description;
            _circleButtonElement.OnClick += () =>
            {
                try
                {
                    Logger.Info($"{Label} button clicked.");
                    ProcessClick();
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                }
            };
        }
        private void Update()
        {
            _circleButtonElement.Label = Label;
            _circleButtonElement.Description = Description;
        }

        public abstract void ProcessClick();
    }
}

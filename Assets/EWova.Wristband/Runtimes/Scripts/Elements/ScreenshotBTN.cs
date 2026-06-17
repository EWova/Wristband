using Cysharp.Threading.Tasks;

using UnityEngine;

namespace EWova.Wristband
{
    public class ScreenshotBTN : BaseBTN
    {
        [SerializeField] private string m_label = "Screenshot";
        public override string LabelKey => m_label;
        [SerializeField] private string m_description = "Capture and share your moment on EWova";
        public override string DescriptionKey => m_description;

        protected override UniTask Load(LoadProcess loadProcess)
        {
            loadProcess.SetComplete();
            return UniTask.CompletedTask;
        }

        protected override UniTask ProcessClick()
        {
            return UniTask.CompletedTask;
        }
    }
}

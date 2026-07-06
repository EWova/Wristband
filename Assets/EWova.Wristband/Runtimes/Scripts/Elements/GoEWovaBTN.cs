using Cysharp.Threading.Tasks;

using UnityEngine;

namespace EWova.Wristband
{
    public class GoEWovaBTN : BaseBTN
    {
        [SerializeField] private string m_label = "Switch to EWova";
        public override string LabelKey => m_label;
        [SerializeField] private string m_description = "Switch to EWova";
        public override string DescriptionKey => m_description;

        protected override UniTask Load(LoadProcess loadProcess)
        {
            loadProcess.SetComplete();
            return UniTask.CompletedTask;
        }

        protected override UniTask ProcessClick()
        {
            string url = EWovaApp.GetDeepLink(LaunchViaDeepLinkOption.Default);
            if (Logger.InfoEnabled)
                Logger.Info($"Opening EWova with URL: {url}");

            if (!Application.isEditor)
                Application.OpenURL(url);

            return UniTask.CompletedTask;
        }
    }
}

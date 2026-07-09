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

        protected override async UniTask ProcessClick()
        {
            string message = Wristband.LocalizeTextProvider.GetLocalizedString("GoToEWovaConfirm");
            string submitLabel = Wristband.LocalizeTextProvider.GetLocalizedString("Confirm");
            bool confirmed = await Wristband.AlertUI.OpenAsync(new AlertUI.AlertData
            {
                Message = message,
                SubmitBTNMessage = submitLabel,
                Submit = () => { }
            });

            if (!confirmed)
                return;

            string url = EWovaApp.GetDeepLink(LaunchViaDeepLinkOption.Default);
            if (Logger.InfoEnabled)
                Logger.Info($"Opening EWova with URL: {url}");

            if (!Application.isEditor)
                Application.OpenURL(url);
        }
    }
}

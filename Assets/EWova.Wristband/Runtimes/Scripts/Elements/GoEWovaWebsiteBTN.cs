using Cysharp.Threading.Tasks;

using UnityEngine;

namespace EWova.Wristband
{
    public class GoEWovaWebsiteBTN : BaseBTN
    {
        public override string LabelKey => "ExploreEWova";
        public override string FeatureKey => "EXPLORE_EWOVA_WEBSITE";
        protected override UniTask Load(LoadProcess loadProcess)
        {
            base.Load(loadProcess);
            loadProcess.SetComplete();
            return UniTask.CompletedTask;
        }
        protected override async UniTask ProcessClick()
        {
            string message = GetLocalizedString("ExploreEWovaConfirm");
            string submitLabel = GetLocalizedString("Confirm");
            bool confirmed = await WristbandController.AlertUI.OpenAsync(new AlertUI.AlertData
            {
                Message = message,
                MainSubmitMessage = submitLabel,
            });

            if (!confirmed)
                return;

            Application.OpenURL("https://ewova.com/");
        }
    }
}

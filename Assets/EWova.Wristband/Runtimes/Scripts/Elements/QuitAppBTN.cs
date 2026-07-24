using Cysharp.Threading.Tasks;

namespace EWova.Wristband
{
    public class QuitAppBTN : BaseBTN
    {
        public override string LabelKey => "QuitApp";
        public override string FeatureKey => "QUIT_APP";

        protected override UniTask Load(LoadProcess loadProcess)
        {
            base.Load(loadProcess);
            loadProcess.SetComplete();
            return UniTask.CompletedTask;
        }

        protected override async UniTask ProcessClick()
        {
            string message = GetLocalizedString("QuitAppConfirm");
            string submitLabel = GetLocalizedString("Confirm");
            bool confirmed = await WristbandController.AlertUI.OpenAsync(new AlertUI.AlertData
            {
                Message = message,
                MainSubmitMessage = submitLabel,
            });

            if (!confirmed)
                return;

#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }
    }
}

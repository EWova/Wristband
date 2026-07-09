using Cysharp.Threading.Tasks;

using UnityEngine;

namespace EWova.Wristband
{
    public class QuitAppBTN : BaseBTN
    {
        [SerializeField] private string m_label = "Close App";
        public override string LabelKey => m_label;
        [SerializeField] private string m_description = "Close the app";
        public override string DescriptionKey => m_description;

        protected override UniTask Load(LoadProcess loadProcess)
        {
            loadProcess.SetComplete();
            return UniTask.CompletedTask;
        }

        protected override async UniTask ProcessClick()
        {
            string message = Wristband.LocalizeTextProvider.GetLocalizedString("QuitAppConfirm");
            string submitLabel = Wristband.LocalizeTextProvider.GetLocalizedString("Confirm");
            bool confirmed = await Wristband.AlertUI.OpenAsync(new AlertUI.AlertData
            {
                Message = message,
                SubmitBTNMessage = submitLabel,
                Submit = () => { }
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

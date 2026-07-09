using Cysharp.Threading.Tasks;

using EWova.LearningPortfolio;

using UnityEngine;

namespace EWova.Wristband
{
    public class ViewLearningProfileBTN : BaseBTN
    {
        [SerializeField] private string m_label = "ViewLearningProfile";
        public override string LabelKey => m_label;
        [SerializeField] private string m_description = "ViewLearningProfile";
        public override string DescriptionKey => m_description;


        private void OnEnable()
        {
#if EWOVA_LEARNING_PORTFOLIO
            LearningPortfolio.LearningPortfolio.OnUserLogin += OnLPUserLogin;
            LearningPortfolio.LearningPortfolio.OnUserLogout += OnLPUserLogout;
            SyncState();
#endif
        }

        private void OnDisable()
        {
#if EWOVA_LEARNING_PORTFOLIO
            LearningPortfolio.LearningPortfolio.OnUserLogin -= OnLPUserLogin;
            LearningPortfolio.LearningPortfolio.OnUserLogout -= OnLPUserLogout;
#endif
        }

#if EWOVA_LEARNING_PORTFOLIO
        private void OnLPUserLogin(LearningPortfolio.LearningPortfolio.UserData _) => SyncState();
        private void OnLPUserLogout() => SyncState();

        protected override void SyncState()
        {
            bool isConnected = LearningPortfolio.LearningPortfolio.IsConnected;
            CircleButtonElement.Show = true;
            CircleButtonElement.DisabledReasonKey = isConnected ? null : "FEATURE_NOT_AVAILABLE";
        }
#endif

        protected override UniTask Load(LoadProcess loadProcess)
        {
#if EWOVA_LEARNING_PORTFOLIO
            loadProcess.SetComplete();
            SyncState();
            return UniTask.CompletedTask;
#else
            return UniTask.CompletedTask;
#endif
        }

        protected override async UniTask ProcessClick()
        {
#if EWOVA_LEARNING_PORTFOLIO
            if (!LearningPortfolio.LearningPortfolio.IsConnected)
            {
                bool ok = await Wristband.AlertUI.OpenAsync(new AlertUI.AlertData
                {
                    Message = Wristband.LocalizeTextProvider.GetLocalizedString("LearningPortfolioNotConnected"),
                    SubmitBTNMessage = Wristband.LocalizeTextProvider.GetLocalizedString("Confirm"),
                    Submit = () => { }
                });

                if (!ok)
                    return;

                var process = new ConnectProcess();
                await LearningPortfolio.LearningPortfolio.ConnectAsync(process, this.destroyCancellationToken);

                if (!process.IsSuccess)
                {
                    Debug.LogError($"Login failed: {process.ClientErrorMessage} {process.ServerErrorMessage}");
                    return;
                }

                Debug.Log("Login successful");
            }
            await UniTask.NextFrame();
            SyncState();

            LearningPortfolio.LearningPortfolio.CreateUserProjectRecordShower(Wristband.LearningPortfolioFrame);
#endif
        }
    }
}

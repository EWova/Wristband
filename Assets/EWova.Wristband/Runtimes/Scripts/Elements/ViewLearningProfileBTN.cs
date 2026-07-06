using Cysharp.Threading.Tasks;

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
            CircleButtonElement.Show = LearningPortfolio.LearningPortfolio.IsConnected;
            CircleButtonElement.IsFeatureEnabled = LearningPortfolio.LearningPortfolio.IsConnected;
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

        protected override UniTask ProcessClick()
        {
#if EWOVA_LEARNING_PORTFOLIO
            if (!LearningPortfolio.LearningPortfolio.IsConnected)
            {
                if (Logger.WarnEnabled)
                    Logger.Warn("LearningPortfolio is not connected. Cannot view learning profile.");
                return UniTask.CompletedTask;
            }
            SyncState();

            LearningPortfolio.LearningPortfolio.CreateUserProjectRecordShower(Wristband.LearningPortfolioFrame);
#endif
            return UniTask.CompletedTask;
        }
    }
}

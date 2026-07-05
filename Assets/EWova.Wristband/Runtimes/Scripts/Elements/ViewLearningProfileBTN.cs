#if EWOVA_LEARNING_PORTFOLIO
using EWova.LearningPortfolio;
#endif

using Cysharp.Threading.Tasks;

using UnityEngine;
using System.Diagnostics;

namespace EWova.Wristband
{
    public class ViewLearningProfileBTN : BaseBTN
    {
        [SerializeField] private string m_label = "ViewLearningProfile";
        public override string LabelKey => m_label;
        [SerializeField] private string m_description = "ViewLearningProfile";
        public override string DescriptionKey => m_description;

        [SerializeField] private RectTransform m_profileRoot;

        protected override async UniTask Load(LoadProcess loadProcess)
        {
#if EWOVA_LEARNING_PORTFOLIO
            if (LearningPortfolio.LearningPortfolio.IsConnected)
            {
                loadProcess.SetComplete();
                return;
            }

            var process = new CheckAvailabilityProcess();
            await LearningPortfolio.LearningPortfolio.CheckAvailabilityAsync(process, destroyCancellationToken);

            if (process.IsSuccess)
                loadProcess.SetComplete();
            else
                loadProcess.SetFailed();
#else
            loadProcess.SetFailed();
            await UniTask.CompletedTask;
#endif
        }

        protected override UniTask ProcessClick()
        {
#if EWOVA_LEARNING_PORTFOLIO
            if (!LearningPortfolio.LearningPortfolio.IsConnected)
            {
                Logger.Warn("LearningPortfolio is not connected. Cannot view learning profile.");
                return UniTask.CompletedTask;
            }

            LearningPortfolio.LearningPortfolio.CreateUserProjectRecordShower(Wristband.LearningPortfolioFrame);
#endif
            return UniTask.CompletedTask;
        }
    }
}

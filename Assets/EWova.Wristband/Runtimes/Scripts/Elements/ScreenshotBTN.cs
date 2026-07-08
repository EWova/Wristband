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

        [SerializeField, Range(1, 100)] private int m_jpgQuality = 85;

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
            loadProcess.SetComplete();
#if EWOVA_LEARNING_PORTFOLIO
            SyncState();
#endif
            return UniTask.CompletedTask;
        }

        protected override async UniTask ProcessClick()
        {
#if EWOVA_LEARNING_PORTFOLIO
            if (!LearningPortfolio.LearningPortfolio.IsConnected)
            {
                if (Logger.WarnEnabled)
                    Logger.Warn("LearningPortfolio is not connected. Cannot capture screenshot.");
                return;
            }
#endif
            var canvasGroup = Wristband != null ? Wristband.ChildMenuCanvasGroup : null;
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;

            await UniTask.NextFrame();

            Texture2D tex = null;
            try
            {
                tex = ScreenCapture.CaptureScreenshotAsTexture();
                var bytes = tex.EncodeToJPG(m_jpgQuality);

                var response = await ApiClient.UploadScreenshotAsync(
                    new ApiModels.UploadScreenshotRequest { imageData = bytes }
                );

                if (Wristband != null)
                    Wristband.LastScreenshotUrl = response.imageUrl;

                if (Logger.InfoEnabled)
                    Logger.Info($"Screenshot uploaded: {response.imageUrl}");
            }
            finally
            {
                if (tex != null) Object.Destroy(tex);
                if (canvasGroup != null) canvasGroup.alpha = 1f;
            }
        }
    }
}

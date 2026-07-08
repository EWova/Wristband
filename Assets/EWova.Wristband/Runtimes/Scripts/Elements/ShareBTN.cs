using Cysharp.Threading.Tasks;

using UnityEngine;

namespace EWova.Wristband
{
    public class ShareBTN : BaseBTN
    {
        [SerializeField] private string m_label = "Share to EWova";
        public override string LabelKey => m_label;
        [SerializeField] private string m_description = "Let others see your activity on EWova";
        public override string DescriptionKey => m_description;

        [SerializeField] private string m_shareDescription = "";
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
                    Logger.Warn("LearningPortfolio is not connected. Cannot share activity.");
                return;
            }
#endif
            string imageUrl = Wristband != null ? Wristband.LastScreenshotUrl : null;

            // If no cached screenshot, capture and upload one now
            if (string.IsNullOrEmpty(imageUrl))
            {
                var canvasGroup = Wristband?.ChildMenuCanvasGroup;
                if (canvasGroup != null) canvasGroup.alpha = 0f;

                await UniTask.NextFrame();

                Texture2D tex = null;
                try
                {
                    tex = ScreenCapture.CaptureScreenshotAsTexture();
                    var bytes = tex.EncodeToJPG(m_jpgQuality);

                    var uploadResponse = await ApiClient.UploadScreenshotAsync(
                        new ApiModels.UploadScreenshotRequest { imageData = bytes }
                    );
                    imageUrl = uploadResponse.imageUrl;
                }
                finally
                {
                    if (tex != null) Object.Destroy(tex);
                    if (canvasGroup != null) canvasGroup.alpha = 1f;
                }
            }

            await ApiClient.ShareActivityAsync(new ApiModels.ShareActivityRequest
            {
                imageUrl = imageUrl,
                description = m_shareDescription
            });

            // Clear cached URL after a successful share
            if (Wristband != null)
                Wristband.LastScreenshotUrl = null;

            if (Logger.InfoEnabled)
                Logger.Info("Activity shared successfully.");
        }
    }
}

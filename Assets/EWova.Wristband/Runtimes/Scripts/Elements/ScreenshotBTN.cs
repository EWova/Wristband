using Cysharp.Threading.Tasks;

using UnityEngine;

namespace EWova.Wristband
{
    public class ScreenshotBTN : BaseBTN
    {
        public override string LabelKey => "CaptureToEWova";
        public override string FeatureKey => "CAPTURE_TO_EWOVA";

        [Tooltip("截圖的 JPG 品質，範圍為 1 到 100。 預設為 95。")]
        [SerializeField, Range(1, 100)] private int m_jpgQuality = 95;
        [Tooltip("延遲截圖的時間，單位為秒。")]
        [SerializeField, Range(0.3f, 10f)] private float m_captureDelay = 3f;

        private int m_nextProcessCooldown = 3000;

        private void OnEnable()
        {
            LearningPortfolio.LearningPortfolio.OnUserLogin += OnLPUserLogin;
            LearningPortfolio.LearningPortfolio.OnUserLogout += OnLPUserLogout;
            SyncState();
        }

        private void OnDisable()
        {
            LearningPortfolio.LearningPortfolio.OnUserLogin -= OnLPUserLogin;
            LearningPortfolio.LearningPortfolio.OnUserLogout -= OnLPUserLogout;
        }

        private void OnLPUserLogin(LearningPortfolio.LearningPortfolio.UserData _) => SyncState();
        private void OnLPUserLogout() => SyncState();

        protected override void UpdateBaseState()
        {
            bool isConnected = LearningPortfolio.LearningPortfolio.IsConnected;
            BaseShow = isConnected;
            BaseEnabled = isConnected;
        }

        protected override UniTask Load(LoadProcess loadProcess)
        {
            base.Load(loadProcess);
            loadProcess.SetComplete();
            return UniTask.CompletedTask;
        }

        bool m_isProcessing = false;

        protected override async UniTask ProcessClick()
        {
            if (!LearningPortfolio.LearningPortfolio.IsConnected)
            {
                if (Logger.WarnEnabled)
                    Logger.Warn("LearningPortfolio is not connected. Cannot capture screenshot.");
                return;
            }

            Texture2D tex = null;
            try
            {
                m_isProcessing = true;

            RECAPTURE:
                WristbandController.ChildMenuRoot.SetActive(false);

                string timerMsg = GetLocalizedString("TimerSeconds");
                var captureOption = await WristbandController.AlertUI.OpenAsync(new AlertUI.AlertData
                {
                    Message = GetLocalizedString("CaptureGuidelines"),
                    SubSubmitMessage = string.Format(timerMsg, m_captureDelay),
                    MainSubmitMessage = GetLocalizedString("CaptureNow"),
                    IsHideCloseButton = true
                });

                if (captureOption.IsClose)
                    return;

                if (captureOption.Type == SubmitType.Sub)
                {
                    WristbandController.MainMenuCircleCountdown(m_captureDelay);
                    await UniTask.Delay((int)(m_captureDelay * 1000));
                }
                tex = await ScreenshotHelper.Capture(1);

                WristbandController.ChildMenuRoot.SetActive(true);

                var captureCheck = await WristbandController.AlertUI.OpenAsync(new AlertUI.AlertData
                {
                    Message = GetLocalizedString("CaptureCheck"),
                    ShowTexture = tex,
                    SubSubmitMessage = GetLocalizedString("CaptureRetake"),
                    MainSubmitMessage = GetLocalizedString("Confirm"),
                });

                if (captureCheck.IsClose)
                    return;

                if (captureCheck.Type == SubmitType.Sub)
                    goto RECAPTURE;

                var bytes = tex.EncodeToJPG(m_jpgQuality);

                var response = await ApiClient.UploadScreenshotAsync(
                    new ApiModels.UploadScreenshotRequest { imageData = bytes }
                );

                if (!response.success)
                {
                    string errorMsg = $"{response.error?.code} - {string.Join(", ", response.error?.message ?? new string[0])}";
                    if (Logger.WarnEnabled)
                        Logger.Warn($"Failed to upload screenshot: {errorMsg}");

                    string msg = GetLocalizedString("ScreenshotFailed");
                    msg = string.Format(msg, errorMsg);

                    WristbandController.AlertUI.OpenAsync(new AlertUI.AlertData
                    {
                        Message = msg,
                        SubSubmitMessage = GetLocalizedString("Confirm"),
                    }).Forget();

                    return;
                }

                WristbandController.LastScreenshot = new ScreenshotObject
                (
                    tex,
                    response.data.imageUrl
                );

                if (Logger.InfoEnabled)
                    Logger.Info($"Screenshot uploaded: {response.data.imageUrl}");
            }
            catch (System.Exception ex)
            {
                if (tex != null)
                {
                    Destroy(tex);
                    tex = null;
                }

                Debug.LogException(ex);
                if (Logger.ErrorEnabled)
                    Logger.Err($"Error during screenshot process: {ex.Message}");

                WristbandController.AlertUI.OpenAsync(new AlertUI.AlertData
                {
                    Message = GetLocalizedString("ScreenshotError"),
                }).Forget();
            }
            finally
            {
                WristbandController.ChildMenuRoot.SetActive(true);

                float waitTime = m_nextProcessCooldown / 1000f;
                // 等待一段時間，避免連續點擊導致多次截圖
                while (waitTime > 0)
                {
                    await UniTask.Yield();

                    waitTime -= Time.deltaTime;
                    if (waitTime < 0)
                        waitTime = 0;
                    CircleButtonElement.Progress = 1f - (waitTime / (m_nextProcessCooldown / 1000f));
                }

                m_isProcessing = false;
            }
        }
    }
}

using Cysharp.Threading.Tasks;

using UnityEngine;

namespace EWova.Wristband
{
    public class ShareBTN : BaseBTN
    {
        public override string LabelKey => "ShareToEWova";
        public override string FeatureKey => "SHARE_TO_EWOVA";

        [SerializeField] private string m_shareDescription = "";

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
                    Logger.Warn("LearningPortfolio is not connected. Cannot share activity.");
                return;
            }

            Texture2D tex = null;

            try
            {
                m_isProcessing = true;

                SubmitResult option = await WristbandController.AlertUI.OpenAsync(new AlertUI.AlertData
                {
                    Message = GetLocalizedString("ShareConfirm"),
                    SubSubmitMessage = GetLocalizedString("ShareWithScreenshotConfirm"),
                    MainSubmitMessage = GetLocalizedString("Confirm"),
                });

                if (option.IsClose)
                    return;

                if (option.Type == SubmitType.Sub)
                {
                    try
                    {
                        bool takeNewOne = true;
                        if (WristbandController.LastScreenshot != null)
                        {
                            SubmitResult useLastScreenshot = await WristbandController.AlertUI.OpenAsync(new AlertUI.AlertData
                            {
                                Message = GetLocalizedString("ShareUseLastCapture"),
                                ShowTexture = WristbandController.LastScreenshot.Texture,
                                SubSubmitMessage = GetLocalizedString("ShareDoNotUseLastCaptureConfirm"),
                                MainSubmitMessage = GetLocalizedString("Confirm"),
                                IsHideCloseButton = true
                            });

                            if (useLastScreenshot.Type == SubmitType.Sub)
                            {
                                tex = WristbandController.LastScreenshot.Texture;
                                takeNewOne = true;
                            }
                        }

                        if (takeNewOne)
                        {
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

                            if (captureOption.Type == SubmitType.Sub)
                                await UniTask.Delay((int)(m_captureDelay * 1000));

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
                        }

                        var bytes = tex.EncodeToJPG(m_jpgQuality);
                        var uploadResponse = await ApiClient.UploadScreenshotAsync(
                            new ApiModels.UploadScreenshotRequest { imageData = bytes }
                        );

                        if (!uploadResponse.success)
                        {
                            string errorMsg = $"{uploadResponse.error?.code} - {string.Join(", ", uploadResponse.error?.message ?? new string[0])}";
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
                            uploadResponse.data.imageUrl
                        );
                    }
                    finally
                    {
                        WristbandController.ChildMenuRoot.SetActive(true);
                    }
                }

                string shareMsg = GetLocalizedString("ShareActivityDefaultPost");
                shareMsg = shareMsg
                    .Replace("{AppName}", LearningPortfolio.LearningPortfolio.ConnectedProject.Name)
                    .Replace("{CompletionProgress}", ((int)(LearningPortfolio.LearningPortfolio.LoggedUserProjectRecordSheet.CompletionProgress * 100f)).ToString());
                var shareCheck = await WristbandController.AlertUI.OpenAsync(new AlertUI.AlertData
                {
                    Message = shareMsg,
                    ShowTexture = tex,
                    MainSubmitMessage = GetLocalizedString("ShareActivityConfirm"),
                });

                if (shareCheck.IsClose)
                    return;

                var callback = await ApiClient.ShareActivityAsync(new ApiModels.ShareActivityRequest
                {
                    imageUrl = WristbandController.LastScreenshot.Url,
                    description = m_shareDescription
                });

                if (!callback.success)
                {
                    string errorMsg = $"{callback.error?.code} - {string.Join(", ", callback.error?.message ?? new string[0])}";
                    if (Logger.WarnEnabled)
                        Logger.Warn($"Failed to share activity: {errorMsg}");
                    string msg = GetLocalizedString("ShareActivityFailed");
                    msg = string.Format(msg, errorMsg);
                    WristbandController.AlertUI.OpenAsync(new AlertUI.AlertData
                    {
                        Message = msg,
                        SubSubmitMessage = GetLocalizedString("Confirm"),
                    }).Forget();
                    return;
                }

                if (WristbandController != null)
                    WristbandController.LastScreenshot = null;

                if (Logger.InfoEnabled)
                    Logger.Info("Activity shared successfully.");
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                if (Logger.ErrorEnabled)
                    Logger.Err($"Error during share process: {ex.Message}");

                WristbandController.AlertUI.OpenAsync(new AlertUI.AlertData
                {
                    Message = GetLocalizedString("ShareActivityError"),
                }).Forget();
            }
            finally
            {
                if (tex != null)
                {
                    Destroy(tex);
                    tex = null;
                }

                await UniTask.Delay(m_nextProcessCooldown);

                m_isProcessing = false;
            }
        }
    }
}

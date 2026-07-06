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

        protected override UniTask Load(LoadProcess loadProcess)
        {
            loadProcess.SetComplete();
            return UniTask.CompletedTask;
        }

        protected override async UniTask ProcessClick()
        {
            string imageUrl = Wristband?.LastScreenshotUrl;

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

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

        protected override UniTask Load(LoadProcess loadProcess)
        {
            loadProcess.SetComplete();
            return UniTask.CompletedTask;
        }

        protected override async UniTask ProcessClick()
        {
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

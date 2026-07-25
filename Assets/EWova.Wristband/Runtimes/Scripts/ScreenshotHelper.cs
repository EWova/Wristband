using Cysharp.Threading.Tasks;
using UnityEngine;

namespace EWova.Wristband
{
    public static class ScreenshotHelper
    {
        public static async UniTask<Texture2D> Capture(int superSizeScale)
        {
            await UniTask.WaitForEndOfFrame();

            var tex = ScreenCapture.CaptureScreenshotAsTexture(superSizeScale);

            if (tex == null)
                throw new System.Exception("Failed to capture screenshot.");

            return tex;
        }
    }
}

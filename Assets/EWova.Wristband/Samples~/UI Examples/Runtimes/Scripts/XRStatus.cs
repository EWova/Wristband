using Cysharp.Threading.Tasks;

using System.Threading;

using UnityEngine;
using UnityEngine.XR.Management;

namespace EWova.Wristband.Samples.UiExamples
{
    public static class XRStatus
    {
        private static UniTask<bool>? _cache;

        /// <summary>
        /// 判斷目前是否有 XR 裝置正在運行 (Loader 已啟動且初始化完成)
        /// </summary>
        /// <param name="checkInterval">檢查間隔時間 (秒)</param>
        /// <param name="timeoutSeconds">超時時間 (秒)</param>
        /// <returns></returns>
        public static UniTask<bool> IsXRActiveAsync(
            float checkInterval = 0.5f,
            float timeoutSeconds = 3.0f,
            CancellationToken cancellationToken = default)
        {
            try { _cache ??= InternalIsXRActiveAsync().Preserve(); }
            catch
            {
                _cache = null;
                throw;
            }
            return _cache.Value.AttachExternalCancellation(cancellationToken);
        }

        private static async UniTask<bool> InternalIsXRActiveAsync(
            float checkInterval = 0.5f,
            float timeoutSeconds = 3.0f)
        {
            float elapsed = 0f;
            while (!IsXRActive && elapsed < timeoutSeconds)
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(checkInterval));
                elapsed += checkInterval;
            }
            return IsXRActive;
        }

        /// <summary>
        /// 判斷目前是否有 XR 裝置正在運行 (Loader 已啟動且初始化完成)
        /// </summary>
        public static bool IsXRActive
        {
            get
            {
                var settings = XRGeneralSettings.Instance;
                if (settings == null || settings.Manager == null)
                    return false;

                return settings.Manager.isInitializationComplete
                       && settings.Manager.activeLoader != null;
            }
        }
    }
}

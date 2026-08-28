using Cysharp.Threading.Tasks;

using System;
using System.Threading;

using UnityEngine;
#if EWOVA_XR_MANAGEMENT
using UnityEngine.XR.Management;
#endif

namespace EWova.Wristband.Samples.UiExamples
{
    public static class XRStatus
    {
        private static bool? _isXRActiveCache;
        private static UniTaskCompletionSource<bool> _utcs;

        public static async UniTask<bool> IsXRActiveAsync(
            float checkInterval = 0.5f,
            float timeoutSeconds = 3.0f,
            CancellationToken cancellationToken = default)
        {
            if (_isXRActiveCache.HasValue)
                return _isXRActiveCache.Value;

            if (_utcs != null)
                return await _utcs.Task.AttachExternalCancellation(cancellationToken);

            _utcs = new UniTaskCompletionSource<bool>();
            try
            {
                _isXRActiveCache = await InternalIsXRActiveAsync(checkInterval, timeoutSeconds).AttachExternalCancellation(cancellationToken);
                _utcs.TrySetResult(_isXRActiveCache.Value);
                return _isXRActiveCache.Value;
            }
            catch (OperationCanceledException)
            {
                _utcs.TrySetCanceled();
                throw;
            }
            catch (Exception ex)
            {
                _utcs.TrySetException(ex);
                throw;
            }
            finally
            {
                _utcs = null;
            }
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
#if EWOVA_XR_MANAGEMENT
                var settings = XRGeneralSettings.Instance;
                if (settings == null || settings.Manager == null)
                    return false;

                return settings.Manager.isInitializationComplete
                       && settings.Manager.activeLoader != null;
#else
                return false;
#endif
            }
        }
    }
}

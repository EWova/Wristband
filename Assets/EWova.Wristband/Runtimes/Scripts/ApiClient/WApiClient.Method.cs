using System;
using System.Threading;

using Cysharp.Threading.Tasks;

using EWova.Networking;

namespace EWova.Wristband
{
    public enum WApiAction
    {
        Unknown,
        // 按鈕與系統行為追蹤
        RecordAppEvent,
        // 截圖與分享
        UploadScreenshot,
        ShareActivity,
        // 學習歷程
        GetLearningProfile
    }

    public partial class WApiClient
    {
        private const string WristbandContentType = "application/json";

        protected UniTask<string> Get(string endpoint, CancellationToken ct = default)
            => base.Get(endpoint, ct: ct);
        protected UniTask<T> Get<T>(string endpoint, CancellationToken ct = default)
            => base.Get<T>(endpoint, ct: ct);
        protected UniTask<string> Post(string endpoint, object jsonBody, CancellationToken ct = default)
            => base.Post(endpoint, jsonBody, contentType: WristbandContentType, ct: ct);
        protected UniTask<T> Post<T>(string endpoint, object jsonBody, CancellationToken ct = default)
            => base.Post<T>(endpoint, jsonBody, contentType: WristbandContentType, ct: ct);

        #region 截圖與分享 API (Screenshot & Share)
        public async UniTask<ApiModels.UploadScreenshotResponse> UploadScreenshotAsync(ApiModels.UploadScreenshotRequest request, CancellationToken ct = default)
        {
            try
            {
                return await Post<ApiModels.UploadScreenshotResponse>("/api/v1/me/screenshots", request, ct);
            }
            catch (ApiException ex)
            {
                throw new ApiWristbandException(WApiAction.UploadScreenshot, "Failed to upload screenshot.", ex);
            }
            catch (OperationCanceledException) { throw; }
        }

        public async UniTask ShareActivityAsync(ApiModels.ShareActivityRequest request, CancellationToken ct = default)
        {
            try
            {
                await Post("/api/v1/me/shares", request, ct);
            }
            catch (ApiException ex)
            {
                throw new ApiWristbandException(WApiAction.ShareActivity, "Failed to share activity.", ex);
            }
            catch (OperationCanceledException) { throw; }
        }

        #endregion
    }

    #region Exceptions

    public abstract class WristbandApiException : Exception
    {
        public WApiAction Action { get; protected set; }
        public ApiException SourceApiEx { get; protected set; }

        protected WristbandApiException(WApiAction action, string message, ApiException sourceApiEx = null)
            : base(message, sourceApiEx)
        {
            Action = action;
            SourceApiEx = sourceApiEx;
        }
    }

    public class ApiWristbandException : WristbandApiException
    {
        public ApiWristbandException(WApiAction action, string message, ApiException innerException = null)
            : base(action, message, innerException)
        {
        }
    }

    #endregion
}
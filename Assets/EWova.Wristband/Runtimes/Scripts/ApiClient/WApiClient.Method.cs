using System;
using System.Net;
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
        // 功能旗標
        GetFeatures,
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
            => Send<string>(RequestTask.GET(
                backendUrlOrAbsoluteUrl: endpoint,
                throwApiExceptionFor4xxResponses: false, // 不拋出 4xx 錯誤，讓呼叫端自行處理
                ct: ct));
        protected UniTask<T> Get<T>(string endpoint, CancellationToken ct = default)
            => Send<T>(RequestTask.GET(
                backendUrlOrAbsoluteUrl: endpoint,
                throwApiExceptionFor4xxResponses: false, // 不拋出 4xx 錯誤，讓呼叫端自行處理
                ct: ct));
        protected UniTask<string> Post(string endpoint, object jsonBody, CancellationToken ct = default)
            => Send<string>(RequestTask.POST(
                backendUrlOrAbsoluteUrl: endpoint,
                body: jsonBody,
                contentType: WristbandContentType,
                throwApiExceptionFor4xxResponses: false, // 不拋出 4xx 錯誤，讓呼叫端自行處理
                ct: ct));
        protected UniTask<T> Post<T>(string endpoint, object jsonBody, CancellationToken ct = default)
            => Send<T>(RequestTask.POST(
                backendUrlOrAbsoluteUrl: endpoint,
                body: jsonBody,
                contentType: WristbandContentType,
                throwApiExceptionFor4xxResponses: false, // 不拋出 4xx 錯誤，讓呼叫端自行處理
                ct: ct));

        #region 功能旗標 API (Feature Flags)

        public async UniTask<ApiModels.FeatureResponse> GetFeaturesAsync(CancellationToken ct = default)
        {
            try
            {
                return await Send<ApiModels.FeatureResponse>(RequestTask.GET(
                    backendUrlOrAbsoluteUrl: "http://127.0.0.1:5500/test.json",
                    isAbsoluteUrl: true,
                    throwApiExceptionFor4xxResponses: false,
                    ct: ct));

                return await Get<ApiModels.FeatureResponse>("api/v1/me/wristband/features", ct);
            }
            catch (ApiException ex)
            {
                throw new ApiWristbandException(WApiAction.GetFeatures, "Failed to get wristband features.", ex);
            }
            catch (OperationCanceledException) { throw; }
        }

        #endregion

        #region 截圖與分享 API (Screenshot & Share)
        public async UniTask<ApiModels.UploadScreenshotResponse> UploadScreenshotAsync(ApiModels.UploadScreenshotRequest request, CancellationToken ct = default)
        {
            try
            {
                return await Post<ApiModels.UploadScreenshotResponse>("api/v1/me/screenshots", request, ct);
            }
            catch (ApiException ex)
            {
                throw new ApiWristbandException(WApiAction.UploadScreenshot, "Failed to upload screenshot.", ex);
            }
            catch (OperationCanceledException) { throw; }
        }

        public async UniTask<ApiModels.ShareActivityResponse> ShareActivityAsync(ApiModels.ShareActivityRequest request, CancellationToken ct = default)
        {
            try
            {
                return await Post<ApiModels.ShareActivityResponse>("api/v1/me/shares", request, ct);
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
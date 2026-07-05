using System;

namespace EWova.Wristband
{
    /// <summary>
    /// 腕帶相關 API 的資料傳遞物件 (DTO) 集合
    /// </summary>
    public static class ApiModels
    {
        #region 行為追蹤 (Analytics & Events)

        [Serializable]
        public class AppEventRequest
        {
            public string eventType;   // 事件類型 (e.g., "click_website", "switch_app", "quit_app")
            public string eventData;   // 附帶的詳細資訊 (可為 JSON 字串)
            public long timestamp;     // 事件觸發時間戳記 (Unix Timestamp)
        }

        #endregion

        #region 功能旗標 (Feature Flags)

        [Serializable]
        public class FeatureState
        {
            public string key;
            public bool visible;
            public bool enabled;
            public string disabledReason; // localization key，對應 Wristband.tsv
        }

        [Serializable]
        public class GetFeaturesResponse
        {
            public FeatureState[] features;
        }

        #endregion

        #region 截圖與分享 (Screenshot & Share)

        [Serializable]
        public class UploadScreenshotRequest
        {
            public byte[] imageData;
        }

        [Serializable]
        public class UploadScreenshotResponse
        {
            public string imageUrl;    // 後端儲存後的圖片 CDN URL
            public string imageId;     // 圖片在資料庫中的唯一識別碼
        }

        [Serializable]
        public class ShareActivityRequest
        {
            public string imageUrl;    // 欲分享的圖片連結
            public string description; // 使用者輸入或自動生成的分享內容描述
        }

        #endregion
    }
}
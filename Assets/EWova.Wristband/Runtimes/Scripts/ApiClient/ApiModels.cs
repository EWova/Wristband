using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;

namespace EWova.Wristband
{
    /// <summary>
    /// 腕帶相關 API 的資料傳遞物件 (DTO) 集合
    /// </summary>
    public static class ApiModels
    {
        public class SingleOrArrayConverter<T> : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(List<T>) || objectType == typeof(T[]);
            }

            public override object ReadJson(
                JsonReader reader,
                Type objectType,
                object existingValue,
                JsonSerializer serializer)
            {
                JToken token = JToken.Load(reader);

                if (token.Type == JTokenType.Array)
                {
                    return token.ToObject<T[]>(serializer);
                }

                return new[] { token.ToObject<T>(serializer) };
            }

            public override void WriteJson(
                JsonWriter writer,
                object value,
                JsonSerializer serializer)
            {
                serializer.Serialize(writer, value);
            }
        }

        #region Common Response

        /// <summary>
        /// API 共用回傳結構
        /// </summary>
        public class BaseResponse<T>
        {
            public bool success;
            public T data;
            public ErrorResponse error;
            public string timestamp;
        }

        /// <summary>
        /// API 錯誤資訊
        /// </summary>
        public class ErrorResponse
        {
            public string code;
            [JsonConverter(typeof(SingleOrArrayConverter<string>))]
            public string[] message;
        }

        #endregion


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

        public class FeatureResponse : BaseResponse<FeatureData>
        {
        }

        public class FeatureData
        {
            public Feature[] features;
        }

        public class Feature
        {
            public string key;
            public bool visible;
            public bool enabled;
            public string disabledReason; // localization key，對應 Wristband.tsv
        }

        #endregion


        #region 截圖與分享 (Screenshot & Share)

        [Serializable]
        public class UploadScreenshotRequest
        {
            public byte[] imageData;
        }

        public class UploadScreenshotResponse : BaseResponse<UploadScreenshotData>
        {
        }

        public class UploadScreenshotData
        {
            public string imageUrl;    // 後端儲存後的圖片 CDN URL
            public string imageId;     // 圖片在資料庫中的唯一識別碼
        }


        [Serializable]
        public class ShareActivityRequest
        {
            public string imageUrl;     // 欲分享的圖片連結
            public string description;  // 使用者輸入或自動生成的分享內容描述
        }

        public class ShareActivityResponse : BaseResponse<object>
        {
        }

        #endregion
    }
}
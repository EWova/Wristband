using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;

using UnityEngine.Scripting;

namespace EWova.Wristband
{
    /// <summary>
    /// 腕帶相關 API 的資料傳遞物件 (DTO) 集合
    /// </summary>
    public static class ApiModels
    {
        // 這些 DTO 完全依賴 Newtonsoft.Json 反射填值，欄位本身在 C# 端可能沒有全部被讀取。
        // 第三方專案若開啟較高的 IL2CPP Managed Stripping Level，UnityLinker 會把「看似沒被使用」
        // 的欄位/建構子當成可移除的死碼，導致反序列化結果在 Editor 正常、在真機上卻是 null/預設值。
        // 加上 [Preserve] 讓這些型別與成員不受 stripping 影響，不依賴使用端的 Player Settings。
        [Preserve]
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
        [Preserve]
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
        [Preserve]
        public class ErrorResponse
        {
            public string code;
            [JsonConverter(typeof(SingleOrArrayConverter<string>))]
            public string[] message;
        }

        #endregion


        #region 行為追蹤 (Analytics & Events)

        [Serializable]
        [Preserve]
        public class AppEventRequest
        {
            public string eventType;   // 事件類型 (e.g., "click_website", "switch_app", "quit_app")
            public string eventData;   // 附帶的詳細資訊 (可為 JSON 字串)
            public long timestamp;     // 事件觸發時間戳記 (Unix Timestamp)
        }

        #endregion


        #region 功能旗標 (Feature Flags)

        [Preserve]
        public class FeatureResponse : BaseResponse<FeatureData>
        {
        }

        [Preserve]
        public class FeatureData
        {
            public Feature[] features;
        }

        [Preserve]
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
        [Preserve]
        public class UploadScreenshotRequest
        {
            public byte[] imageData;
        }

        [Preserve]
        public class UploadScreenshotResponse : BaseResponse<UploadScreenshotData>
        {
        }

        [Preserve]
        public class UploadScreenshotData
        {
            public string imageUrl;    // 後端儲存後的圖片 CDN URL
            public string imageId;     // 圖片在資料庫中的唯一識別碼
        }


        [Serializable]
        [Preserve]
        public class ShareActivityRequest
        {
            public string imageUrl;     // 欲分享的圖片連結
            public string description;  // 使用者輸入或自動生成的分享內容描述
        }

        [Preserve]
        public class ShareActivityResponse : BaseResponse<object>
        {
        }

        #endregion
    }
}
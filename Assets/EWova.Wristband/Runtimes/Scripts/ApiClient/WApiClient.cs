using EWova.Networking;
using EWova.Auth;
using System.Collections.Generic;

namespace EWova.Wristband
{
    public partial class WApiClient : AuthApiClient
    {
        public static string WServiceUrl
        {
            get
            {
                if (Environment.DeploymentMode is DeploymentMode.Development)
                {
                    return "https://wristbands.ewova.dev/api/v1";
                }
                else
                {
#if UNITY_EDITOR
                    Authoring.EditorLogger.Warn("你正在編輯器中使用正式環境的 API URL，請確認是否有意這麼做，避免對正式環境造成不必要的影響。可到 EWova/Editor/DeveloymentMode 切換回 Development 開發環境。");
#endif
                    return "https://wristbands.ewova.com/api/v1";
                }
            }
        }

        public WApiClient(EWova.Logger logger = null)
            : base(EwovaAuthManager.Instance, WServiceUrl, logger)
        {
            //AdditionalHeaders["x-api-key"] = apiSettings.APIKey;
        }

        protected override void CollectPackages(List<SdkPackageInfo> list)
        {
            base.CollectPackages(list);

            list.Add(new SdkPackageInfo
            {
                Name = PackageInfo.Name,
                Version = PackageInfo.Version
            });
        }
    }
}

using EWova.LearningPortfolio;
using System.Collections.Generic;

namespace EWova.Wristband
{
    internal partial class WApiClient : LPApiClient
    {
        private static WApiClient _instance;
        /// <summary>整個 App 共用的單一實例，避免每個 Wristband 都各自建立一份。</summary>
        public static WApiClient Instance => _instance ??= new WApiClient();

#if UNITY_EDITOR
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetInstanceForDomainReload()
        {
            _instance = null;
        }
#endif

        internal protected WApiClient(EWova.Logger logger = null)
            // 這邊使用學習歷程 Auth
            : base(LearningPortfolio.LearningPortfolio.EWovaAuth, logger)
        {
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

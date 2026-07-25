using EWova.LearningPortfolio;
using System.Collections.Generic;

namespace EWova.Wristband
{
    public partial class WApiClient : LPApiClient
    {
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

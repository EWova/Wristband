using TMPro;

using UnityEngine;

namespace EWova.Wristband
{
    public class GoEWovaBTN : BaseBTN
    {
        [SerializeField] private string m_label = "Switch to EWova";
        public override string Label => m_label;
        [SerializeField] private string m_description = "Switch to EWova";
        public override string Description => m_description;

        public override void ProcessClick()
        {
            string url = EWova.GetDeepLink(DeepLinkQueryInclude.Default);
            Logger.Info($"Opening EWova with URL: {url}");

            if (Application.isEditor)
                return;

            Application.OpenURL(url);
        }
    }
}

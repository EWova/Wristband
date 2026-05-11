using UnityEngine;

namespace EWova.Wristband
{
    public class GoEWovaWebsiteBTN : BaseBTN
    {
        [SerializeField] private string m_label = "Explore EWova";
        public override string Label => m_label;
        [SerializeField] private string m_description = "Explore more on EWova";
        public override string Description => m_description;

        public override void ProcessClick()
        {
            Application.OpenURL("https://ewova.com/");
        }
    }
}

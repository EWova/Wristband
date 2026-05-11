using UnityEngine;

namespace EWova.Wristband
{
    public class ShareBTN : BaseBTN
    {
        [SerializeField] private string m_label = "Share to EWova";
        public override string Label => m_label;
        [SerializeField] private string m_description = "Let others see your activity on EWova";
        public override string Description => m_description;

        public override void ProcessClick()
        {
        }
    }
}

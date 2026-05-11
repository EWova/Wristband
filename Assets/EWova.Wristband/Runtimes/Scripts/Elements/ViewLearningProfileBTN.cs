using EWova.NetService;

using UnityEngine;

namespace EWova.Wristband
{
    public class ViewLearningProfileBTN : BaseBTN
    {
        [SerializeField] private string m_label = "View Learning Profile";
        public override string Label => m_label;

        public override void ProcessClick()
        {
        }
    }
}

using UnityEngine;

namespace EWova.Wristband
{
    public class ScreenshotBTN : BaseBTN
    {
        [SerializeField] private string m_label = "Screenshot";
        public override string Label => m_label;
        [SerializeField] private string m_description = "Capture and share your moment on EWova";
        public override string Description => m_description;

        public override void ProcessClick()
        {
        }
    }
}

using EWova.NetService;

using UnityEngine;

namespace EWova.Wristband
{
    public class QuitAppBTN : BaseBTN
    {
        [SerializeField] private string m_label = "Close App";
        public override string Label => m_label;
        [SerializeField] private string m_description = "Close the app";
        public override string Description => m_description;

        public override void ProcessClick()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }
    }
}

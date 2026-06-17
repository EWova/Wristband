using Cysharp.Threading.Tasks;

using UnityEngine;

namespace EWova.Wristband
{
    public class GoEWovaWebsiteBTN : BaseBTN
    {
        [SerializeField] private string m_label = "Explore EWova";
        public override string LabelKey => m_label;
        [SerializeField] private string m_description = "Explore more on EWova";
        public override string DescriptionKey => m_description;
        protected override UniTask Load(LoadProcess loadProcess)
        {
            loadProcess.SetComplete();
            return UniTask.CompletedTask;
        }
        protected override UniTask ProcessClick()
        {
            Application.OpenURL("https://ewova.com/");
            return UniTask.CompletedTask;
        }
    }
}

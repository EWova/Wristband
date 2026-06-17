using Cysharp.Threading.Tasks;

using UnityEngine;

namespace EWova.Wristband
{
    public class ViewLearningProfileBTN : BaseBTN
    {
        [SerializeField] private string m_label = "View Learning Profile";
        public override string LabelKey => m_label;
        [SerializeField] private string m_description = "View Your Learning Profile";
        public override string DescriptionKey => m_description;

        protected override UniTask Load(LoadProcess loadProcess)
        {
            loadProcess.SetComplete();
            return UniTask.CompletedTask;
        }

        protected override UniTask ProcessClick()
        {
            return UniTask.CompletedTask;
        }
    }
}

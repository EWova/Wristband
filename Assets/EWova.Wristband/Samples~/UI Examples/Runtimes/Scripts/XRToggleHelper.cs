using Cysharp.Threading.Tasks;

using UnityEngine;

namespace EWova.Wristband.Samples.UiExamples
{
    public class XRToggleHelper : MonoBehaviour
    {
        public bool DefaultActive = true;
        public bool IsXREnableToActiveOrDeactivate = true;

        private void Start()
        {
            StartAsync().Forget();
        }
        private async UniTaskVoid StartAsync()
        {
            gameObject.SetActive(DefaultActive);

            bool isXRActive = await XRStatus.IsXRActiveAsync(cancellationToken: gameObject.GetCancellationTokenOnDestroy());

            gameObject.SetActive(isXRActive ? IsXREnableToActiveOrDeactivate : !IsXREnableToActiveOrDeactivate);
        }
    }
}

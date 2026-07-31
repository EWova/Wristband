using Cysharp.Threading.Tasks;

using UnityEngine;

namespace EWova.Wristband
{
    public class GoEWovaBTN : BaseBTN
    {
        private abstract class IState
        {
            public Sprite Sprite;
            public abstract string LabelKey { get; }
            public abstract string MainSubmitMessageKey { get; }
            public abstract string SubSubmitMessageKey { get; }
            public abstract string ConfirmationMessageKey { get; }
            public abstract EWovaDeepLinkLaunchOption EWovaDeepLinkLaunchOption { get; }
        }
        [System.Serializable]
        private class GoToEWovaState : IState
        {
            public override string LabelKey => "GoToEWova";
            public override string ConfirmationMessageKey => "GoToEWovaConfirmation";
            public override string MainSubmitMessageKey => "Confirm";
            public override string SubSubmitMessageKey => null;
            public override EWovaDeepLinkLaunchOption EWovaDeepLinkLaunchOption => EWovaDeepLinkLaunchOption.JustLaunch;
        }
        [System.Serializable]
        private class BackToEWovaState : IState
        {
            public override string LabelKey => "BackToEWova";
            public override string ConfirmationMessageKey => "BackToEWovaConfirmation";
            public override string MainSubmitMessageKey => "Confirm";
            public override string SubSubmitMessageKey => null;
            public override EWovaDeepLinkLaunchOption EWovaDeepLinkLaunchOption => EWovaDeepLinkLaunchOption.Default;
        }
        [System.Serializable]
        private class BackToEWovaSpaceState : IState
        {
            public override string LabelKey => "BackToEWovaSpace";
            public override string ConfirmationMessageKey => "BackToEWovaSpaceConfirmation";
            public override string MainSubmitMessageKey => "BackToEWovaSpaceConfirm";
            public override string SubSubmitMessageKey => "JustLaunchEWovaConfirm";
            public override EWovaDeepLinkLaunchOption EWovaDeepLinkLaunchOption => EWovaDeepLinkLaunchOption.Default;
        }

        public override string LabelKey => (CurrentState ?? _goToEWovaState).LabelKey;
        public override string FeatureKey => "GO_TO_EWOVA";

        [SerializeField] private GoToEWovaState _goToEWovaState = new GoToEWovaState();
        [SerializeField] private BackToEWovaState _backToEWovaState = new BackToEWovaState();
        [SerializeField] private BackToEWovaSpaceState _backToEWovaSpaceState = new BackToEWovaSpaceState();

        private IState CurrentState;

        public int TestValue = 0;

        protected override UniTask Load(LoadProcess loadProcess)
        {
            base.Load(loadProcess);
            loadProcess.SetComplete();

            RefreshState();

            return UniTask.CompletedTask;
        }

        protected override void UpdateBaseState()
        {
            base.UpdateBaseState();
            RefreshState();
        }
        private void RefreshState()
        {
            var invokeContext = EWovaApp.InvocationContext;

            bool wasInEWova = invokeContext != null;
            bool wasInEWovaSpace = invokeContext != null
                && invokeContext.WorldGuid != null && invokeContext.SpaceInstanceIndex != null;

            IState nextState;

            if (wasInEWovaSpace)
                nextState = _backToEWovaSpaceState;
            else if (wasInEWova)
                nextState = _backToEWovaState;
            else
                nextState = _goToEWovaState;

            if (CurrentState == nextState)
                return;

            CurrentState = nextState;
            CircleButtonElement.LabelTMP.text = GetLocalizedString(CurrentState.LabelKey);
            CircleButtonElement.Image.sprite = CurrentState.Sprite;
        }

        protected override async UniTask ProcessClick()
        {
            string confirmationMessageKey = GetLocalizedString(CurrentState.ConfirmationMessageKey);
            string submitLabel = GetLocalizedString(CurrentState.MainSubmitMessageKey);
            string subSubmitLabel = CurrentState.SubSubmitMessageKey == null ? null : GetLocalizedString(CurrentState.SubSubmitMessageKey);

            bool confirmed = await WristbandController.AlertUI.OpenAsync(new AlertUI.AlertData
            {
                Message = confirmationMessageKey,
                MainSubmitMessage = submitLabel,
                SubSubmitMessage = subSubmitLabel,
            });

            if (!confirmed)
                return;

            var option = CurrentState.EWovaDeepLinkLaunchOption;

            string url = await EWovaApp.GetDeepLink(option, LearningPortfolio.LearningPortfolio.EWovaAuth);
            if (Logger.InfoEnabled)
                Logger.Info($"Opening EWova with URL: {url}");

            if (!Application.isEditor)
                Application.OpenURL(url);
        }
    }
}

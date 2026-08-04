using Cysharp.Threading.Tasks;

using EWova.LearningPortfolio;

using System.Threading;

using UnityEngine;

namespace EWova.Wristband
{
    public class ViewLearningProfileBTN : BaseBTN
    {
        private abstract class IState
        {
            public Sprite Sprite;
            public abstract string LabelKey { get; }
        }
        [System.Serializable]
        private class LoginState : IState
        {
            public override string LabelKey => "Login";
        }
        [System.Serializable]
        private class ViewProfileState : IState
        {
            public override string LabelKey => "ViewLearningProfile";
        }

        public override string LabelKey => (CurrentState ?? _loginState).LabelKey;
        public override string FeatureKey => "VIEW_LEARNING_PROFILE";

        [SerializeField] private LoginState _loginState = new LoginState();
        [SerializeField] private ViewProfileState _viewProfileState = new ViewProfileState();

        private IState CurrentState;

        private void OnEnable()
        {
            LearningPortfolio.LearningPortfolio.OnUserLogin += OnLPUserLogin;
            LearningPortfolio.LearningPortfolio.OnUserLogout += OnLPUserLogout;
        }

        private void OnDisable()
        {
            LearningPortfolio.LearningPortfolio.OnUserLogin -= OnLPUserLogin;
            LearningPortfolio.LearningPortfolio.OnUserLogout -= OnLPUserLogout;
        }

        private void OnLPUserLogin(LearningPortfolio.LearningPortfolio.UserData _) => SyncState();
        private void OnLPUserLogout() => SyncState();

        protected override void Update()
        {
            base.Update();

            if (_process != null)
            {
                CircleButtonElement.Progress = _process.Progress;
                if (_process.IsCompleted)
                    _process = null;
            }
        }

        protected override void UpdateBaseState()
        {
            base.UpdateBaseState();
            RefreshState();
        }

        private void RefreshState()
        {
            bool isConnected = LearningPortfolio.LearningPortfolio.IsConnected;
            IState nextState = isConnected ? (IState)_viewProfileState : _loginState;

            if (isConnected)
                CircleButtonElement.Progress = 1.0f;

            if (CurrentState == nextState)
                return;

            CurrentState = nextState;
            CircleButtonElement.LabelTMP.text = GetLocalizedString(CurrentState.LabelKey);
            CircleButtonElement.Image.sprite = CurrentState.Sprite;
        }

        protected override UniTask Load(LoadProcess loadProcess)
        {
            base.Load(loadProcess);
            loadProcess.SetComplete();

            RefreshState();

            return UniTask.CompletedTask;
        }

        ConnectProcess _process = null;
        CancellationTokenSource _connectSource = null;
        protected override async UniTask ProcessClick()
        {
            if (LearningPortfolio.LearningPortfolio.IsConnected)
            {
                SyncState();
                LearningPortfolio.LearningPortfolio.CreateUserProjectRecordShower(WristbandController.LearningPortfolioFrame);
                return;
            }

            (bool blocked, string msg) = LearningPortfolio.LearningPortfolio.IsConnectBlockedByCustomLogic;
            if (blocked)
            {
                await WristbandController.AlertUI.OpenAsync(new AlertUI.AlertData
                {
                    Message = msg,
                    SubSubmitMessage = GetLocalizedString("Confirm"),
                });
                return;
            }

            SubmitResult needConnect = await WristbandController.AlertUI.OpenAsync(new AlertUI.AlertData
            {
                Message = GetLocalizedString("LearningPortfolioNotConnected"),
                MainSubmitMessage = GetLocalizedString("LearningPortfolioConfirm"),
            });

            if (!needConnect)
                return;

            _process = new ConnectProcess();
            _connectSource = new CancellationTokenSource();
            try
            {
                CancellationToken token = CancellationTokenSource.CreateLinkedTokenSource(_connectSource.Token, destroyCancellationToken).Token;

                var process = _process;
                await LearningPortfolio.LearningPortfolio.ConnectAsync(process, token);

                if (!process.IsSuccess)
                {
                    if (Logger.ErrorEnabled)
                        Logger.Err($"Login failed: {process.ClientErrorMessage} {process.ServerErrorMessage}");
                    return;
                }
            }
            catch (System.Exception ex)
            {
                if (Logger.ErrorEnabled)
                    Logger.Err($"Login failed: {ex}");
                return;
            }
            finally
            {
                _connectSource.Cancel();
                _connectSource.Dispose();
                _process = null;
            }

            if (Logger.InfoEnabled)
                Logger.Info($"Login success");

            ResyncAllBTNState();
        }
    }
}

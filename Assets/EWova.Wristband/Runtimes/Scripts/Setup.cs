using System;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using UnityEngine;

namespace EWova.Wristband
{
    [Flags]
    public enum WristbandFeatureFlags
    {
        None = 0,
        GoToEWova = 1 << 0,
        CaptureToEWova = 1 << 1,
        ShareToEWova = 1 << 2,
        ExploreWebsite = 1 << 3,
        QuitApp = 1 << 4,
        LearningProfile = 1 << 5,
    }

    [RequireComponent(typeof(Wristband))]
    public class Setup : MonoBehaviour
    {
        private WristbandFeatureFlags m_fallbackFeatures =
            WristbandFeatureFlags.GoToEWova
            | WristbandFeatureFlags.ExploreWebsite
            | WristbandFeatureFlags.QuitApp
            | WristbandFeatureFlags.LearningProfile;

        private void Start()
        {
            Logger.PrintLevel = LogLevel.Warn | LogLevel.Error;
            FetchFeaturesAsync().Forget();
        }

        private async UniTaskVoid FetchFeaturesAsync()
        {
            var wristband = GetComponent<Wristband>();
            wristband.Interactable = false;

            try
            {
                IProgress<float> progress = new Progress<float>(p =>
                {
                    wristband.SetCircleController(MainMenuCircleControllerFactory.UpdateDirectly(1.0f - p));
                });
                var response = await wristband.ApiClient.GetFeaturesCachedAsync(progress, destroyCancellationToken);
                wristband.LoadFeatures(response.data.features);
                wristband.Interactable = true;
            }
            catch (Exception ex)
            {
                if (Logger.ErrorEnabled)
                    Logger.Err("Failed to fetch wristband features. Falling back to default features.");

                if (ex is not OperationCanceledException)
                    Debug.LogException(ex);

                wristband.LoadFeatures(ToFeatureStates(m_fallbackFeatures));
                wristband.Interactable = true;
            }
        }

        private static readonly Dictionary<WristbandFeatureFlags, string> _flagKeys = new()
        {
            { WristbandFeatureFlags.GoToEWova,      "GO_TO_EWOVA" },
            { WristbandFeatureFlags.CaptureToEWova, "CAPTURE_TO_EWOVA" },
            { WristbandFeatureFlags.ShareToEWova,   "SHARE_TO_EWOVA" },
            { WristbandFeatureFlags.ExploreWebsite, "EXPLORE_EWOVA_WEBSITE" },
            { WristbandFeatureFlags.QuitApp,        "QUIT_APP" },
            { WristbandFeatureFlags.LearningProfile, "VIEW_LEARNING_PROFILE" },
        };

        private static ApiModels.Feature[] ToFeatureStates(WristbandFeatureFlags flags)
        {
            var result = new List<ApiModels.Feature>();
            foreach (var kv in _flagKeys)
            {
                result.Add(new ApiModels.Feature
                {
                    key = kv.Value,
                    visible = flags.HasFlag(kv.Key),
                    enabled = flags.HasFlag(kv.Key),
                });
            }
            return result.ToArray();
        }
    }
}

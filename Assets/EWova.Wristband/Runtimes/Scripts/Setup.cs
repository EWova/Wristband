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

#if UNITY_EDITOR
        [SerializeField] private bool m_editorOfflineMode = false;
        [SerializeField] private WristbandFeatureFlags m_editorTestFeatures = WristbandFeatureFlags.GoToEWova | WristbandFeatureFlags.CaptureToEWova | WristbandFeatureFlags.ShareToEWova | WristbandFeatureFlags.ExploreWebsite | WristbandFeatureFlags.QuitApp;
#endif

        private void Start()
        {
            Logger.PrintLevel = LogLevel.Warn | LogLevel.Error;
            FetchFeaturesAsync().Forget();
        }

        private async UniTaskVoid FetchFeaturesAsync()
        {
            var wristband = GetComponent<Wristband>();

#if UNITY_EDITOR
            if (m_editorOfflineMode)
            {
                if (Logger.InfoEnabled)
                    Logger.Info("[Editor] Offline mode enabled. Using editor test features.");
                wristband.LoadFeatures(ToFeatureStates(m_editorTestFeatures));
                return;
            }
#endif

            try
            {
                var response = await wristband.ApiClient.GetFeaturesCachedAsync(destroyCancellationToken);
                wristband.LoadFeatures(response.data.features);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                if (Logger.ErrorEnabled)
                    Logger.Err("Failed to fetch wristband features. Falling back to default features.");
                Debug.LogException(ex);
                wristband.LoadFeatures(ToFeatureStates(m_fallbackFeatures));
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

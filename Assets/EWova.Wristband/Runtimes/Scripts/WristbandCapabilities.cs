using System.Collections.Generic;

namespace EWova.Wristband
{
    internal static class WristbandCapabilities
    {
        internal static readonly HashSet<string> Supported = new(System.StringComparer.OrdinalIgnoreCase)
        {
            "GO_TO_EWOVA",
            "QUIT_APP",
            "EXPLORE_EWOVA_WEBSITE",

#if EWOVA_LEARNING_PORTFOLIO
            "CAPTURE_TO_EWOVA",
            "SHARE_TO_EWOVA",
            "VIEW_LEARNING_PROFILE",
#endif
        };
    }
}

using System.Collections.Generic;

namespace EWova.Wristband
{
    internal static class WristbandCapabilities
    {
        internal static readonly HashSet<string> Supported = new(System.StringComparer.OrdinalIgnoreCase)
        {
            "GO_TO_EWOVA",
            "CAPTURE_TO_EWOVA",
            "SHARE_TO_EWOVA",
            "EXPLORE_EWOVA_WEBSITE",
            "QUIT_APP",

#if EWOVA_LEARNING_PORTFOLIO
            "VIEW_LEARNING_PROFILE",
#endif
        };
    }
}

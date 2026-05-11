namespace EWova.Wristband
{
    internal static class Logger
    {
        private static Debug _debug = new Debug(
            prefix: "[EWova.Wristband] ",
            printLevel: /*Debug.Level.Info |*/ Debug.Level.Warn | Debug.Level.Error
            );


        internal static void Info(string message) => _debug.Log(message);
        internal static void Warn(string message) => _debug.LogWarning(message);
        internal static void Error(string message) => _debug.LogError(message);
    }
}

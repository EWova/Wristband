using UnityEngine;

namespace EWova.Wristband
{
    internal static class Logger
    {
        private static global::EWova.Logger _logger = new global::EWova.Logger(
            prefix: "[EWova]Wristband ",
            printLevel: LogLevel.Full
            );

        internal static LogLevel PrintLevel
        {
            get => _logger.PrintLevel;
            set => _logger.PrintLevel = value;
        }

        [HideInCallstack]
        internal static void Info(string message)
        {
            _logger.Info(message);
        }

        [HideInCallstack]
        internal static void Warn(string message)
        {
            _logger.Warn(message);
        }

        [HideInCallstack]
        internal static void Err(string message)
        {
            _logger.Err(message);
        }
    }
}

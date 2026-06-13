using BepInEx.Logging;

namespace E2EApi
{
    /// <summary>API-internal logging through BepInEx.</summary>
    internal static class Log
    {
        private static ManualLogSource _source;

        private static ManualLogSource Source
        {
            get
            {
                if (_source == null)
                {
                    _source = Logger.CreateLogSource(E2EApiInfo.Name);
                }
                return _source;
            }
        }

        public static void Info(string message) => Source.LogInfo(message);
        public static void Warn(string message) => Source.LogWarning(message);
        public static void Error(string message) => Source.LogError(message);
        public static void Debug(string message) => Source.LogDebug(message);
    }
}

using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

using UnityEngine;


namespace WeArt.Utils
{
    /// <summary>
    /// Utility class used to log events and messages in the <see cref="WeArt"/> framework
    /// </summary>
    public static class WeArtLog
    {
        /// <summary>Logs a message in the debug console</summary>
        /// <param name="message">The string message</param>
        /// <param name="logType">The kind of log</param>
        /// <param name="onlyInDevelopmentBuild">True if this log should be ignored in normal builds</param>
        /// <param name="callerPath">The path of the caller (optional)</param>
        public static void Log(
            object message,
            LogType logType = LogType.Log,
            bool onlyInDevelopmentBuild = false,
            [CallerFilePath] string callerPath = "")
        {
            if (onlyInDevelopmentBuild && Debug.isDebugBuild)
                return;

            string logMessage = $"{PathToContextString(callerPath)}: {message}";

            if (logType == LogType.Log)
                Debug.Log(logMessage);

            else if (logType == LogType.Warning)
                Debug.LogWarning(logMessage);

            else if (logType == LogType.Error || logType == LogType.Exception)
                Debug.LogError(logMessage);

            else if (logType == LogType.Assert)
                Debug.LogAssertion(logMessage);
        }


        private static readonly Dictionary<string, string> PathToContext = new Dictionary<string, string>();

        private static string PathToContextString(string path)
        {
            if (PathToContext.TryGetValue(path, out string context))
                return context;

            context = Path.GetFileNameWithoutExtension(path);
            context = $"<b>[{context}]</b>";

            PathToContext[path] = context;
            return context;
        }
    }
}
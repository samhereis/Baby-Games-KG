using Helpers;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Loggers
{
    [Serializable]
    public class CustomLogger : MonoBehaviour
    {
        public static CustomLogger instance;

        private void Awake()
        {
            instance = this;
        }

        [field: SerializeField] public bool enableLogs { get; private set; } = true;

        public void Log(string prefix, string message, Object context = null, LogTypes logType = LogTypes.General, Color? color = null)
        {
            if (enableLogs == false) { return; }
            if (color == null) { color = Color.white; }

            string output = message;
            if (string.IsNullOrEmpty(prefix) == false) { output = $"{prefix.Colorize(color)}: {output}"; }
            output = $"{logType.ToString().Colorize(color)} - {output}";

            Debug.Log(output, context);
        }

        public void LogWarning(string prefix, string message, Object context, LogTypes logType, Color? color = null)
        {
            if (enableLogs == false) { return; }
            if (color == null) { color = Color.yellow; }

            string output = message;
            if (string.IsNullOrEmpty(prefix) == false) { output = $"{prefix.Colorize(color)}: {output}"; }
            output = $"{logType.ToString().Colorize(color)} - {output}";

            Debug.LogWarning(output, context);
        }

        public void LogError(string prefix, string message, Object context, LogTypes logType, Color? color = null)
        {
            if (enableLogs == false) { return; }
            if (color == null) { color = Color.red; }

            string output = message;
            if (string.IsNullOrEmpty(prefix) == false) { output = $"{prefix.Colorize(color)}: {output}"; }
            output = $"{logType.ToString().Colorize(color)} - {output}";

            Debug.LogError(output, context);
        }

        public void LogException(Exception exception, string prefix = null, Object context = null, LogTypes logType = LogTypes.General)
        {
            if (prefix != null)
            {
                Debug.LogException(new Exception(prefix, exception), context);
            }
            else
            {
                Debug.LogException(exception, context);
            }
        }
    }
}
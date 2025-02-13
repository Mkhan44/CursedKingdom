using PurrNet.Modules;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace PurrNet.Logging
{
    public static class PurrLogger
    {
        [UsedByIL]
        public static void LogSimpleError(string message, Object reference)
        {
            Debug.LogError(message, reference);
        }

        public static void Log(string message, Object reference = null, LogStyle logStyle = default, [CallerFilePath] string filePath = "")
        {
            LogMessage(message, reference, logStyle, LogType.Log, filePath);
        }

        public static void LogWarning(string message, Object reference = null, LogStyle logStyle = default, [CallerFilePath] string filePath = "")
        {
            LogMessage(message, reference, logStyle, LogType.Warning, filePath);
        }

        public static void LogError(string message, Object reference = null, LogStyle logStyle = default, [CallerFilePath] string filePath = "")
        {
            LogMessage(message, reference, logStyle, LogType.Error, filePath);
        }

        public static void LogException(string message, Object reference = null, LogStyle logStyle = default, [CallerFilePath] string filePath = "")
        {
            LogMessage(message, reference, logStyle, LogType.Exception, filePath);
        }

        private static void LogMessage(string message, Object reference, LogStyle logStyle, LogType logType, string filePath)
        {
            string formattedMessage = FormatMessage_Internal(message, logStyle, filePath);

            switch (logType)
            {
                case LogType.Log:
                    Debug.Log(formattedMessage, reference);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(formattedMessage, reference);
                    break;
                case LogType.Error:
                    Debug.LogError(formattedMessage, reference);
                    break;
                case LogType.Exception:
                    Debug.LogException(new Exception(formattedMessage), reference);
                    break;
            }
        }
        
        public static string FormatMessage(string message, LogStyle logStyle = default, [CallerFilePath] string filePath = "")
        {
            return FormatMessage_Internal(message, logStyle, filePath);
        }
        
        public static void Throw<T>(string message, LogStyle logStyle = default, [CallerFilePath] string filePath = "") where T : Exception
        {
            string formattedMessage = FormatMessage_Internal(message, logStyle, filePath);
            throw (T)Activator.CreateInstance(typeof(T), formattedMessage);
        }

        private static string FormatMessage_Internal(string message, LogStyle logStyle, string filePath)
        {
            string fileName = System.IO.Path.GetFileName(filePath).Replace(".cs", "");
            
            var prefix = logStyle.headerColor.HasValue ? $"<color=#{ColorUtility.ToHtmlStringRGB(logStyle.headerColor.Value)}>[{fileName}]</color>" :
                $"[{fileName}]";
            
            var text = logStyle.textColor.HasValue ? $"<color=#{ColorUtility.ToHtmlStringRGB(logStyle.textColor.Value)}>{message}</color>" :
                message;
            
            return $"{prefix} {text}";
        }
    }

    public readonly struct LogStyle
    {
        public Color? headerColor { get; }

        public Color? textColor { get; }

        public LogStyle(Color? headerColor = default, Color? textColor = default)
        {
            this.headerColor = headerColor;
            this.textColor = textColor;
        }
    }
}

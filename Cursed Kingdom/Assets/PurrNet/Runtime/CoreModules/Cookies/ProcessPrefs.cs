using System.Collections.Generic;

namespace PurrNet.Modules
{
    internal static class ProcessPrefs
    {
        static readonly Dictionary<string, string> _prefs = new Dictionary<string, string>();

#if UNITY_EDITOR
        public static void Set(string key, string value)
        {
            UnityEditor.SessionState.SetString(key, value);
        }

        public static string Get(string key, string defaultValue = "")
        {
            return UnityEditor.SessionState.GetString(key, defaultValue);
        }

        public static void Unset(string key)
        {
            UnityEditor.SessionState.EraseString(key);
        }
#else
        public static void Set(string key, string value)
        {
            _prefs[key] = value;
        }

        public static string Get(string key, string defaultValue = "")
        {
            return !_prefs.TryGetValue(key, out string value) ? defaultValue : value;
        }

        public static void Unset(string key)
        {
            _prefs.Remove(key);
        }
#endif
    }
}
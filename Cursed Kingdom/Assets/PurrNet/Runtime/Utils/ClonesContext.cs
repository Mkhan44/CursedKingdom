using System.IO;

namespace PurrNet.Utils
{
    public static class ClonesContext
    {
#if UNITY_EDITOR
        private static bool? _cachedIsClone;
#endif

        public static bool isClone
        {
            get
            {
#if !UNITY_EDITOR
                return false;
#else
                if (_cachedIsClone.HasValue)
                    return _cachedIsClone.Value;

                var assetsFolder = new DirectoryInfo("Assets");
                var isClone = assetsFolder.Attributes.HasFlag(FileAttributes.ReparsePoint);

                _cachedIsClone = isClone;
                return isClone;
#endif
            }
        }
    }
}
using UnityEditor;
using UnityEngine;

namespace PurrNet.Editor
{
    public static class PurrNetManagerCreator
    {
        [MenuItem("GameObject/PurrNet/NetworkManager", false, 2)]
        public static void CreateManager()
        {
            var obj = new GameObject("PurrNet");
            obj.AddComponent<NetworkManager>();
        }
    }
}

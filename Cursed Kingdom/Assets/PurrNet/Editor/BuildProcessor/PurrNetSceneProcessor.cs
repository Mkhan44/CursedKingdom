using System.IO;
using PurrNet.Utils;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PurrNet.Editor
{
    public class PurrNetSceneProcessor : IProcessSceneWithReport, IPreprocessBuildWithReport,
        IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report)
        {
            const string PATH = "Assets/Resources/PurrHashes.json";

            if (File.Exists(PATH))
                File.Delete(PATH);

            if (File.Exists(PATH + ".meta"))
                File.Delete(PATH + ".meta");

            if (Directory.Exists("Assets/Resources"))
            {
                bool isResourcesFolderEmpty = Directory.GetFiles("Assets/Resources").Length == 0 &&
                                              Directory.GetDirectories("Assets/Resources").Length == 0;

                if (isResourcesFolderEmpty)
                {
                    Directory.Delete("Assets/Resources");
                    if (File.Exists("Assets/Resources.meta"))
                        File.Delete("Assets/Resources.meta");
                }
            }

            AssetDatabase.Refresh();
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            Hasher.ClearState();
            NetworkManager.CallAllRegisters();

            const string PATH = "Assets/Resources/PurrHashes.json";
            Directory.CreateDirectory(Path.GetDirectoryName(PATH) ?? string.Empty);

            var hashes = Hasher.GetAllHashesAsText();
            File.WriteAllText(PATH, hashes);

            AssetDatabase.Refresh();
        }

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            var rootObjects = scene.GetRootGameObjects();
            var obj = new GameObject("PurrNetSceneHelper");

            if (report == null)
                obj.hideFlags = HideFlags.HideAndDontSave;

            var sceneInfo = obj.AddComponent<PurrSceneInfo>();
            sceneInfo.rootGameObjects = new System.Collections.Generic.List<GameObject>();

            for (uint i = 0; i < rootObjects.Length; i++)
            {
                sceneInfo.rootGameObjects.Add(rootObjects[i]);
            }
        }
    }
}

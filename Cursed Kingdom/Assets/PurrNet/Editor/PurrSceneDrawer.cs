using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PurrNet
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(PurrSceneAttribute))]
    public class PurrSceneDrawer : PropertyDrawer
    {
        private static Dictionary<string, SceneAsset> sceneCache = new Dictionary<string, SceneAsset>();
        private static double lastCacheUpdate;
        private const double CACHE_LIFETIME = 30.0;

        private static void UpdateSceneCache()
        {
            double currentTime = EditorApplication.timeSinceStartup;
            if (currentTime - lastCacheUpdate < CACHE_LIFETIME && sceneCache.Count > 0)
            {
                return;
            }

            sceneCache.Clear();
            string[] sceneGuids = AssetDatabase.FindAssets("t:SceneAsset");

            foreach (string guid in sceneGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                if (scene != null && !sceneCache.ContainsKey(scene.name))
                {
                    sceneCache[scene.name] = scene;
                }
            }

            lastCacheUpdate = currentTime;
        }

        private static SceneAsset GetSceneAssetFromName(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
                return null;

            UpdateSceneCache();
            return sceneCache.TryGetValue(sceneName, out SceneAsset scene) ? scene : null;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "PurrScene attribute can only be used with string fields");
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            SceneAsset sceneObj = GetSceneAssetFromName(property.stringValue);

            EditorGUI.BeginChangeCheck();
            var newScene = EditorGUI.ObjectField(
                position,
                label,
                sceneObj,
                typeof(SceneAsset),
                false
            ) as SceneAsset;

            if (EditorGUI.EndChangeCheck())
            {
                property.stringValue = newScene != null ? newScene.name : "";
            }

            EditorGUI.EndProperty();
        }
    }
#endif
}
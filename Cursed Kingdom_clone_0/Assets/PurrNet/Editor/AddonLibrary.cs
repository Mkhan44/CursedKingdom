using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;
using PurrNet.Logging;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace PurrNet.Editor
{
    public class AddonLibrary : EditorWindow
    {
        private static readonly List<Addon> _addons = new List<Addon>();
        private static readonly List<Addon> _exampleAddons = new List<Addon>();
        private static readonly List<Addon> _transportAddons = new List<Addon>();
        private static readonly List<Addon> _toolAddons = new List<Addon>();
        private static readonly List<Addon> _systemAddons = new List<Addon>();
        private static readonly List<UnityWebRequest> _imageRequests = new List<UnityWebRequest>();

        private static bool _fetchedAddons;
        private UnityWebRequest _request;
        private Vector2 scrollViewPosition;
        private Texture2D defaultIcon;
        private int selectedTab = 0;

        private const int imageWidth = 100;
        private const int sectionOneWidth = 250;

        private GUIStyle wrapStyle;
        
        [MenuItem("Tools/PurrNet/Addon Library")]
        public static void ShowWindow()
        {
            _fetchedAddons = false;
            _imageRequests.Clear();

            var window = GetWindow<AddonLibrary>("PurrNet Addon Library");
            window.minSize = new Vector2(350, 300);
            window.LoadDefaultIcon();
        }

        private void OnGUI()
        {
            wrapStyle = new GUIStyle(GUI.skin.label);
            wrapStyle.wordWrap = true;
            if (_addons.Count <= 0 && !_fetchedAddons)
            {
                HandleGettingAddons();
                HandleWaiting("Populating addons...");
                return;
            }

            foreach (var imageRequest in _imageRequests)
            {
                if (!imageRequest.isDone)
                {
                    if (imageRequest.result == UnityWebRequest.Result.ConnectionError || imageRequest.result == UnityWebRequest.Result.ProtocolError)
                    {
                        HandleError(imageRequest.result.ToString());
                        return;
                    }
                }
            }

            foreach (var request in _imageRequests)
            {
                if (!request.isDone)
                {
                    HandleWaiting("Loading images...");
                    return;
                }
            }
            
            List<string> availableTabs = new List<string>();

            availableTabs.Add("All");
            if (_exampleAddons.Count > 0) availableTabs.Add("Examples");
            if (_transportAddons.Count > 0) availableTabs.Add("Transports");
            if (_toolAddons.Count > 0) availableTabs.Add("Tools");
            if (_systemAddons.Count > 0) availableTabs.Add("Systems");

            selectedTab = GUILayout.Toolbar(selectedTab, availableTabs.ToArray());

            scrollViewPosition = EditorGUILayout.BeginScrollView(scrollViewPosition);

            //Debug.Log($"example count: {_exampleAddons.Count} | transport count: {_transportAddons.Count} | tool count: {_toolAddons.Count} | system count: {_systemAddons.Count} | all count: {_addons.Count}");
            switch (availableTabs[selectedTab])
            {
                case "All":
                    HandleAddons(_addons);
                    break;
                case "Examples":
                    HandleAddons(_exampleAddons);
                    break;
                case "Transports":
                    HandleAddons(_transportAddons);
                    break;
                case "Tools":
                    HandleAddons(_toolAddons);
                    break;
                case "Systems":
                    HandleAddons(_systemAddons);
                    break;
            }

            EditorGUILayout.EndScrollView();
        }

        private void LoadDefaultIcon()
        {
            string scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            string directory = System.IO.Path.GetDirectoryName(scriptPath);
            string relativePath = System.IO.Path.Combine(directory, "Editor Default Resources", "Pebbles.png");
            defaultIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(relativePath);
        }

        private void HandleGettingAddons()
        {
            if (_request == null)
            {
                _request = UnityWebRequest.Get("https://pebblesgames.com/wp-content/PurrNet/PurrNetAddons.json");
                _request.SendWebRequest();
            }
            else if (_request.isDone)
            {
                if (_request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(_request.error);
                }
                else
                {
                    string json = _request.downloadHandler.text;
                    AddonsWrapper wrapper = JsonUtility.FromJson<AddonsWrapper>(json);
                    if (wrapper == null || wrapper.addons == null)
                    {
                        HandleError("Failed to parse JSON");
                        return;
                    }

                    if (wrapper.addons.Count <= 0)
                    {
                        HandleError("No addons found");
                        return;
                    }

                    _addons.Clear();

                    foreach (var addon in wrapper.addons)
                    {
                        var imageRequest = UnityWebRequestTexture.GetTexture(addon.imageUrl); 
                        imageRequest.SendWebRequest();
                        _imageRequests.Add(imageRequest);
                        addon.icon = defaultIcon;
                        _addons.Add(addon);

                        string category = addon.category.ToLower();

                        if (category.Contains("example"))
                            _exampleAddons.Add(addon);
                        if (category.Contains("transport"))
                            _transportAddons.Add(addon);
                        if (category.Contains("tool"))
                            _toolAddons.Add(addon);
                        if (category.Contains("system"))
                            _systemAddons.Add(addon);
                    }

                    _fetchedAddons = true;
                }
            }
        }

        private void HandleAddons(List<Addon> addonsToHandle)
        {
            int columns = Mathf.Max(1, Mathf.FloorToInt(position.width / (imageWidth + sectionOneWidth + 20)));
            float cardWidth = position.width / columns - 10;

            for (int i = 0; i < addonsToHandle.Count; i += columns)
            {
                EditorGUILayout.BeginHorizontal();

                for (int j = 0; j < columns; j++)
                {
                    if (i + j < addonsToHandle.Count)
                    {
                        var addon = addonsToHandle[i + j];
                        GUIStyle centerTitle = new GUIStyle(GUI.skin.label)
                        {
                            alignment = TextAnchor.MiddleCenter,
                            fontStyle = FontStyle.Bold
                        };
                        GUIStyle centerDescription = new GUIStyle(GUI.skin.label)
                        {
                            alignment = TextAnchor.MiddleCenter,
                            wordWrap = true
                        };

                        GUIStyle boxStyle = new GUIStyle("box") { alignment = TextAnchor.MiddleCenter };

                        EditorGUILayout.BeginVertical(boxStyle, GUILayout.Width(cardWidth));

                        if (columns == 1 || position.width < 500)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            GUILayout.Label(addon.icon, GUILayout.Width(imageWidth), GUILayout.Height(imageWidth));
                            GUILayout.FlexibleSpace();
                            GUILayout.EndHorizontal();

                            GUILayout.Label(addon.name, centerTitle);
                            GUILayout.Label(addon.description, centerDescription, GUILayout.Width(cardWidth - 20));
                        }
                        else
                        {
                            EditorGUILayout.BeginHorizontal();

                            GUILayout.Label(addon.icon, GUILayout.Width(imageWidth), GUILayout.Height(imageWidth));

                            EditorGUILayout.BeginVertical();
                            GUILayout.Label(addon.name, EditorStyles.boldLabel);
                            GUILayout.Label(addon.description, wrapStyle);
                            GUILayout.Label($"Version: {addon.version}");
                            GUILayout.Label($"Author: {addon.author}");
                            EditorGUILayout.EndVertical();

                            EditorGUILayout.EndHorizontal();
                        }

                        HandleInstallButton(addon);

                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space(10);
                    }
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(10);
            }
        }

        private void HandleInstallButton(Addon addon)
        {
            if (!ExistsInProject(addon))
            {
                if (GUILayout.Button("Install"))
                    AddAddon(addon);
                return;
            }
            
            if (addon.asManifest)
            {
                GUIStyle redButtonStyle = new GUIStyle(GUI.skin.button);
                redButtonStyle.normal.textColor = Color.red;
                if (GUILayout.Button("Remove", redButtonStyle))
                    RemoveFromManifest(addon);
            }
            else
            {
                GUI.enabled = false;
                if (GUILayout.Button("Already installed"))
                    AddAddon(addon);
                GUI.enabled = true;
            }
        }

        private void AddAddon(Addon addon)
        {
            // Implement the logic to update the manifest file with the given gitUrl
            if (addon.asManifest)
                AddAddon_Manifest(addon);
            else
                AddAddon_Assets(addon);
        }

        private void HandleWaiting(string message)
        {
            GUILayout.BeginVertical("Box");
            GUILayout.Label(message);
            GUILayout.EndVertical();
        }
        
        private void HandleError(string error)
        {
            EditorGUILayout.HelpBox("Failed to fetch addons: " + error, MessageType.Error);
        }

        private bool ExistsInProject(Addon addon)
        {
            if (addon.asManifest)
            {
                string manifestPath = "Packages/manifest.json";
                var manifest = JObject.Parse(File.ReadAllText(manifestPath));
                var dependencies = manifest["dependencies"] as JObject;

                string parsedName = "com.purrnet." + addon.name.Replace(" ", "").ToLower();
                return dependencies.ContainsKey(parsedName) && dependencies[parsedName].ToString() == addon.projectUrl;
            }

            return false;
        }
        
        private void RemoveFromManifest(Addon addon)
        {
            string manifestPath = "Packages/manifest.json";
            var manifest = JObject.Parse(File.ReadAllText(manifestPath));
            var dependencies = manifest["dependencies"] as JObject;

            string parsedName = "com.purrnet." + addon.name.Replace(" ", "").ToLower();
    
            if (dependencies.ContainsKey(parsedName))
            {
                dependencies.Remove(parsedName);
                File.WriteAllText(manifestPath, manifest.ToString());
                AssetDatabase.Refresh(); 
            }
        }
        
        private void AddAddon_Manifest(Addon addon)
        {
            string manifestPath = "Packages/manifest.json";
            var manifest = JObject.Parse(File.ReadAllText(manifestPath));
            var dependencies = manifest["dependencies"] as JObject;

            string parsedName = addon.name.Replace(" ", "").ToLower();
            dependencies["com.purrnet."+parsedName] = addon.projectUrl;
            File.WriteAllText(manifestPath, manifest.ToString());
            AssetDatabase.Refresh();
        }
        
        private void AddAddon_Assets(Addon addon)
        {
            string parsedName = addon.name.Replace(" ", "").ToLower();
            string tempPath = Path.Combine(Path.GetTempPath(), parsedName + ".unitypackage");

            using (WebClient client = new WebClient())
            {
                client.DownloadFile(addon.projectUrl, tempPath);
            }

            if (File.Exists(tempPath))
            {
                AssetDatabase.ImportPackage(tempPath, true);
                File.Delete(tempPath);
                
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets");
            }
            else
            {
                PurrLogger.LogError($"Couldn't get the {addon.name} package and install it or delete the temp file."); 
            }
        }

        [System.Serializable]
        private class Addon
        {
            public string name;
            public string description;
            public string version;
            public string author;
            public bool asManifest;
            public string projectUrl;
            public string category;
            public string imageUrl;
            public Texture2D icon;
        }

        [System.Serializable]
        private class AddonsWrapper
        {
            public List<Addon> addons;
        }
    }
}

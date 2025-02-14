using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using PurrNet.Logging;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using PurrNet.Utils;
using UnityEditor;
#endif

namespace PurrNet
{
    [CreateAssetMenu(fileName = "NetworkPrefabs", menuName = "PurrNet/Network Prefabs", order = -201)]
    public class NetworkPrefabs : PrefabProviderScriptable
    {
        public bool autoGenerate = true;
        public bool networkOnly = true;
        public bool poolByDefault;
        public Object folder;
        public List<PrefabData> prefabs = new List<PrefabData>();
        
        [Serializable]
        public struct PrefabData
        {
            public GameObject prefab;
            public bool pooled;
            public int warmupCount;
        }

        public override IReadOnlyList<PrefabData> allPrefabs => prefabs;

        public override bool TryGetPrefab(int id, out GameObject prefab)
        {
            if (id < 0 || id >= prefabs.Count)
            {
                prefab = null;
                return false;
            }

            prefab = prefabs[id].prefab;
            return true;
        }

        public override bool TryGetPrefabData(int id, out PrefabData prefab)
        {
            if (id < 0 || id >= prefabs.Count)
            {
                prefab = default;
                return false;
            }

            prefab = prefabs[id];
            return true;
        }

        public override bool TryGetPrefab(int id, int offset, out GameObject prefab)
        {
            if (!TryGetPrefab(id, out var root))
            {
                prefab = null;
                return false;
            }

            if (offset == 0)
            {
                prefab = root;
                return true;
            }

            root.GetComponentsInChildren(true, _identities);

            if (offset < 0 || offset >= _identities.Count)
            {
                prefab = null;
                return false;
            }

            prefab = _identities[offset].gameObject;
            return true;
        }

        static readonly List<NetworkIdentity> _identities = new List<NetworkIdentity>();
#if UNITY_EDITOR
        private bool _generating;
#endif

        private void OnValidate()
        {
            if (autoGenerate)
                Generate();
        }

        /// <summary>
        /// Editor only method to generate network prefabs from a specified folder.
        /// </summary>
        public void Generate()
        {
        #if UNITY_EDITOR
            if (ApplicationContext.isClone)
                return;

            if (_generating) return;

            _generating = true;

            try
            {
                EditorUtility.DisplayProgressBar("Getting Network Prefabs", "Checking existing...", 0f);

                if (folder == null || string.IsNullOrEmpty(AssetDatabase.GetAssetPath(folder)))
                {
                    EditorUtility.DisplayProgressBar("Getting Network Prefabs", "No folder found...", 0f);
                    if (autoGenerate && prefabs.Count > 0)
                    {
                        prefabs.Clear();
                        EditorUtility.SetDirty(this);
                    }

                    EditorUtility.ClearProgressBar();
                    _generating = false;
                    return;
                }

                EditorUtility.DisplayProgressBar("Getting Network Prefabs", "Found folder...", 0f);
                string folderPath = AssetDatabase.GetAssetPath(folder);

                if (string.IsNullOrEmpty(folderPath))
                {
                    EditorUtility.DisplayProgressBar("Getting Network Prefabs", "No folder path...", 0f);

                    if (autoGenerate && prefabs.Count > 0)
                    {
                        prefabs.Clear();
                        EditorUtility.SetDirty(this);
                    }

                    EditorUtility.ClearProgressBar();
                    _generating = false;
                    PurrLogger.LogError("Exiting Generate method early due to empty folder path.");
                    return;
                }

                EditorUtility.DisplayProgressBar("Getting Network Prefabs", "Getting existing paths...", 0f);

                var existingPaths = new HashSet<string>();
                foreach (var prefabData in prefabs)
                {
                    if (prefabData.prefab)
                    {
                        existingPaths.Add(AssetDatabase.GetAssetPath(prefabData.prefab));
                    }
                }

                EditorUtility.DisplayProgressBar("Getting Network Prefabs", "Finding paths...", 0.1f);

                List<GameObject> foundPrefabs = new List<GameObject>();
                string[] guids = AssetDatabase.FindAssets("t:prefab", new[] { folderPath });
                for (var i = 0; i < guids.Length; i++)
                {
                    var guid = guids[i];
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

                    if (prefab)
                    {
                        EditorUtility.DisplayProgressBar("Getting Network Prefabs", $"Looking at {prefab.name}",
                            0.1f + 0.7f * ((i + 1f) / guids.Length));

                        if (!networkOnly)
                        {
                            foundPrefabs.Add(prefab);
                            continue;
                        }

                        prefab.GetComponentsInChildren(true, _identities);

                        if (_identities.Count > 0)
                            foundPrefabs.Add(prefab);
                    }
                }

                EditorUtility.DisplayProgressBar("Getting Network Prefabs", "Sorting...", 0.9f);

                foundPrefabs.Sort((a, b) =>
                {
                    string pathA = AssetDatabase.GetAssetPath(a);
                    string pathB = AssetDatabase.GetAssetPath(b);

                    var fileInfoA = new FileInfo(pathA);
                    var fileInfoB = new FileInfo(pathB);

                    return fileInfoA.CreationTime.CompareTo(fileInfoB.CreationTime);
                });

                EditorUtility.DisplayProgressBar("Getting Network Prefabs", "Removing invalid prefabs...", 0.95f);
                
                int removed = prefabs.RemoveAll(prefabData => !prefabData.prefab || !File.Exists(AssetDatabase.GetAssetPath(prefabData.prefab)));

                for (int i = 0; i < prefabs.Count; i++)
                {
                    if (!foundPrefabs.Contains(prefabs[i].prefab))
                    {
                        prefabs.RemoveAt(i);
                        removed++;
                        i--;
                    }
                }

                int added = 0;
                foreach (var foundPrefab in foundPrefabs)
                {
                    var foundPath = AssetDatabase.GetAssetPath(foundPrefab);
                    if (!existingPaths.Contains(foundPath))
                    {
                        prefabs.Add(new PrefabData { prefab = foundPrefab, pooled = poolByDefault, warmupCount = 5});
                        added++;
                    }
                }

                if (removed > 0 || added > 0)
                    EditorUtility.SetDirty(this);
            }
            catch (Exception e)
            {
                PurrLogger.LogError($"An error occurred during prefab generation: {e.Message}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                _generating = false;
            }
        #endif
        }
    }
}
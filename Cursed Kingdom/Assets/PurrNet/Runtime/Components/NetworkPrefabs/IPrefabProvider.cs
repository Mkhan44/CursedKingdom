using System.Collections.Generic;
using UnityEngine;

namespace PurrNet
{
    public struct PrefabData
    {
        public int prefabId;
        public GameObject prefab;
        public bool pooled;
        public int warmupCount;
    }

    public interface IPrefabProvider
    {
        IEnumerable<PrefabData> allPrefabs { get; }

        bool TryGetPrefabData(int prefabId, out PrefabData prefabData);

        bool TryGetPrefabData(GameObject prefab, out PrefabData prefabData);
    }

    public abstract class PrefabProviderScriptable : ScriptableObject, IPrefabProvider
    {
        public abstract IEnumerable<PrefabData> allPrefabs { get; }

        public abstract bool TryGetPrefabData(int prefabId, out PrefabData prefabData);

        public abstract bool TryGetPrefabData(GameObject prefab, out PrefabData prefabData);
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace PurrNet
{
    public interface IPrefabProvider
    {
        IReadOnlyList<NetworkPrefabs.PrefabData> allPrefabs { get; }
        
        bool TryGetPrefab(int id, out GameObject prefab);
        
        bool TryGetPrefabData(int id, out NetworkPrefabs.PrefabData prefab);
        
        bool TryGetPrefab(int id, int offset, out GameObject prefab);
    }

    public abstract class PrefabProviderScriptable : ScriptableObject, IPrefabProvider
    {
        public abstract IReadOnlyList<NetworkPrefabs.PrefabData> allPrefabs { get; }

        public abstract bool TryGetPrefab(int id, out GameObject prefab);
        
        public abstract bool TryGetPrefabData(int id, out NetworkPrefabs.PrefabData prefab);
        
        public abstract bool TryGetPrefab(int id, int offset, out GameObject prefab);
    }
}

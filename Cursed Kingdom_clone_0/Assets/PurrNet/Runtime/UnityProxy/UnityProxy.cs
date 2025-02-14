using PurrNet.Logging;
using PurrNet.Modules;
using PurrNet.Pooling;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace PurrNet
{
    [UsedByIL]
    public static class UnityProxy
    {
        static GameObject GetGameObject<T>(T obj) where T : Object
        {
            return obj switch
            {
                Component component => component.gameObject,
                GameObject gameObject => gameObject,
                _ => null
            };
        }

        static T OnPreInstantiate<T>(NetworkPrefabs.PrefabData prefabData, InstantiateData<T> instantiateData) where T : Object
        {
            var prefab = prefabData.prefab;

            if (!instantiateData.TryGetHierarchy(out var hierarchy))
            {
                PurrLogger.LogError($"Failed to get hierarchy for `{prefab.name}`");
                return null;
            }
            
            if (!prefabData.pooled)
            {
                var instance = instantiateData.Instantiate();
                var go = GetGameObject(instance);
                PurrNetGameObjectUtils.NotifyGameObjectCreated(go, prefab);
                return instance;
            }

            if (!HierarchyPool.TryGetPrefabPrototype(prefab, out var prototype))
            {
                var instance = instantiateData.Instantiate();
                var go = GetGameObject(instance);
                PurrNetGameObjectUtils.NotifyGameObjectCreated(go, prefab);
                return instance;
            }

            var result = hierarchy.CreatePrototype(prototype, null);

            if (!result)
            {
                PurrLogger.LogError($"Failed to create prototype for `{prefab.name}`");
                return null;
            }
            
            instantiateData.ApplyToExisting(result, prefab);
            PurrNetGameObjectUtils.NotifyGameObjectCreated(result, prefab);
            
            if (result.TryGetComponent(out T component))
                return component;
            
            return (T)(Object)result;
        }
        
        static bool OnDestroy(Object instance)
        {
            if (instance is not NetworkIdentity &&
                instance is not GameObject)
                return true;
            
            var go = GetGameObject(instance);
            
            if (!go)
                return true;

            if (!go.GetComponentInChildren<NetworkIdentity>())
                return true;

            var identities = ListPool<NetworkIdentity>.Instantiate();
            go.GetComponentsInChildren(true, identities);

            for (var i = 0; i < identities.Count; i++)
            {
                var identity = identities[i];
                identity.Despawn();
            }
            
            ListPool<NetworkIdentity>.Destroy(identities);
            
            bool shouldDestroy = !go.GetComponent<NetworkIdentity>();
            return shouldDestroy;
        }
        
        static bool TryGetPrefabData(Object prefab, out NetworkPrefabs.PrefabData prefabData)
        {
            var prefabGo = GetGameObject(prefab);
            
            if (!prefabGo)
            {
                prefabData = default;
                return false;
            }

            var manager = NetworkManager.main;
            
            if (!manager || manager.isOffline)
            {
                prefabData = default;
                return false;
            }
            
            return manager.TryGetPrefabData(prefabGo, out prefabData, out _);
        }
        
        [UsedByIL]
        public static Object InstantiateDirectly(Object original) => Object.Instantiate(original);

        [UsedByIL]
        public static Object Instantiate(Object original)
        {
            if (!TryGetPrefabData(original, out var prefabData))
                return Object.Instantiate(original);
            return OnPreInstantiate(prefabData, new InstantiateData<Object>(original));
        }

        [UsedByIL]
        public static Object Instantiate(Object original, Transform parent, bool instantiateInWorldSpace)
        {
            if (!TryGetPrefabData(original, out var prefabData))
                return Object.Instantiate(original, parent, instantiateInWorldSpace);
            
            return OnPreInstantiate(prefabData, new InstantiateData<Object>(original, parent, instantiateInWorldSpace));
        }
        
        public static Object InstantiateDirectly(Object original, Transform parent, bool instantiateInWorldSpace)
            => Object.Instantiate(original, parent, instantiateInWorldSpace);
        
        [UsedByIL]
        public static Object Instantiate(Object original, Vector3 position, Quaternion rotation)
        {
            if (!TryGetPrefabData(original, out var prefabData))
                return Object.Instantiate(original, position, rotation);
            
            return OnPreInstantiate(prefabData, new InstantiateData<Object>(original, position, rotation));
        }
        
        public static Object InstantiateDirectly(Object original, Vector3 position, Quaternion rotation)
            => Object.Instantiate(original, position, rotation);

        [UsedByIL]
        public static Object Instantiate(
            Object original,
            Vector3 position,
            Quaternion rotation,
            Transform parent)
        {
            if (!TryGetPrefabData(original, out var prefabData))
                return Object.Instantiate(original, position, rotation, parent);
            
            return OnPreInstantiate(prefabData, new InstantiateData<Object>(original, position, rotation, parent));
        }
        
        public static Object InstantiateDirectly(
            Object original,
            Vector3 position,
            Quaternion rotation,
            Transform parent)
        {
            return Object.Instantiate(original, position, rotation, parent);
        }

#if UNITY_2023_1_OR_NEWER
        [UsedByIL]
        public static Object Instantiate(Object original, Scene scene)
        {
            if (!TryGetPrefabData(original, out var prefabData))
                return Object.Instantiate(original, scene);
            
            return OnPreInstantiate(prefabData, new InstantiateData<Object>(original, scene));
        }
        
        public static Object InstantiateDirectly(Object original, Scene scene)
            => Object.Instantiate(original, scene);
        
        public static T InstantiateDirectly<T>(T original, Scene scene) where T : Object
            => (T)Object.Instantiate(original, scene);
#endif
        
        [UsedByIL]
        public static Object Instantiate(Object original, Transform parent)
        {
            if (!TryGetPrefabData(original, out var prefabData))
                return Object.Instantiate(original, parent);
            
            return OnPreInstantiate(prefabData, new InstantiateData<Object>(original, parent));
        }
        
        public static Object InstantiateDirectly(Object original, Transform parent)
            => Object.Instantiate(original, parent);

        [UsedByIL]
        public static T Instantiate<T>(T original) where T : Object
        {
            if (!TryGetPrefabData(original, out var prefabData))
                return Object.Instantiate(original);

            return OnPreInstantiate(prefabData, new InstantiateData<T>(original));
        }
        
#if UNITY_6000_0_35
        [UsedByIL]
        public static T Instantiate<T>(T original, InstantiateParameters parameters) where T : Object
        {
            if (!TryGetPrefabData(original, out var prefabData))
                return Object.Instantiate(original, parameters);
            return OnPreInstantiate(prefabData, new InstantiateData<T>(original, parameters));
        }
        
        [UsedByIL]
        public static T Instantiate<T>(T original, Vector3 pos, Quaternion rot, InstantiateParameters parameters) where T : Object
        {
            if (!TryGetPrefabData(original, out var prefabData))
                return Object.Instantiate(original, pos, rot, parameters);
            return OnPreInstantiate(prefabData, new InstantiateData<T>(original, pos, rot, parameters));
        }
        
        [UsedByIL]
        public static T InstantiateDirectly<T>(T original, InstantiateParameters parameters) where T : Object
            => Object.Instantiate(original, parameters);
        
        [UsedByIL]
        public static T InstantiateDirectly<T>(T original, Vector3 pos, Quaternion rot, InstantiateParameters parameters) where T : Object
            => Object.Instantiate(original, pos, rot, parameters);
#endif
        
        public static T InstantiateDirectly<T>(T original) where T : Object
            => Object.Instantiate(original);

        [UsedByIL]
        public static T Instantiate<T>(T original, Vector3 position, Quaternion rotation) where T : Object
        {
            if (!TryGetPrefabData(original, out var prefabData))
                return Object.Instantiate(original, position, rotation);
            
            return OnPreInstantiate(prefabData, new InstantiateData<T>(original, position, rotation));
        }
        
        public static T InstantiateDirectly<T>(T original, Vector3 position, Quaternion rotation) where T : Object
            => Object.Instantiate(original, position, rotation);
        
        [UsedByIL]
        public static T Instantiate<T>(T original, Vector3 position, Quaternion rotation, Scene scene) where T : Object
        {
            if (!TryGetPrefabData(original, out var prefabData))
            {
                var obj = Object.Instantiate(original, position, rotation);
                var go = GetGameObject(obj);

                if (go && go.scene.handle != scene.handle)
                    SceneManager.MoveGameObjectToScene(go, scene);
                return obj;
            }
            
            return OnPreInstantiate(prefabData, new InstantiateData<T>(original, position, rotation));
        }
        
        public static T InstantiateDirectly<T>(T original, Vector3 position, Quaternion rotation, Scene scene) where T : Object
        {
            var obj = Object.Instantiate(original, position, rotation);
            var go = GetGameObject(obj);
                
            if (go)
                SceneManager.MoveGameObjectToScene(go, scene);
            return obj;
        }

        [UsedByIL]
        public static T Instantiate<T>(
            T original,
            Vector3 position,
            Quaternion rotation,
            Transform parent)
            where T : Object
        {
            if (!TryGetPrefabData(original, out var prefabData))
                return Object.Instantiate(original, position, rotation, parent);
            
            return OnPreInstantiate(prefabData, new InstantiateData<T>(original, position, rotation, parent));
        }
        
        public static T InstantiateDirectly<T>(
            T original,
            Vector3 position,
            Quaternion rotation,
            Transform parent)
            where T : Object
            => Object.Instantiate(original, position, rotation, parent);

        [UsedByIL]
        public static T Instantiate<T>(T original, Transform parent) where T : Object
        {
            if (!TryGetPrefabData(original, out var prefabData))
                return Object.Instantiate(original, parent);
            
            return OnPreInstantiate(prefabData, new InstantiateData<T>(original, parent));
        }
        
        public static T InstantiateDirectly<T>(T original, Transform parent) where T : Object
            => Object.Instantiate(original, parent);
        
        [UsedByIL]
        public static T Instantiate<T>(T original, Transform parent, bool worldPositionStays) where T : Object
        {
            if (!TryGetPrefabData(original, out var prefabData))
                return Object.Instantiate(original, parent, worldPositionStays);
            
            return OnPreInstantiate(prefabData, new InstantiateData<T>(original, parent, worldPositionStays));
        }
        
        public static T InstantiateDirectly<T>(T original, Transform parent, bool worldPositionStays) where T : Object
            => Object.Instantiate(original, parent, worldPositionStays);
        
        [UsedByIL]
        public static void Destroy(Object obj)
        {
            if (OnDestroy(obj))
                Object.Destroy(obj);
        }
        
        public static void DestroyDirectly(Object obj)
            => Object.Destroy(obj);

        [UsedByIL]
        public static void Destroy(Object obj, float t)
        {
            // TODO: Implement delay
            Object.Destroy(obj, t);
        }
        
        public static void DestroyDirectly(Object obj, float t)
            => Object.Destroy(obj, t);
        
        [UsedByIL]
        public static void DestroyImmediate(Object obj)
        {
            if (OnDestroy(obj))
                Object.DestroyImmediate(obj);
        }
        
        public static void DestroyImmediateDirectly(Object obj)
            => Object.DestroyImmediate(obj);
        
        [UsedByIL]
        public static void DestroyImmediate(Object obj, bool allowDestroyingAssets)
        {
            if (OnDestroy(obj))
                Object.DestroyImmediate(obj, allowDestroyingAssets);
        }
        
        public static void DestroyImmediateDirectly(Object obj, bool allowDestroyingAssets)
            => Object.DestroyImmediate(obj, allowDestroyingAssets);
    }
}

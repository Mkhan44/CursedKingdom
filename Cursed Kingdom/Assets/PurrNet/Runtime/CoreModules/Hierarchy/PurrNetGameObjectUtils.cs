using PurrNet.Logging;
using PurrNet.Pooling;
using UnityEngine;

namespace PurrNet.Modules
{
    public static class PurrNetGameObjectUtils
    {
        public delegate void GameObjectCreated(GameObject go, GameObject prefab);

        /// <summary>
        /// This is called in editor only when a GameObject is created in the scene by drag and drop.
        /// </summary>
        public static event GameObjectCreated onGameObjectCreated;

        public static void NotifyGameObjectCreated(GameObject go, GameObject prefab)
        {
            onGameObjectCreated?.Invoke(go, prefab);
        }

        struct BehaviourState
        {
            public Behaviour component;
            public bool enabled;
        }

        public static int GetTransformDepth(this Transform transform)
        {
            int depth = 0;
            while (transform.parent)
            {
                transform = transform.parent;
                depth++;
            }

            return depth;
        }

        /// <summary>
        /// Awake is not called on disabled game objects, so we need to ensure it's called for all components.
        /// </summary>
        public static void MakeSureAwakeIsCalled(this GameObject root)
        {
            var components = ListPool<Behaviour>.Instantiate();
            var cache = ListPool<BehaviourState>.Instantiate();
            var gosToDeactivate = HashSetPool<GameObject>.Instantiate();

            // for components in disabled game objects, disabled them, activate game object, and reset their enabled state
            root.GetComponentsInChildren(true, components);

            for (int i = 0; i < components.Count; i++)
            {
                var child = components[i];

                if (!child)
                    continue;

                if (!child.gameObject.activeSelf)
                {
                    cache.Add(new BehaviourState
                    {
                        component = child,
                        enabled = child.enabled
                    });

                    child.enabled = false;

                    gosToDeactivate.Add(child.gameObject);
                }
            }

            foreach (var go in gosToDeactivate)
            {
                go.SetActive(true);
                go.SetActive(false);
            }

            for (int i = 0; i < cache.Count; i++)
            {
                var state = cache[i];
                state.component.enabled = state.enabled;
            }

            ListPool<Behaviour>.Destroy(components);
            HashSetPool<GameObject>.Destroy(gosToDeactivate);
            ListPool<BehaviourState>.Destroy(cache);
        }
    }
}
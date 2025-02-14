using System;
using System.Collections.Generic;
using PurrNet.Modules;
using PurrNet.Pooling;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PurrNet.Editor
{
    public static class PurrDeleteHandler
    {
        static bool IsAnyNetworked(Object[] objectsToDelete)
        {
            if (!NetworkManager.main)
                return false;
            
            foreach (var obj in objectsToDelete)
            {
                switch (obj)
                {
                    case GameObject go when go.GetComponentInChildren<NetworkIdentity>():
                    case NetworkIdentity:
                        return true;
                }
            }
            
            return false;
        }
        
        public static bool CustomDuplicateLogic(Object[] objectsToDuplicate)
        {
            // if nothing network related just do normal duplicate
            if (!IsAnyNetworked(objectsToDuplicate))
                return false;
            
            var curatedObjects = GetCuratedGameObjectList(objectsToDuplicate);
            
            if (curatedObjects.Count == 0)
                return false;

            var newSelection = new Object[curatedObjects.Count];
            var manager = NetworkManager.main;

            for (var i = curatedObjects.Count - 1; i >= 0; i--)
            {
                var obj = curatedObjects[i];
                
                if (obj.TryGetComponent(out NetworkIdentity identity))
                {
                    var dup = identity.Duplicate();
                    dup.transform.parent = obj.transform.parent;
                    newSelection[i] = dup;
                }
                else
                {
                    var copy = UnityProxy.Instantiate(obj, obj.transform.parent);
                    newSelection[i] = copy;

                    var identities = new DisposableList<TransformIdentityPair>(16);
                    HierarchyPool.GetDirectChildren(copy.transform, identities);
                    foreach (var pair in identities)
                    {
                        if (manager.TryGetModule<HierarchyFactory>(manager.isServer, out var hierarchyFactory) &&
                            hierarchyFactory.TryGetHierarchy(obj.scene, out var hierarchy))
                        {
                            hierarchy.Spawn(pair.identity.gameObject);
                        }
                    }
                }
            }
            
            Selection.objects = newSelection;
            return true;
        }
        
        private static List<GameObject> GetCuratedGameObjectList(Object[] objects)
        {
            // Get unique GameObjects from selection
            HashSet<GameObject> gameObjects = new HashSet<GameObject>();

            foreach (var obj in objects)
            {
                if (obj is GameObject go)
                {
                    gameObjects.Add(go);
                }
            }

            // Remove children of already selected roots
            List<GameObject> curatedList = new List<GameObject>();
            foreach (var go in gameObjects)
            {
                if (!IsChildOfAny(go, gameObjects))
                {
                    curatedList.Add(go);
                }
            }

            return curatedList;
        }

        private static bool IsChildOfAny(GameObject go, HashSet<GameObject> gameObjects)
        {
            var parent = go.transform.parent;
            while (parent != null)
            {
                if (gameObjects.Contains(parent.gameObject))
                    return true;
                parent = parent.parent;
            }
            return false;
        }
        
        public static bool CustomDeleteLogic(Object[] objectsToDelete)
        {
            // if nothing network related just do normal delete
            if (!IsAnyNetworked(objectsToDelete))
                return false;
            
            var objectsToDeleteList = new List<Object>(objectsToDelete);

            for (int i = 0; i < objectsToDeleteList.Count; i++)
            {
                var obj = objectsToDeleteList[i];

                switch (obj)
                {
                    case GameObject go:
                    {
                        var identity = go.GetComponent<NetworkIdentity>();
                        if (identity)
                        {
                            objectsToDeleteList.RemoveAt(i--);
                            identity.Despawn();
                        }
                        else
                        {
                            using var children = new DisposableList<TransformIdentityPair>(16);
                            HierarchyPool.GetDirectChildren(go.transform, children);
                            foreach (var child in children)
                                child.identity.Despawn();
                        }
                        break;
                    }
                    case NetworkIdentity identity:
                        objectsToDeleteList.RemoveAt(i--);
                        identity.Despawn();
                        break;
                }
            }

            // Perform custom deletion logic
            foreach (var obj in objectsToDeleteList)
                Undo.DestroyObjectImmediate(obj);

            // deselect all objects
            Selection.objects = Array.Empty<Object>();
            
            return true;
        }
    }
}
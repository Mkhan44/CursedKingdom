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
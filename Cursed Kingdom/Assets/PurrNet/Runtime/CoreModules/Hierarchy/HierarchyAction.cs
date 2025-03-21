using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using PurrNet.Packing;
using UnityEngine;

namespace PurrNet.Modules
{
    internal enum HierarchyActionType : byte
    {
        Spawn,
        Despawn,
        ChangeParent,
        SetActive,
        SetEnabled
    }

    internal enum DespawnType : byte
    {
        ComponentOnly,
        GameObject
    }

    internal struct HierarchyAction : IPackedSimple
    {
        public HierarchyActionType type;
        public PlayerID actor;

        public DespawnAction despawnAction;
        public SpawnAction spawnAction;
        public ChangeParentAction changeParentAction;
        public SetActiveAction setActiveAction;
        public SetEnabledAction setEnabledAction;

        static readonly StringBuilder _sb = new StringBuilder();

        [UsedImplicitly]
        public static void WriteType(BitPacker packer, HierarchyAction type)
        {
            type.Serialize(packer);
        }

        [UsedImplicitly]
        public static void ReadType(BitPacker packer, ref HierarchyAction type)
        {
            type.Serialize(packer);
        }

        public override string ToString()
        {
            _sb.Clear();

            switch (type)
            {
                case HierarchyActionType.Spawn:
                    _sb.Append(spawnAction);
                    break;
                case HierarchyActionType.Despawn:
                    _sb.Append(despawnAction);
                    break;
                case HierarchyActionType.ChangeParent:
                    _sb.Append(changeParentAction);
                    break;
                case HierarchyActionType.SetActive:
                    _sb.Append(setActiveAction);
                    break;
                case HierarchyActionType.SetEnabled:
                    _sb.Append(setEnabledAction);
                    break;
            }

            return _sb.ToString();
        }

        public void Serialize(BitPacker packer)
        {
            Packer<HierarchyActionType>.Serialize(packer, ref type);
            Packer<PlayerID>.Serialize(packer, ref actor);

            switch (type)
            {
                case HierarchyActionType.Spawn:
                    Packer<SpawnAction>.Serialize(packer, ref spawnAction);
                    break;
                case HierarchyActionType.Despawn:
                    Packer<DespawnAction>.Serialize(packer, ref despawnAction);
                    break;
                case HierarchyActionType.ChangeParent:
                    Packer<ChangeParentAction>.Serialize(packer, ref changeParentAction);
                    break;
                case HierarchyActionType.SetActive:
                    Packer<SetActiveAction>.Serialize(packer, ref setActiveAction);
                    break;
                case HierarchyActionType.SetEnabled:
                    Packer<SetEnabledAction>.Serialize(packer, ref setEnabledAction);
                    break;
            }
        }

        public NetworkID? GetIdentityId()
        {
            return type switch
            {
                HierarchyActionType.Spawn => spawnAction.identityId,
                HierarchyActionType.Despawn => despawnAction.identityId,
                HierarchyActionType.ChangeParent => changeParentAction.identityId,
                HierarchyActionType.SetActive => setActiveAction.identityId,
                HierarchyActionType.SetEnabled => setEnabledAction.identityId,
                _ => null
            };
        }
    }

    internal struct HierarchyActionBatch : IPacked
    {
        public SceneID sceneId;
        public List<HierarchyAction> actions;

        public void Write(BitPacker packer)
        {
            Packer<SceneID>.Write(packer, sceneId);
            Packer<List<HierarchyAction>>.Write(packer, actions);
        }

        public void Read(BitPacker packer)
        {
            Packer<SceneID>.Read(packer, ref sceneId);
            Packer<List<HierarchyAction>>.Read(packer, ref actions);
        }
    }

    internal struct DespawnAction : IPackedAuto
    {
        public NetworkID identityId { get; set; }
        public DespawnType despawnType { get; set; }

        public override string ToString() => $"Despawn: {identityId} ({despawnType})";
    }

    internal struct SetActiveAction : IPackedAuto
    {
        public NetworkID identityId { get; set; }
        public bool active { get; set; }

        public override string ToString() => $"SetActive: {identityId} ({active})";
    }

    internal struct SetEnabledAction : IPackedAuto
    {
        public NetworkID identityId { get; set; }
        public bool enabled { get; set; }

        public override string ToString() => $"SetEnabled: {identityId} ({enabled})";
    }

    internal struct SpawnAction : IPackedAuto
    {
        public int prefabId { get; set; }
        public NetworkID identityId { get; set; }
        public ushort childCount { get; set; }
        public TransformInfo transformInfo { get; set; }

        /// <summary>
        /// Spawn a child of the root identity.
        /// This avoids the need to spawn the root, get the child and then despawn the root.
        /// </summary>
        public ushort childOffset { get; set; }

        public override string ToString() => $"Spawn: {identityId} (pid: {prefabId}, children: {childCount})";
    }

    public struct ChangeParentAction : IPackedAuto
    {
        public NetworkID identityId { get; set; }
        public NetworkID? parentId { get; set; }

        public override string ToString() => $"ChangeParent: {identityId} (target parent id: {parentId})";
    }

    public struct TransformInfo : IPackedAuto
    {
        public NetworkID? parentId { get; set; }
        public bool activeHierarchy { get; set; }
        public Vector3 localPos { get; set; }
        public Quaternion localRot { get; set; }
        public Vector3 localScale { get; set; }

        public TransformInfo(Transform trs)
        {
            activeHierarchy = trs.gameObject.activeInHierarchy;

            var parent = trs.parent;

            if (parent)
            {
                parentId = parent.TryGetComponent(out NetworkIdentity parentIdentity) ? parentIdentity.id : null;
            }
            else
            {
                parentId = null;
            }

            localPos = trs.localPosition;
            localRot = trs.localRotation;
            localScale = trs.localScale;
        }
    }
}
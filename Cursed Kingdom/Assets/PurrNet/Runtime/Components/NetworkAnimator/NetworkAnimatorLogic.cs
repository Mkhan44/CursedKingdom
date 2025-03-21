using System.Collections.Generic;
using JetBrains.Annotations;

namespace PurrNet
{
    public sealed partial class NetworkAnimator : ITick
    {
        readonly List<NetAnimatorRPC> _dirty = new List<NetAnimatorRPC>();
        readonly List<NetAnimatorRPC> _ikActions = new List<NetAnimatorRPC>();

        readonly List<PlayerID> _reconcilePlayers = new List<PlayerID>();

        protected override void OnObserverAdded(PlayerID player)
        {
            Reconcile(player);
            _reconcilePlayers.Add(player);
        }

        public void OnTick(float delta)
        {
            if (!IsController(_ownerAuth))
            {
                if (_dirty.Count > 0)
                    _dirty.Clear();
                return;
            }

            if (_autoSyncParameters)
                CheckForParameterChanges();
            SendDirtyActions();
        }

        /// <summary>
        /// Sends the current state of the animator to the observers.
        /// This is useful when you need to ensure that the observers are in sync with the controller.
        /// </summary>
        public void Reconcile(bool isIk = false)
        {
            if (!IsController(_ownerAuth))
                return;

            var data = NetAnimatorActionBatch.CreateReconcile(_dontSyncHashes, _animator, isIk);

            if (isServer)
            {
                ApplyActionsOnObservers(data);
            }
            else
            {
                ForwardThroughServer(data);
            }
        }

        /// <summary>
        /// Sends the current state of the animator to the target player.
        /// This is useful when a new player joins the scene.
        /// Or when you need to ensure that the player is in sync with the controller.
        /// </summary>
        /// <param name="target">The target player to reconcile the state with.</param>
        /// <param name="isIk">Whether to reconcile the IK state or the regular state.</param>
        public void Reconcile(PlayerID target, bool isIk = false)
        {
            if (!IsController(_ownerAuth))
                return;

            var data = NetAnimatorActionBatch.CreateReconcile(_dontSyncHashes, _animator, isIk);

            if (isServer)
            {
                ReconcileState(target, data);
            }
            else
            {
                ForwardThroughServerToTarget(target, data);
            }
        }

        private void SendDirtyActions()
        {
            if (_dirty.Count <= 0)
                return;

            var batch = new NetAnimatorActionBatch
            {
                actions = _dirty
            };

            if (isServer)
            {
                ApplyActionsOnObservers(batch);
            }
            else
            {
                ForwardThroughServer(batch);
            }

            _dirty.Clear();
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (IsController(_ownerAuth))
            {
                _ikActions.Clear();

                for (var i = 0; i < _reconcilePlayers.Count; i++)
                    Reconcile(_reconcilePlayers[i], true);
                _reconcilePlayers.Clear();
                return;
            }

            _reconcilePlayers.Clear();

            for (var i = 0; i < _ikActions.Count; i++)
                _ikActions[i].Apply(_animator);
        }

        [TargetRpc]
        private void ReconcileState([UsedImplicitly] PlayerID player, NetAnimatorActionBatch actions)
        {
            if (IsController(_ownerAuth))
                return;

            ExecuteBatch(actions);
        }

        [ServerRpc]
        private void ForwardThroughServerToTarget(PlayerID target, NetAnimatorActionBatch actions)
        {
            if (_ownerAuth)
                ReconcileState(target, actions);
        }

        [ServerRpc]
        private void ForwardThroughServer(NetAnimatorActionBatch actions)
        {
            if (_ownerAuth)
            {
                if (!isClient)
                    ExecuteBatch(actions);
                ApplyActionsOnObservers(actions);
            }
        }

        [ObserversRpc]
        private void ApplyActionsOnObservers(NetAnimatorActionBatch actions)
        {
            if (IsController(_ownerAuth))
                return;

            ExecuteBatch(actions);
        }

        private void ExecuteBatch(NetAnimatorActionBatch actions)
        {
            if (!_animator)
                return;

            if (actions.actions == null)
                return;

            for (var i = 0; i < actions.actions.Count; i++)
            {
                bool isIk = actions.actions[i].type is
                    NetAnimatorAction.SetIKPosition or NetAnimatorAction.SetIKRotation or
                    NetAnimatorAction.SetIKHintPosition or NetAnimatorAction.SetLookAtPosition or
                    NetAnimatorAction.SetBoneLocalRotation;

                if (!isIk)
                    actions.actions[i].Apply(_animator);
                else _ikActions.Add(actions.actions[i]);
            }
        }
    }
}
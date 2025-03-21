using JetBrains.Annotations;
using UnityEngine;

namespace PurrNet
{
    public sealed class NetworkOwnershipToggle : NetworkIdentity
    {
        [Header("GameObjects to toggle when owner is set")]
        [Tooltip("GameObjects to activate when the owner is set")]
        [SerializeField]
        private GameObject[] _toActivate;

        [Tooltip("GameObjects to deactivate when the owner is set")] [SerializeField]
        private GameObject[] _toDeactivate;

        [Header("Components to toggle when owner is set")]
        [Tooltip("Components to enable when the owner is set")]
        [SerializeField]
        private Behaviour[] _toEnable;

        [Tooltip("Components to disable when the owner is set")] [SerializeField]
        private Behaviour[] _toDisable;

        private bool _lastOwner;

        private void Awake()
        {
            Setup(false);
        }

        [UsedImplicitly]
        public void Setup(bool asOwner)
        {
            _lastOwner = asOwner;

            for (var i = 0; i < _toActivate.Length; i++)
            {
                var go = _toActivate[i];
                if (!go) continue;
                go.SetActive(asOwner);
            }

            for (var i = 0; i < _toDeactivate.Length; i++)
            {
                var go = _toDeactivate[i];
                if (!go) continue;
                go.SetActive(!asOwner);
            }

            for (var i = 0; i < _toEnable.Length; i++)
            {
                var comp = _toEnable[i];
                if (!comp) continue;
                comp.enabled = asOwner;
            }

            for (var i = 0; i < _toDisable.Length; i++)
            {
                var comp = _toDisable[i];
                if (!comp) continue;
                comp.enabled = !asOwner;
            }
        }

        protected override void OnOwnerChanged(PlayerID? oldOwner, PlayerID? newOwner, bool asServer)
        {
            if (isOwner != _lastOwner)
                Setup(isOwner);
        }
    }
}
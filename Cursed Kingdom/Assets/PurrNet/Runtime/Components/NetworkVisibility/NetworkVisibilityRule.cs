using UnityEngine;

namespace PurrNet
{
    public abstract class NetworkVisibilityRule : ScriptableObject, INetworkVisibilityRule
    {
        protected NetworkManager manager;

        public void Setup(NetworkManager nmanager)
        {
            manager = nmanager;
        }

        /// <summary>
        /// The higher the complexity the later the rule will be checked.
        /// We will prioritize rules with lower complexity first.
        /// </summary>
        public abstract int complexity { get; }

        public abstract bool CanSee(PlayerID player, NetworkIdentity target);
    }
}
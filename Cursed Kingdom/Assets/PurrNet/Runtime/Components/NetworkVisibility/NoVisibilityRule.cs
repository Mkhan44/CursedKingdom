using UnityEngine;

namespace PurrNet
{
    [CreateAssetMenu(menuName = "PurrNet/NetworkVisibility/No Visiblity")]
    public class NoVisibilityRule : NetworkVisibilityRule
    {
        public override int complexity => 0;

        public override bool CanSee(PlayerID player, NetworkIdentity target)
        {
            return false;
        }
    }
}
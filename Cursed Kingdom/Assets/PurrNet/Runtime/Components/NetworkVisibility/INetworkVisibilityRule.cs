namespace PurrNet
{
    public interface INetworkVisibilityRule
    {
        /// <summary>
        /// The higher the complexity the later the rule will be checked.
        /// We will prioritize rules with lower complexity first.
        /// </summary>
        int complexity { get; }

        /// <summary>
        /// Check if a player can see a target.
        /// </summary>
        /// <param name="player">PlayerID to check</param>
        /// <param name="target">Which identity we talking about</param>
        /// <returns></returns>
        bool CanSee(PlayerID player, NetworkIdentity target);
    }
}
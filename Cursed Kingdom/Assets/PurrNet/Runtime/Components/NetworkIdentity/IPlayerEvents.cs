﻿namespace PurrNet
{
    public interface ITick
    {
        /// <summary>
        /// Similar to FixedUpdate but tailored for networked objects and it's tick system.
        /// </summary>
        /// <param name="delta"></param>
        void OnTick(float delta);
    }
    
    public interface IPlayerEvents
    {
        void OnPlayerConnected(PlayerID playerId, bool isReconnect, bool asServer);
        
        void OnPlayerDisconnected(PlayerID playerId, bool asServer);
    }
    
    public interface IServerSceneEvents
    {
        void OnPlayerLoadedScene(PlayerID playerId);
        
        void OnPlayerUnloadedScene(PlayerID playerId);
    }
}
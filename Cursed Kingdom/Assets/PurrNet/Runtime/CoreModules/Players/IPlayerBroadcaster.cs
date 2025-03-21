using System.Collections.Generic;
using PurrNet.Transports;

namespace PurrNet
{
    public interface IPlayerBroadcaster
    {
        public void Send<T>(PlayerID player, T data, Channel method = Channel.ReliableOrdered);

        public void Send<T>(IEnumerable<PlayerID> players, T data, Channel method = Channel.ReliableOrdered);

        public void SendToServer<T>(T data, Channel method = Channel.ReliableOrdered);

        public void SendToAll<T>(T data, Channel method = Channel.ReliableOrdered);

        public void Unsubscribe<T>(PlayerBroadcastDelegate<T> callback) where T : new();

        public void Subscribe<T>(PlayerBroadcastDelegate<T> callback) where T : new();
    }
}
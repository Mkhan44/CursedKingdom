#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

#if STEAMWORKS_NET
#define STEAMWORKS_NET_PACKAGE
#endif

using System;
using System.Collections;
using PurrNet.Transports;
#if STEAMWORKS_NET_PACKAGE && !DISABLESTEAMWORKS
using System.Runtime.InteropServices;
using PurrNet.Logging;
using Steamworks;
#endif

namespace PurrNet.Steam
{
    public class SteamClient
    {
#if STEAMWORKS_NET_PACKAGE && !DISABLESTEAMWORKS
        const int MAX_MESSAGES = 256;
        private Callback<SteamNetConnectionStatusChangedCallback_t> _onLocalConnectionState;
        
        private CSteamID _hostSteamID;
        private HSteamNetConnection _connection;
        private bool _isDedicated;
        static byte[] buffer = new byte[1024];
        readonly IntPtr[] _messages = new IntPtr[MAX_MESSAGES];
#endif

#pragma warning disable CS0067 // Event is never used
        public event Action<ByteData> onDataReceived;
#pragma warning restore CS0067 // Event is never used
        public event Action<ConnectionState> onConnectionState;

        private ConnectionState _state = ConnectionState.Disconnected;
        
        public ConnectionState connectionState
        {
            get => _state;
            set
            {
                if (_state == value)
                    return;
                
                _state = value;
                onConnectionState?.Invoke(_state);
            }
        }

        public IEnumerator Connect(string address, ushort port, bool dedicated = false)
        {
            yield return null;
#if STEAMWORKS_NET_PACKAGE && !DISABLESTEAMWORKS
            _isDedicated = dedicated;
            
            var addr = new SteamNetworkingIPAddr();
            addr.Clear();
            addr.SetIPv4(address.GetIPv4(), port);

            _connection = _isDedicated ? 
                SteamGameServerNetworkingSockets.ConnectByIPAddress(ref addr, 0, null) :
                SteamNetworkingSockets.ConnectByIPAddress(ref addr, 0, null);
            
            PostConnect();
#endif
        }
        
        public IEnumerator ConnectP2P(string steamId, bool dedicated = false)
        {
            yield return null;
#if STEAMWORKS_NET_PACKAGE && !DISABLESTEAMWORKS
            if (ulong.TryParse(steamId, out var id) == false)
            {
                PurrLogger.LogError("Invalid Steam ID provided as address to connect");
                yield break;
            }
            
            _isDedicated = dedicated;
            _hostSteamID = new CSteamID(id);
            
            var networkIdentity = new SteamNetworkingIdentity();
            networkIdentity.SetSteamID(_hostSteamID);
            
            _connection = _isDedicated ? 
                SteamGameServerNetworkingSockets.ConnectP2P(ref networkIdentity, 0, 0, null) :
                SteamNetworkingSockets.ConnectP2P(ref networkIdentity, 0, 0, null);
            
            PostConnect();
#endif
        }
        
        public void Send(ByteData data, Channel channel)
        {
#if STEAMWORKS_NET_PACKAGE && !DISABLESTEAMWORKS
            MakeSureBufferCanFit(data.length);
            
            var pinnedArray = GCHandle.Alloc(data.data, GCHandleType.Pinned);
            var ptr = pinnedArray.AddrOfPinnedObject() + data.offset;
            
            byte sendFlag = channel switch {
                Channel.Unreliable => Constants.k_nSteamNetworkingSend_Unreliable,
                Channel.UnreliableBatched => Constants.k_nSteamNetworkingSend_Unreliable,
                Channel.UnreliableSequenced => Constants.k_nSteamNetworkingSend_Reliable,

                Channel.ReliableOrdered => Constants.k_nSteamNetworkingSend_Reliable,
                Channel.ReliableBatched => Constants.k_nSteamNetworkingSend_Reliable,
                Channel.ReliableUnordered => Constants.k_nSteamNetworkingSend_Reliable,
                _ => 0
            };
            
            if (_isDedicated)
                SteamGameServerNetworkingSockets.SendMessageToConnection(_connection, ptr, (uint)data.length, sendFlag, out _);
            else SteamNetworkingSockets.SendMessageToConnection(_connection, ptr, (uint)data.length, sendFlag, out _);
#endif
        }
        
        public void RunCallbacks()
        {
#if STEAMWORKS_NET_PACKAGE && !DISABLESTEAMWORKS
            SendQueuesMessages();
            ReceiveMessages();
#endif
        }
        
#if STEAMWORKS_NET_PACKAGE && !DISABLESTEAMWORKS
        private void SendQueuesMessages()
        {
            if (_isDedicated)
                SteamGameServerNetworkingSockets.FlushMessagesOnConnection(_connection);
            else SteamNetworkingSockets.FlushMessagesOnConnection(_connection);
        }
 
        private void ReceiveMessages()
        {
            int receivedCount = _isDedicated ? 
                SteamGameServerNetworkingSockets.ReceiveMessagesOnConnection(_connection, _messages, MAX_MESSAGES) : 
                SteamNetworkingSockets.ReceiveMessagesOnConnection(_connection, _messages, MAX_MESSAGES);
                
            for (int j = 0; j < receivedCount; j++)
            {
                var ptr = _messages[j];
                var data = Marshal.PtrToStructure<SteamNetworkingMessage_t>(ptr);
                    
                int packetLength = data.m_cbSize;
                MakeSureBufferCanFit(packetLength);
                    
                Marshal.Copy(data.m_pData, buffer, 0, packetLength);
                var byteData = new ByteData(buffer, 0, packetLength);
                    
                SteamNetworkingMessage_t.Release(ptr);
                onDataReceived?.Invoke(byteData);
            }
        }

        private static void MakeSureBufferCanFit(int packetLength)
        {
            if (buffer.Length < packetLength)
                Array.Resize(ref buffer, packetLength);
        }

        private void PostConnect()
        {
            if (_connection == HSteamNetConnection.Invalid)
            {
                connectionState = ConnectionState.Disconnecting;
                connectionState = ConnectionState.Disconnected;
                PurrLogger.LogError("Failed to connect to host");
                return;
            }
            
            connectionState = ConnectionState.Connecting;
            _onLocalConnectionState = Callback<SteamNetConnectionStatusChangedCallback_t>.Create(OnLocalConnectionState);
        }

        private void OnLocalConnectionState(SteamNetConnectionStatusChangedCallback_t param)
        {
            if (param.m_hConn != _connection)
                return;
            
            switch (param.m_info.m_eState)
            {
                case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting:
                    connectionState = ConnectionState.Connecting;
                    break;
                case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected:
                    connectionState = ConnectionState.Connected;
                    break;
                case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally:
                case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer:
                    connectionState = ConnectionState.Disconnecting;
                    connectionState = ConnectionState.Disconnected;
                    break;
            }
        }

        void Disconnect()
        {
            if (_connection != HSteamNetConnection.Invalid)
            {
                if (connectionState != ConnectionState.Disconnected)
                    connectionState = ConnectionState.Disconnecting;

                try
                {
                    if (_isDedicated)
                        SteamGameServerNetworkingSockets.CloseConnection(_connection, 0, null, false);
                    else SteamNetworkingSockets.CloseConnection(_connection, 0, null, false);
                }
                catch
                {
                    // ignored
                }
                
                connectionState = ConnectionState.Disconnected;
                _connection = HSteamNetConnection.Invalid;
            }
        }
#endif

        public void Stop()
        {
#if STEAMWORKS_NET_PACKAGE && !DISABLESTEAMWORKS
            if (_onLocalConnectionState != null)
            {
                _onLocalConnectionState.Dispose();
                _onLocalConnectionState = null;
            }
            
            Disconnect();
#endif
        }
    }
}

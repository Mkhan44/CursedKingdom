using PurrNet.Transports;
using PurrNet;
using UnityEngine;
using System.Threading.Tasks;
using System;
using System.Collections;

namespace Purrnet.Tests
{
    public static class PurrTests
    {
        public static void DoWith(NetworkManager manager, Action action)
        {
            if (manager == NetworkManager.main)
            {
                action();
                return;
            }

            var old = NetworkManager.main;
            NetworkManager.SetMainInstance(manager);
            action();
            NetworkManager.SetMainInstance(old);
        }

        public static IEnumerator ReconnectClient(NetworkManager manager)
        {
            manager.StopClient();

            while (manager.clientState != ConnectionState.Disconnected)
                yield return null;

            manager.StartClient();

            WaitOrThrow(() => manager.clientState == ConnectionState.Connected);
        }

        public static IEnumerator WaitOrThrow(Func<bool> condition, float timeout = 5f, string message = "")
        {
            float time = Time.time;
            while (!condition())
            {
                if (Time.time - time > timeout)
                    throw new TimeoutException(message);

                yield return null;
            }
        }

        public static NetworkManager BuildNetworkManager<T>() where T : GenericTransport
        {
            var go = new GameObject("NetworkTransport");
            var transport = go.AddComponent<T>();
            return BuildNetworkManager(transport);
        }

        public static NetworkManager BuildNetworkManager(GenericTransport transport)
        {
            var go = new GameObject("NetworkManager");
            var manager = go.AddComponent<NetworkManager>();

            manager.startClientFlags = StartFlags.None;
            manager.startServerFlags = StartFlags.None;
            manager.transport = transport;

            return manager;
        }

        static ushort _nextPort = 1000;

        public static UDPTransport BuildSafeUDP()
        {
            var go = new GameObject("UDPTransport");
            var udp = go.AddComponent<UDPTransport>();
            udp.serverPort = _nextPort++;
            return udp;
        }

        public static async Task<NetworkManager> BuildNetworkManagerWithHost(GenericTransport transport)
        {
            var nm = BuildNetworkManager(transport);
            return await InternalStartHost(nm);
        }

        public static async Task<NetworkManager> BuildHostUDP()
        {
            var nm = BuildNetworkManager(PurrTests.BuildSafeUDP());
            return await InternalStartHost(nm);
        }

        static NetworkManager _udpHostCached;

        public static IEnumerator StartHostUDP()
        {
            if (_udpHostCached != null)
                yield break;

            var nm = BuildNetworkManager(PurrTests.BuildSafeUDP());

            _udpHostCached = nm;

            nm.StartServer();
            nm.StartClient();

            var time = Time.time;

            while (!nm.isHost)
            {
                if (Time.time - time > 5)
                    break;

                yield return null;
            }

            if (!nm.isHost)
                throw new Exception($"Failed to start transport as host after 5 seconds.");
        }

        private static async Task<NetworkManager> InternalStartHost(NetworkManager nm)
        {
            nm.StartServer();
            nm.StartClient();

            var time = Time.time;

            while (!nm.isHost)
            {
                if (Time.time - time > 5)
                    break;

                await Task.Yield();
            }

            if (!nm.isHost)
                throw new Exception($"Failed to start transport as host after 5 seconds.");

            return nm;
        }
    }
}

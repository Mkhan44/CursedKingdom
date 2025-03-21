// #define USE_LOCAL_MASTER

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

namespace PurrNet.Transports
{
    [UsedImplicitly]
    [Serializable]
    public struct RelayServer
    {
        public string host;
        public int restPort;
        public int udpPort;
        public int webSocketsPort;
        public string region;
    }

    [UsedImplicitly]
    [Serializable]
    public struct Relayers
    {
        public RelayServer[] servers;
    }

    [UsedImplicitly]
    [Serializable]
    public struct HostJoinInfo
    {
        public bool ssl;
        public string secret;
        public int port;
    }

    [UsedImplicitly]
    [Serializable]
    public struct ClientJoinInfo
    {
        public bool ssl;
        public string secret;
        public string host;
        public int port;
    }

    public static class PurrTransportUtils
    {
        static UniTask<string> Get(string url)
        {
            var request = UnityWebRequest.Get(url);
            return request.SendWebRequest().ToUniTask().ContinueWith(_ => request.downloadHandler.text);
        }

        internal static async UniTask<ClientJoinInfo> Join(string server, string roomName)
        {
            var url = $"{server}join";
            var request = UnityWebRequest.Get(url);
            request.SetRequestHeader("name", roomName);
            var response = await request.SendWebRequest().ToUniTask();
            var text = response.downloadHandler.text;
            var res = JsonUtility.FromJson<ClientJoinInfo>(text);
#if USE_LOCAL_MASTER
            res.ssl = false;
#else
            res.ssl = true;
#endif
            return res;
        }

        internal static async UniTask<HostJoinInfo> Alloc(string server, string region, string roomName)
        {
            var url = $"{server}allocate_ws";

            var request = UnityWebRequest.Get(url);
            request.SetRequestHeader("region", region);
            request.SetRequestHeader("name", roomName);
            var response = await request.SendWebRequest().ToUniTask();
            var text = response.downloadHandler.text;
            var res = JsonUtility.FromJson<HostJoinInfo>(text);
#if USE_LOCAL_MASTER
            res.ssl = false;
#else
            res.ssl = true;
#endif
            return res;
        }

        static async UniTask<float> PingInMS(string url)
        {
            var request = UnityWebRequest.Get(url);
            var sent = DateTime.Now;
            await request.SendWebRequest().ToUniTask();
            var received = DateTime.Now;
            return (float)(received - sent).TotalSeconds;
        }

        public static async UniTask<Relayers> GetRelayServersAsync(string server)
        {
            string master = $"{server}servers";
            var response = await Get(master);
            return JsonUtility.FromJson<Relayers>(response);
        }

        public static async UniTask<RelayServer> GetRelayServerAsync(string masterServer)
        {
            var servers = await GetRelayServersAsync(masterServer);
            float minPing = float.MaxValue;
            RelayServer result = default;

            var pings = new List<Task<float>>();

            foreach (var server in servers.servers)
            {
#if USE_LOCAL_MASTER
                var pingUrl = $"http://{server.host}:{server.restPort}/ping";
#else
                var pingUrl = $"https://{server.host}:{server.restPort}/ping";
#endif
                pings.Add(PingInMS(pingUrl).AsTask());
            }

            await Task.WhenAny(pings);

            for (var i = 0; i < pings.Count; i++)
            {
                var ping = pings[i];

                if (ping.Status != TaskStatus.RanToCompletion)
                    continue;

                var resultPing = ping.Result;

                if (resultPing < minPing)
                {
                    minPing = resultPing;
                    result = servers.servers[i];
                }
            }

            return result;
        }
    }
}
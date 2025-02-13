using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using PurrNet.Transports;

namespace Purrnet.Tests
{
    public class ConnectionTests
    {
        [UnityTest]
        public IEnumerator UDPTransport()
        {
            var networkManager = PurrTests.BuildNetworkManager(PurrTests.BuildSafeUDP());

            yield return null;

            Assert.AreEqual(false, networkManager.isServer);
            Assert.AreEqual(false, networkManager.isClient);

            Assert.AreEqual(ConnectionState.Disconnected, networkManager.serverState);
            Assert.AreEqual(ConnectionState.Disconnected, networkManager.clientState);

            yield return null;

            networkManager.StartServer();

            yield return null;

            Assert.AreEqual(ConnectionState.Connected, networkManager.serverState);
            Assert.AreEqual(ConnectionState.Disconnected, networkManager.clientState);

            yield return null;

            networkManager.StartClient();

            Assert.AreEqual(ConnectionState.Connected, networkManager.serverState);
            Assert.AreEqual(ConnectionState.Connecting, networkManager.clientState);

            float timer = 0;

            while (networkManager.clientState != ConnectionState.Connected && timer < 5)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            Assert.AreEqual(ConnectionState.Connected, networkManager.serverState);
            Assert.AreEqual(ConnectionState.Connected, networkManager.clientState);
        }
    }
}

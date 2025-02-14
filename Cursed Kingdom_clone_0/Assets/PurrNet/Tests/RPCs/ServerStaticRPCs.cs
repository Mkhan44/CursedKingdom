using System.Collections;
using NUnit.Framework;
using PurrNet;
using PurrNet.Transports;
using UnityEngine;
using UnityEngine.TestTools;

namespace Purrnet.Tests
{
    public class ServerStaticRPCs
    {
        private static bool _receivedServerRPC = false;
        private static int _receivedNumber = -1;

        [UnityTest]
        public IEnumerator SimpleCallWithoutArguments()
        {
            yield return PurrTests.StartHostUDP();

            _receivedServerRPC = false;
            ServerRPCExample();

            yield return PurrTests.WaitOrThrow(() => _receivedServerRPC);

            Assert.AreEqual(true, _receivedServerRPC);
        }

        [ServerRpc]
        private static void ServerRPCExample()
        {
            _receivedServerRPC = true;
        }

        [UnityTest]
        public IEnumerator SimpleCallWithArgument()
        {
            yield return PurrTests.StartHostUDP();

            _receivedNumber = -1;
            ServerRPCExample(69);

            yield return PurrTests.WaitOrThrow(() => _receivedNumber != -1);

            Assert.AreEqual(69, _receivedNumber);
        }

        [ServerRpc]
        private static void ServerRPCExample(int number)
        {
            _receivedNumber = number;
        }

        private static int _receivedNumber0 = -1;
        private static int _receivedNumber1 = -1;
        private static float _receivedNumber3 = -1;
        private static string _receivedString4 = null;

        [UnityTest]
        public IEnumerator SimpleCallWithMultipleArgument()
        {
            yield return PurrTests.StartHostUDP();

            _receivedNumber0 = -1;
            _receivedNumber1 = -1;
            _receivedNumber3 = -1;
            _receivedString4 = null;

            ServerRPCExample(69, 70, 71, "72");

            yield return PurrTests.WaitOrThrow(() => _receivedString4 != null);

            Assert.AreEqual(69, _receivedNumber0);
            Assert.AreEqual(70, _receivedNumber1);
            Assert.AreEqual(71, _receivedNumber3);
            Assert.AreEqual("72", _receivedString4);
        }

        [ServerRpc]
        private static void ServerRPCExample(int number, int number2, float number3, string number4)
        {
            _receivedNumber0 = number;
            _receivedNumber1 = number2;
            _receivedNumber3 = number3;
            _receivedString4 = number4;
        }
    }
}

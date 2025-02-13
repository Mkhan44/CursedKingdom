using System.Collections;
using NUnit.Framework;
using PurrNet;
using PurrNet.Logging;
using UnityEngine;
using UnityEngine.TestTools;

namespace Purrnet.Tests
{
    public class StaticRPCsBuffering
    {
        static int _received;

        static void CallRPC()
        {
            BufferedRPCTest(25);
        }

        [UnityTest]
        public IEnumerator BufferingWithOneArgument()
        {
            // This builds a network manager setup and starts host
            var nmTask = PurrTests.BuildHostUDP();
            while (!nmTask.IsCompleted) yield return null;
            var nm = nmTask.Result;

            _received = -1;

            // This ensures we are buffering with the network manager above
            // all tests run at once so there could be multiple
            PurrTests.DoWith(nm, CallRPC);

            // Wait until the received value is updated
            yield return PurrTests.WaitOrThrow(() => _received != -1, message: "Failed to receive first value.");

            Assert.AreEqual(25, _received);

            // Now to actually test buffering, lets reset the received value
            _received = -1;

            // Reconnect client
            yield return PurrTests.ReconnectClient(nm);

            // Wait until the received value is updated
            yield return PurrTests.WaitOrThrow(() => _received != -1, message: "Never received a value update!");

            // Verify that value is back to previous
            Assert.AreEqual(25, _received);
        }

        [ObserversRpc(bufferLast: true)]
        private static void BufferedRPCTest(int value)
        {
            _received = value;
        }
    }
}

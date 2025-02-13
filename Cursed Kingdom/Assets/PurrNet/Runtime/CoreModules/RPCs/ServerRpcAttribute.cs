using System;
using PurrNet.Modules;
using PurrNet.Transports;
using UnityEngine.Scripting;

namespace PurrNet
{
    public class ServerRpcAttribute : PreserveAttribute
    {
        [UsedByIL]
        public ServerRpcAttribute(
            Channel channel = Channel.ReliableOrdered,
            bool runLocally = false, 
            bool requireOwnership = true,
            float asyncTimeoutInSec = 5f) {  }
    }
}

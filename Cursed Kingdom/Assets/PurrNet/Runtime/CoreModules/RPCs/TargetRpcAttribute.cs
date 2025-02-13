using System;
using PurrNet.Modules;
using PurrNet.Transports;
using UnityEngine.Scripting;

namespace PurrNet
{
    public class TargetRpcAttribute : PreserveAttribute
    {
        [UsedByIL]
        public TargetRpcAttribute(
            Channel channel = Channel.ReliableOrdered, 
            bool runLocally = false, 
            bool bufferLast = false, 
            bool requireServer = true,
            float asyncTimeoutInSec = 5f) { }
    }
}
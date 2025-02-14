using PurrNet.Modules;
using PurrNet.Transports;
using UnityEngine.Scripting;

namespace PurrNet
{
    public class ObserversRpcAttribute : PreserveAttribute
    {
        [UsedByIL]
        public ObserversRpcAttribute(Channel channel = Channel.ReliableOrdered, 
            bool runLocally = false,
            bool bufferLast = false,
            bool requireServer = true, 
            bool excludeOwner = false,
            bool excludeSender = false,
            float asyncTimeoutInSec = 5f) { }
    }
}

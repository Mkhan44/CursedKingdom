using System;
using PurrNet.Modules;
using PurrNet.Utils;
using UnityEngine.Scripting;

namespace PurrNet.Packing
{
    public class NetworkRegister
    {
        [UsedByIL]
        public static void Hash(RuntimeTypeHandle handle)
        {
            var type = Type.GetTypeFromHandle(handle);
            Hasher.PrepareType(type);
        }
    }

    public interface IPackedAuto
    {
    }

    public interface IPacked
    {
        void Write(BitPacker packer);

        void Read(BitPacker packer);
    }

    public interface IPackedSimple
    {
        void Serialize(BitPacker packer);
    }
}
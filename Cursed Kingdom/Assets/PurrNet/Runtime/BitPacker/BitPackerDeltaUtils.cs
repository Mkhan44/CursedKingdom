using K4os.Compression.LZ4;
using PurrNet.Packing;

namespace PurrNet
{
    public static class BitPackerDeltaUtils
    {
        public static void CreateDelta(BitPacker origin, BitPacker target, BitPacker result)
        {
            result.ResetPositionAndMode(false);

            var o = origin.ToByteData().span;
            var t = target.ToByteData().span;

            Fossil.Delta.Create(o, t, result);

            var pickled = LZ4Pickler.Pickle(result.ToByteData().span);

            result.ResetPositionAndMode(true);
            result.WriteBytes(pickled);
        }

        public static void ApplyDelta(BitPacker origin, BitPacker delta, BitPacker result)
        {
            result.ResetPositionAndMode(false);

            var o = origin.ToByteData().span;

            var unpickled = LZ4Pickler.Unpickle(delta.ToByteData().span);

            delta.ResetPositionAndMode(false);
            delta.WriteBytes(unpickled);
            var t = delta.ToByteData().span;
            delta.ResetPositionAndMode(true);

            Fossil.Delta.Apply(o, delta, t, result);
        }
    }
}
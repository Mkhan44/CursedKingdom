using System;

namespace Fossil
{
    public class RollingHash
    {
        private ushort a;
        private ushort b;
        private ushort i;
        private readonly byte[] z = new byte[Delta.NHASH];

        /**
         * Initialize the rolling hash using the first NHASH characters of z[]
         */
        public void Init(ReadOnlySpan<byte> z, int pos)
        {
            ushort _a = 0, _b = 0, _i, x;
            for (_i = 0; _i < Delta.NHASH; _i++)
            {
                x = z[pos + _i];
                _a = (ushort)((_a + x) & 0xffff);
                _b = (ushort)((_b + (Delta.NHASH - _i) * x) & 0xffff);
                this.z[_i] = (byte)x;
            }

            this.a = (ushort)(_a & 0xffff);
            this.b = (ushort)(_b & 0xffff);
            this.i = 0;
        }

        /**
         * Advance the rolling hash by a single character "c"
         */
        public void Next(byte c)
        {
            ushort old = this.z[this.i];
            this.z[this.i] = c;
            this.i = (ushort)((this.i + 1) & (Delta.NHASH - 1));
            this.a = (ushort)(this.a - old + c);
            this.b = (ushort)(this.b - Delta.NHASH * old + this.a);
        }


        /**
         * Return a 32-bit hash value
         */
        public uint Value()
        {
            return ((uint)(this.a & 0xffff)) | (((uint)(this.b & 0xffff)) << 16);
        }

        /*
         * Compute a hash on NHASH bytes.
         *
         * This routine is intended to be equivalent to:
         *    hash h;
         *    hash_init(&h, zInput);
         *    return hash_32bit(&h);
         */
        public static uint Once(byte[] z)
        {
            ushort a, b, i;
            a = b = z[0];
            for (i = 1; i < Delta.NHASH; i++)
            {
                a += z[i];
                b += a;
            }

            return a | (((uint)b) << 16);
        }
    }
}
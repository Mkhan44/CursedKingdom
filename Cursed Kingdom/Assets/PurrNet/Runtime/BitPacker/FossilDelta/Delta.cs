using System;
using PurrNet.Modules;
using PurrNet.Packing;

namespace Fossil
{
    public enum DeltaOp : byte
    {
        At,
        Colon,
        Semicolon
    }

    public static class Delta
    {
        [UsedByIL]
        static void DeltaOpWrite(BitPacker zDelta, DeltaOp op)
        {
            zDelta.WriteBits((byte)op, 2);
        }

        [UsedByIL]
        static void DeltaOpReader(BitPacker zDelta, ref DeltaOp op)
        {
            op = (DeltaOp)zDelta.ReadBits(2);
        }

        public const ushort NHASH = 16;

        static readonly RollingHash _rollingHash = new RollingHash();

        static int[] _collide = new int[1024];
        static int[] _landmark = new int[1024];

        public static void Create(ReadOnlySpan<byte> origin, ReadOnlySpan<byte> target, BitPacker zDelta)
        {
            int i, lastRead = -1;

            var originLength = origin.Length;
            var targetLength = target.Length;

            // For very small files, just write target as-is
            if (originLength <= NHASH)
            {
                Packer<PackedUInt>.Write(zDelta, (uint)targetLength);
                Packer<DeltaOp>.Write(zDelta, DeltaOp.Colon);
                zDelta.WriteBytes(target);
                Packer<PackedUInt>.Write(zDelta, Checksum(target));
                Packer<DeltaOp>.Write(zDelta, DeltaOp.Semicolon);
                return;
            }

            // Compute the hash table used to locate matching sections in the source.
            int nHash = originLength / NHASH;

            if (nHash > _collide.Length)
            {
                Array.Resize(ref _collide, nHash);
                Array.Resize(ref _landmark, nHash);
            }

            for (i = 0; i < nHash; i++) _collide[i] = -1;
            for (i = 0; i < nHash; i++) _landmark[i] = -1;
            int hv;

            for (i = 0; i < originLength - NHASH; i += NHASH)
            {
                _rollingHash.Init(origin, i);
                hv = (int)(_rollingHash.Value() % nHash);
                _collide[i / NHASH] = _landmark[hv];
                _landmark[hv] = i / NHASH;
            }

            int _base = 0;
            int iSrc, iBlock;
            int bestCnt, bestOfst, bestLitsz;
            while (_base + NHASH < targetLength)
            {
                bestOfst = 0;
                bestLitsz = 0;
                _rollingHash.Init(target, _base);
                i = 0; // Trying to match a landmark against zOut[_base+i]
                bestCnt = 0;
                while (true)
                {
                    int limit = 250;
                    hv = (int)(_rollingHash.Value() % nHash);
                    iBlock = _landmark[hv];
                    while (iBlock >= 0 && (limit--) > 0)
                    {
                        int cnt, litsz;
                        int j, k, x, y;

                        iSrc = iBlock * NHASH;
                        for (j = 0, x = iSrc, y = _base + i; x < originLength && y < targetLength; j++, x++, y++)
                        {
                            if (origin[x] != target[y]) break;
                        }

                        j--;

                        // Beginning at iSrc-1, match backwards as far as we can.
                        // k counts the number of characters that match.
                        for (k = 1; k < iSrc && k <= i; k++)
                        {
                            if (origin[iSrc - k] != target[_base + i - k]) break;
                        }

                        k--;

                        // Compute the offset and size of the matching region.
                        cnt = j + k + 1;
                        litsz = i - k; // Number of bytes of literal text before the copy
                        // sz will hold the number of bytes needed to encode the "insert"
                        // command and the copy command, not counting the "insert" text.

                        const int MIN_MATCH_LENGTH = 4;
                        if (cnt >= MIN_MATCH_LENGTH && cnt > bestCnt)
                        {
                            // Remember this match only if it is the best so far and it
                            // does not increase the file size.
                            bestCnt = cnt;
                            bestOfst = iSrc - k;
                            bestLitsz = litsz;
                        }

                        // Check the next matching block
                        iBlock = _collide[iBlock];
                    }

                    // We have a copy command that does not cause the delta to be larger
                    // than a literal insert.  So add the copy command to the delta.
                    if (bestCnt > 0)
                    {
                        if (bestLitsz > 0)
                        {
                            // Add an insert command before the copy.
                            Packer<PackedUInt>.Write(zDelta, (uint)bestLitsz);
                            Packer<DeltaOp>.Write(zDelta, DeltaOp.Colon);
                            zDelta.WriteBytes(target.Slice(_base, bestLitsz));
                            _base += bestLitsz;
                        }

                        _base += bestCnt;
                        Packer<PackedUInt>.Write(zDelta, (uint)bestCnt);
                        Packer<DeltaOp>.Write(zDelta, DeltaOp.At);
                        Packer<PackedUInt>.Write(zDelta, (uint)bestOfst);
                        if (bestOfst + bestCnt - 1 > lastRead)
                        {
                            lastRead = bestOfst + bestCnt - 1;
                        }

                        break;
                    }

                    // If we reach this point, it means no match is found so far
                    if (_base + i + NHASH >= targetLength)
                    {
                        // We have reached the end and have not found any
                        // matches.  Do an "insert" for everything that does not match
                        Packer<PackedUInt>.Write(zDelta, (uint)(targetLength - _base));
                        Packer<DeltaOp>.Write(zDelta, DeltaOp.Colon);
                        zDelta.WriteBytes(target.Slice(_base, targetLength - _base));
                        _base = targetLength;
                        break;
                    }

                    // Advance the hash by one character. Keep looking for a match.
                    _rollingHash.Next(target[_base + i + NHASH]);
                    i++;
                }
            }

            // Output a final "insert" record to get all the text at the end of
            // the file that does not match anything in the source.
            if (_base < targetLength)
            {
                Packer<PackedUInt>.Write(zDelta, (uint)(targetLength - _base));
                Packer<DeltaOp>.Write(zDelta, DeltaOp.Colon);
                zDelta.WriteBytes(target.Slice(_base, targetLength - _base));
            }

            Packer<PackedUInt>.Write(zDelta, Checksum(target));
            Packer<DeltaOp>.Write(zDelta, DeltaOp.Semicolon);
        }

        public static void Apply(ReadOnlySpan<byte> origin, BitPacker delta, ReadOnlySpan<byte> deltaRaw,
            BitPacker zOut)
        {
            uint lenDelta = (uint)deltaRaw.Length * 8;
            PackedUInt cache = default;

            while (delta.positionInBits < lenDelta)
            {
                uint cnt, ofst;

                Packer<PackedUInt>.Read(delta, ref cache);
                cnt = cache;

                DeltaOp op = default;
                Packer<DeltaOp>.Read(delta, ref op);

                switch (op)
                {
                    case DeltaOp.At:
                        Packer<PackedUInt>.Read(delta, ref cache);
                        ofst = cache;

                        if (ofst + cnt > origin.Length)
                            throw new Exception(
                                $"Copy command exceeds origin bounds: offset={ofst}, count={cnt}, length={origin.Length}");
                        zOut.WriteBytes(origin.Slice((int)ofst, (int)cnt));
                        break;

                    case DeltaOp.Colon:
                        if (delta.positionInBits + cnt * 8 > deltaRaw.Length * 8)
                            throw new Exception(
                                $"Insert command exceeds delta bounds: pos={delta.positionInBits}, count={cnt * 8}, length={deltaRaw.Length * 8}");
                        zOut.WriteBytes(delta, (int)cnt);
                        break;

                    case DeltaOp.Semicolon: return;
                    default:
                        throw new Exception($"Unknown delta operation: {op}");
                }
            }
        }

        // Return a 32-bit checksum of the array.
        static uint Checksum(ReadOnlySpan<byte> arr, int count = 0, uint sum = 0)
        {
            uint sum0 = 0, sum1 = 0, sum2 = 0, N = (uint)(count == 0 ? arr.Length : count);

            int z = 0;

            while (N >= 16)
            {
                sum0 += (uint)arr[z + 0] + arr[z + 4] + arr[z + 8] + arr[z + 12];
                sum1 += (uint)arr[z + 1] + arr[z + 5] + arr[z + 9] + arr[z + 13];
                sum2 += (uint)arr[z + 2] + arr[z + 6] + arr[z + 10] + arr[z + 14];
                sum += (uint)arr[z + 3] + arr[z + 7] + arr[z + 11] + arr[z + 15];
                z += 16;
                N -= 16;
            }

            while (N >= 4)
            {
                sum0 += arr[z + 0];
                sum1 += arr[z + 1];
                sum2 += arr[z + 2];
                sum += arr[z + 3];
                z += 4;
                N -= 4;
            }

            sum += (sum2 << 8) + (sum1 << 16) + (sum0 << 24);
            switch (N & 3)
            {
                case 3:
                    sum += (uint)(arr[z + 2] << 8);
                    sum += (uint)(arr[z + 1] << 16);
                    sum += (uint)(arr[z + 0] << 24);
                    break;
                case 2:
                    sum += (uint)(arr[z + 1] << 16);
                    sum += (uint)(arr[z + 0] << 24);
                    break;
                case 1:
                    sum += (uint)(arr[z + 0] << 24);
                    break;
            }

            return sum;
        }
    }
}
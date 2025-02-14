using NUnit.Framework;
using PurrNet.Packing;

public class ZigZagEncodingTests
{
    [Test]
    public void TestZigZagBitPatterns()
    {
        var v= PackedUintSerializer.ZigzagEncode(1);
        var vdec = PackedUintSerializer.ZigzagDecode(v);
        Assert.That(vdec, Is.EqualTo(1), "Failed to zigzag encode 1");
        
        var v1= PackedUintSerializer.ZigzagEncode(-1);
        var vdec1 = PackedUintSerializer.ZigzagDecode(v1);
        Assert.That(vdec1, Is.EqualTo(-1), "Failed to zigzag encode -1");
    }
}
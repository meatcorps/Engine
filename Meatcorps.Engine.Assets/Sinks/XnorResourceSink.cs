using Meatcorps.Engine.Assets.Interfaces;

namespace Meatcorps.Engine.Assets.Sinks;

public class XnorResourceSink: IEncryptDecryptSink
{
    private readonly byte[] _keys;

    public XnorResourceSink(byte key)
    {
        _keys = new[] { key };
    }

    public XnorResourceSink(byte[] keys)
    {
        if (keys == null || keys.Length == 0)
            throw new ArgumentException("XNOR keys cannot be null or empty.");

        _keys = keys;
    }

    // encrypt == decrypt for XNOR
    public byte[] Encrypt(byte[] data)
        => XnorTransform(data);

    public byte[] Decrypt(byte[] data)
        => XnorTransform(data);

    public int Decrypt(byte[] data, byte[] to)
        => XnorTransform(data, to);

    private byte[] XnorTransform(byte[] data)
    {
        var output = new byte[data.Length];
        XnorTransform(data, output);
        return output;
    }

    private int XnorTransform(byte[] src, byte[] dst)
    {
        if (dst.Length < src.Length)
            throw new ArgumentException("Destination buffer too small.");

        for (var i = 0; i < src.Length; i++)
        {
            var key = _keys[i % _keys.Length];
            dst[i] = (byte)~(src[i] ^ key);
        }

        return src.Length;
    }
}
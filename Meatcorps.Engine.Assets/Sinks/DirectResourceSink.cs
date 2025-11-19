using Meatcorps.Engine.Assets.Interfaces;

namespace Meatcorps.Engine.Assets.Sinks;

public class DirectResourceSink: IEncryptDecryptSink
{
    public byte[] Encrypt(byte[] data)
    {
        return data;
    }

    public byte[] Decrypt(byte[] data)
    {
        return data;
    }
}
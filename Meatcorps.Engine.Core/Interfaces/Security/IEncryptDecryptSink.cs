namespace Meatcorps.Engine.Core.Interfaces.Security;

public interface IEncryptDecryptSink
{
    byte[] Encrypt(byte[] data);
    byte[] Decrypt(byte[] data);
    int Decrypt(byte[] data, byte[] to);
}
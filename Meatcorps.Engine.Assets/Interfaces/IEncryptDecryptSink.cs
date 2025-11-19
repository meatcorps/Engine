namespace Meatcorps.Engine.Assets.Interfaces;

public interface IEncryptDecryptSink
{
    byte[] Encrypt(byte[] data);
    byte[] Decrypt(byte[] data);
}
using System.Security.Cryptography;
using Meatcorps.Engine.Assets.Interfaces;

namespace Meatcorps.Engine.Assets.Sinks;

public class AesResourceSink: IEncryptDecryptSink
{
    private string _password;

    public AesResourceSink(string password)
    {
        _password = password;
    }
    
    /// <summary>
    /// AES-256 + PBKDF2 (password -> key)
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public byte[] Encrypt(byte[] data)
    {
        var salt = RandomNumberGenerator.GetBytes(16);

        using var keyDerivation = new Rfc2898DeriveBytes(
            _password,
            salt,
            100_000,
            HashAlgorithmName.SHA256);

        var key = keyDerivation.GetBytes(32);
        var iv = keyDerivation.GetBytes(16);
        
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        var cipher = encryptor.TransformFinalBlock(data, 0, data.Length);

        var output = new byte[salt.Length + iv.Length + cipher.Length];
        Buffer.BlockCopy(salt, 0, output, 0, salt.Length);
        Buffer.BlockCopy(iv, 0, output, salt.Length, iv.Length);
        Buffer.BlockCopy(cipher, 0, output, salt.Length + iv.Length, cipher.Length);
        
        return output;
    }

    /// <summary>
    /// AES-256 + PBKDF2 (password -> key)
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public byte[] Decrypt(byte[] data)
    {
        var salt = new byte[16];
        var iv = new byte[16];

        Buffer.BlockCopy(data, 0, salt, 0, salt.Length);
        Buffer.BlockCopy(data, salt.Length, iv, 0, iv.Length);

        var cipher = new byte[data.Length - salt.Length - iv.Length];
        Buffer.BlockCopy(data, salt.Length + iv.Length, cipher, 0, cipher.Length);

        using var keyDerivation = new Rfc2898DeriveBytes(
            _password,
            salt,
            100_000,
            HashAlgorithmName.SHA256);
        
        var key = keyDerivation.GetBytes(32);
        
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
    }

    public int Decrypt(byte[] data, byte[] to)
    {
        var salt = new byte[16];
        var iv = new byte[16];

        Buffer.BlockCopy(data, 0, salt, 0, 16);
        Buffer.BlockCopy(data, 16, iv, 0, 16);

        var cipherOffset = 32;
        var cipherRemaining = data.Length - cipherOffset;

        using var keyDerivation = new Rfc2898DeriveBytes(
            _password, salt, 100_000, HashAlgorithmName.SHA256);

        var key = keyDerivation.GetBytes(32);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();

        var inputOffset = cipherOffset;
        var outputOffset = 0;

        var blockSize = aes.BlockSize / 8;   // 16 bytes for AES
        var chunkSize = blockSize * 1024;    // 16KB per chunk

        while (cipherRemaining > chunkSize)
        {
            var written = decryptor.TransformBlock(
                data,
                inputOffset,
                chunkSize,
                to,
                outputOffset);

            inputOffset     += chunkSize;
            cipherRemaining -= chunkSize;
            outputOffset    += written;
        }

        var final = decryptor.TransformFinalBlock(
            data,
            inputOffset,
            cipherRemaining);

        Buffer.BlockCopy(final, 0, to, outputOffset, final.Length);
        outputOffset += final.Length;

        return outputOffset; 
    }
}
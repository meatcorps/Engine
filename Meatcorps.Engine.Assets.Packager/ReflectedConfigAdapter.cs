using System.Collections;
using System.Reflection;
using Meatcorps.Engine.Assets.Interfaces;
using Meatcorps.Engine.Core.Interfaces.Security;

namespace Meatcorps.Engine.Assets.Packager;

internal sealed class ReflectedConfigAdapter : IAssetPackagerConfig
{
    private readonly object _inner;
    private readonly PropertyInfo _encryptProp;
    private readonly PropertyInfo _packsProp;

    public ReflectedConfigAdapter(object inner)
    {
        _inner = inner;
        var t = inner.GetType();

        _encryptProp = t.GetProperty("EncryptDecryptSink")
                       ?? throw new InvalidOperationException("EncryptDecryptSink property not found");
        _packsProp   = t.GetProperty("AssetPacks")
                       ?? throw new InvalidOperationException("AssetPacks property not found");
    }

    public IEncryptDecryptSink EncryptDecryptSink
    {
        get
        {
            var sinkObj = _encryptProp.GetValue(_inner)
                         ?? throw new InvalidOperationException("EncryptDecryptSink is null");
            return new ReflectedEncryptDecryptSink(sinkObj);
        }
    }

    public List<AssetPack> AssetPacks
    {
        get
        {
            // If AssetPack is also from the shared library and identity matches, you can just cast.
            // Otherwise, map manually into your own AssetPack DTO.
            var value = _packsProp.GetValue(_inner)
                        ?? throw new InvalidOperationException("AssetPacks is null");

            return ((IEnumerable)value)
                .Cast<object>()
                .Select(CloneAssetPack)
                .ToList();
        }
    }

    private static AssetPack CloneAssetPack(object src)
    {
        var t = src.GetType();
        var nameProp = t.GetProperty("Name")!;
        var pathProp = t.GetProperty("RelativePath")!;
        var sinkProp = t.GetProperty("EncryptDecryptSink")!;

        var name = (string)nameProp.GetValue(src)!;
        var path = (string)pathProp.GetValue(src)!;
        var sinkObj = sinkProp.GetValue(src);

        return new AssetPack
        {
            Name = name,
            RelativePath = path,
            EncryptDecryptSink = sinkObj is null
                ? null
                : new ReflectedEncryptDecryptSink(sinkObj)
        };
    }
}

internal sealed class ReflectedEncryptDecryptSink : IEncryptDecryptSink
{
    private readonly object _inner;
    private readonly MethodInfo _encrypt;
    private readonly MethodInfo _decrypt;

    public ReflectedEncryptDecryptSink(object inner)
    {
        _inner = inner;
        var t = inner.GetType();

        _encrypt = t.GetMethod("Encrypt", new[] { typeof(byte[]) })
                   ?? throw new InvalidOperationException("Encrypt(byte[]) not found");

        _decrypt = t.GetMethod("Decrypt", new[] { typeof(byte[]) })
                   ?? throw new InvalidOperationException("Decrypt(byte[]) not found");
    }

    public byte[] Encrypt(byte[] data)
        => (byte[])_encrypt.Invoke(_inner, new object[] { data })!;

    public byte[] Decrypt(byte[] data)
        => (byte[])_decrypt.Invoke(_inner, new object[] { data })!;

    public int Decrypt(byte[] data, byte[] to)
        => (int)_decrypt.Invoke(_inner, new object[] { data, to })!;
}
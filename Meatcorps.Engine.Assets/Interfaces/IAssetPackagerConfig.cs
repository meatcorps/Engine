using Meatcorps.Engine.Core.Interfaces.Security;

namespace Meatcorps.Engine.Assets.Interfaces;

public interface IAssetPackagerConfig
{
    IEncryptDecryptSink EncryptDecryptSink { get; }
    List<AssetPack> AssetPacks { get; }
}

public struct AssetPack
{
    public string Name { get; init; }
    public string RelativePath { get; init; }
    public IEncryptDecryptSink? EncryptDecryptSink { get; init; }
}
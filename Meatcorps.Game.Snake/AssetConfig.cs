using Meatcorps.Engine.Assets.Interfaces;
using Meatcorps.Engine.Core.Interfaces.Security;
using Meatcorps.Engine.Core.Security.Sinks;

namespace Meatcorps.Game.Snake;

public class AssetConfig: IAssetPackagerConfig
{
    public IEncryptDecryptSink EncryptDecryptSink => new AesResourceSink("HELLOWORLD");
    public List<AssetPack> AssetPacks => new List<AssetPack>(
    [
        new AssetPack
        {
            EncryptDecryptSink       = null,
            Name = "Asset.pak",
            RelativePath = "Assets"
        }
    ]);
}
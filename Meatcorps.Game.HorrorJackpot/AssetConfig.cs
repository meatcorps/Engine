using Meatcorps.Engine.Assets.Interfaces;
using Meatcorps.Engine.Assets.Sinks;

namespace Meatcorps.Game.HorrorJackpot;

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
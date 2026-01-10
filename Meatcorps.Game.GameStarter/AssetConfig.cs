using Meatcorps.Engine.Assets.Interfaces;
using Meatcorps.Engine.Core.Interfaces.Security;
using Meatcorps.Engine.Core.Security.Sinks;

namespace Meatcorps.Game.GameStarter;

public class AssetConfig: IAssetPackagerConfig
{
    public IEncryptDecryptSink EncryptDecryptSink => new AesResourceSink("CHANGETHIS!");
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
using Meatcorps.Engine.Assets.Interfaces;
using Meatcorps.Engine.Assets.Manager;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Interfaces;

namespace Meatcorps.Engine.RayLib.Assets.Modules;

public static class RaylibAssetsModule
{
    public static void Load(IAssetPackagerConfig config)
    {
        GlobalObjectManager.ObjectManager.Register(config);

        var firstAsset = config.AssetPacks.First();
        var firstAssetInstance =
            new PackageResourceLoader(new AssetPackageManager(firstAsset.Name, firstAsset.EncryptDecryptSink ?? config.EncryptDecryptSink));
        GlobalObjectManager.ObjectManager.Register<IRaylibResource>(firstAssetInstance);
        
        foreach (var assetPack in config.AssetPacks)
            GlobalObjectManager.ObjectManager.Register<IRaylibResource>(firstAssetInstance, assetPack.Name);
    }
}
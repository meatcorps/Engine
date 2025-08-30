using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Storage.Services;

namespace Meatcorps.Engine.Core.Storage.Module;

public static class StorageModule
{
    public static void Load(ObjectManager.ObjectManager manager)
    {
        if (manager == GlobalObjectManager.ObjectManager)
        {
            manager.Register(new PersistentLoaderAndSaverService());
            manager.Register(new PersistentDatabase(manager.Get<PersistentLoaderAndSaverService>()!,
                manager.Get<PersistentLoaderAndSaverService>()!));

        }

        manager.Register(new MemoryDatabaseService());
        manager.Register(new DataCollectionService(manager));
    }
}
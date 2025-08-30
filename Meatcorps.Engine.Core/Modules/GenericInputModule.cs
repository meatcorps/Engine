using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Core.Interfaces.Input;
using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.ObjectManager;

namespace Meatcorps.Engine.Core.Modules;

public class GenericInputModule
{
    public static GenericInputModule Create<T>(GenericMapper<T> mapper, int maxPlayers) where T : Enum
    {
        
        GlobalObjectManager.ObjectManager.RegisterOnce(new PlayerInputRouter<T>());
        var router = GlobalObjectManager.ObjectManager.Get<PlayerInputRouter<T>>()!;
        
        for (var i = 0; i < maxPlayers; i++)
            router.AssignMapper(i + 1, mapper);

        GlobalObjectManager.ObjectManager.Add<IBackgroundService>(mapper);
        
        return new GenericInputModule();
    }
}
using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Session.Factories;
using Meatcorps.Engine.Session.Interfaces;

namespace Meatcorps.Engine.Session.Modules;

public class SessionModule
{
    public static SessionModule Create<TEnumSession, TEnumPlayer>(SessionFactory<TEnumSession, TEnumPlayer> factory) where TEnumSession : Enum where TEnumPlayer : Enum
    {
        var sessionService = new SessionService<TEnumSession, TEnumPlayer>(factory);
        GlobalObjectManager.ObjectManager.Register(sessionService);
        GlobalObjectManager.ObjectManager.Register<ISessionService>(sessionService);
        GlobalObjectManager.ObjectManager.Add<IBackgroundService>(sessionService);
        GlobalObjectManager.ObjectManager.RegisterList<ISessionTracker<TEnumSession, TEnumPlayer>>();
        return new SessionModule();
    }
    
    public SessionModule RegisterTracker<TEnumSession, TEnumPlayer>(ISessionTracker<TEnumSession, TEnumPlayer> tracker) where TEnumSession : Enum where TEnumPlayer : Enum
    {
        GlobalObjectManager.ObjectManager.Add(tracker);
        return this;
    }
}
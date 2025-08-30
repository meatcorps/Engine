using System.Globalization;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Session.Data;
using Meatcorps.Engine.Session.Interfaces;
using Meatcorps.Engine.Session.ValueTypes;

namespace Meatcorps.Engine.Session.Factories;

public class SessionFactory<TEnumSession, TEnumPlayer> 
    where TEnumSession : Enum
    where TEnumPlayer : Enum 
{
    private Func<SessionDataBag<TEnumSession>>? _sessionDataFactory;
    private Func<SessionDataBag<TEnumPlayer>>? _playerSessionDataFactory;
    public int MaxPlayers { get; private set; } = 1;

    public SessionFactory()
    {
    }

    public SessionFactory(Func<SessionDataBag<TEnumSession>> sessionDataFactory,
        Func<SessionDataBag<TEnumPlayer>> playerSessionDataFactory, int maxPlayers = 2)
    {
        _sessionDataFactory = sessionDataFactory;
        _playerSessionDataFactory = playerSessionDataFactory;
        MaxPlayers = maxPlayers;
    }
    
    public SessionFactory<TEnumSession, TEnumPlayer> SetMaxPlayers(int maxPlayers)
    {
        MaxPlayers = maxPlayers;
        return this;
    }

    public SessionFactory<TEnumSession, TEnumPlayer> SetSessionDataFactory(
        Func<SessionDataBag<TEnumSession>> sessionDataFactory)
    {
        _sessionDataFactory = sessionDataFactory;
        return this;
    }

    public SessionFactory<TEnumSession, TEnumPlayer> SetPlayerSessionDataFactory(
        Func<SessionDataBag<TEnumPlayer>> playerSessionDataFactory)
    {
        _playerSessionDataFactory = playerSessionDataFactory;
        return this;
    }

    public SessionSet<TEnumSession, TEnumPlayer> GenerateSessionSet(int seed = 0)
    {
        return new SessionSet<TEnumSession, TEnumPlayer>(this, seed, MaxPlayers);
    }
    
    public SessionDataBag<TEnumSession> GenerateSessionData(int seed = 0)
    {
        if (_sessionDataFactory == null)
            throw new Exception("Session data factory not set");
        var data = _sessionDataFactory();
        if (seed == 0)
            seed = new Random().Next();
        data.RegisterItem(new SessionDataItemUniversal<int>(SessionDefaultTypes.SessionSeed, seed));
        data.RegisterItem(new SessionDataItemUniversalDate(SessionDefaultTypes.SessionStarted));
        return data;
    }
    
    public SessionDataBag<TEnumPlayer> GeneratePlayerSessionData(int playerId, string playerName)
    {
        if (_playerSessionDataFactory == null)
            throw new Exception("Player session data factory not set");
        var data = _playerSessionDataFactory();
        data.RegisterItem(new SessionDataItemUniversal<int>(PlayerDefaultTypes.PlayerId, playerId));
        data.RegisterItem(new SessionDataItemUniversal<string>(PlayerDefaultTypes.PlayerName, playerName));
        return data;
    }

    public SessionFactory<TEnumSession, TEnumPlayer> RegisterTracker(ISessionTracker<TEnumSession, TEnumPlayer> tracker)
    {
        GlobalObjectManager.ObjectManager.RegisterList<ISessionTracker<TEnumSession, TEnumPlayer>>();
        GlobalObjectManager.ObjectManager.Add<ISessionTracker<TEnumSession, TEnumPlayer>>(tracker);
        return this;
    }
}
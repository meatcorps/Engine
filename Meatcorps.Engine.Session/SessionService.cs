using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Session.Data;
using Meatcorps.Engine.Session.Factories;
using Meatcorps.Engine.Session.Interfaces;

namespace Meatcorps.Engine.Session;

public sealed class SessionService<TEnumSession, TEnumPlayer>: IBackgroundService, IDisposable, ISessionService where TEnumSession : Enum
    where TEnumPlayer : Enum 
{
    private readonly SessionFactory<TEnumSession, TEnumPlayer> _factory;
    private bool _stopCalled;
    public SessionSet<TEnumSession, TEnumPlayer> CurrentSession { get; private set; }
    public int Seed { get; set; } = 0;
    public int MaxPlayers => _factory.MaxPlayers;
    
    private bool _isDisposed;
    
    public SessionService(SessionFactory<TEnumSession, TEnumPlayer> factory)
    {
        _factory = factory;
        // We do not notify the trackers for the first one. To start a service, they should call StartSession.
        // This is just a temporary placeholder
        CurrentSession = _factory.GenerateSessionSet(Seed);
    }

    public void StartSession(int seed)
    {
        Seed = seed;
        var oldSession = CurrentSession;
        StartSession();
        if (!_stopCalled)
        {
            SessionStopped(oldSession);
            _stopCalled = false;
        }
    }
    
    public void StartSession()
    {
        CurrentSession.Reset(true);
        CurrentSession = _factory.GenerateSessionSet(Seed);
        foreach (var tracker in GetSessionTrackers())
            tracker.SessionStarted(CurrentSession);
    }

    public SessionDataBag<TEnumPlayer> CreateTemporaryPlayer(int id, string playerName)
    {
        var playerId = Guid.NewGuid().GetHashCode();
        var data = _factory.GeneratePlayerSessionData(playerId, playerName);
        return data;
    }
    
    public bool TryPlayerJoin(string playerName)
    {
        return TryPlayerJoin(Guid.NewGuid().GetHashCode(), playerName);
    }

    public bool TryPlayerJoin(int playerId, string playerName)
    {
        var result = CurrentSession.TryAddPlayer(playerId, playerName, out var data);
        if (result)
            PlayerJoined(CurrentSession.SessionData, data, CurrentSession.TotalPlayers);
        return result;
    }

    public bool TryPlayerJoin(string playerName, out SessionDataBag<TEnumPlayer>? data)
    {
        return TryPlayerJoin(Guid.NewGuid().GetHashCode(), playerName, out data);
    }

    public bool TryPlayerJoin(int playerId, string playerName, out SessionDataBag<TEnumPlayer>? data)
    {
        var result = CurrentSession.TryAddPlayer(playerId, playerName, out data);
        if (result)
            PlayerJoined(CurrentSession.SessionData, data, CurrentSession.TotalPlayers);
        return result;
    }

    public bool TryPlayerDrop(int playerId)
    {
        var result = CurrentSession.TryGetPlayerData(playerId, out _, out var data);
        if (result)
            PlayerLeft(CurrentSession.SessionData, data);
        return result;
    }

    public void StopSession()
    {
        _stopCalled = true;
        SessionStopped(CurrentSession);
    }
    
    public void ResetSession()
    {
        CurrentSession.Reset();
    }

    public void ClearPlayers()
    {
        foreach (var player in CurrentSession.PlayerData)
            PlayerLeft(CurrentSession.SessionData, player);
        CurrentSession.ClearPlayers();
    }


    public void SessionStopped(SessionSet<TEnumSession, TEnumPlayer> session)
    {
        foreach (var tracker in GetSessionTrackers())
            tracker.SessionEnded(session);
    }

    public void PlayerJoined(SessionDataBag<TEnumSession> session, SessionDataBag<TEnumPlayer> data, int totalPlayers)
    {
        foreach (var tracker in GetSessionTrackers())
            tracker.PlayerJoined(session, data, totalPlayers);
    }

    public void PlayerLeft(SessionDataBag<TEnumSession> session, SessionDataBag<TEnumPlayer> data)
    {
        foreach (var tracker in GetSessionTrackers())
            tracker.PlayerLeft(session, data);
    }

    public IEnumerable<SessionDataBag<TEnumPlayer>> AllCurrentPlayers()
    {
        return CurrentSession.PlayerData;
    }

    private IEnumerable<ISessionTracker<TEnumSession, TEnumPlayer>> GetSessionTrackers()
    {
        return GlobalObjectManager.ObjectManager.GetList<ISessionTracker<TEnumSession, TEnumPlayer>>()!;
    }
    
    public void PreUpdate(float deltaTime)
    {
    }

    public void Update(float deltaTime)
    {
    }

    public void LateUpdate(float deltaTime)
    {
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;
        
        _isDisposed = true;
        
        foreach (var player in CurrentSession.PlayerData)
            PlayerLeft(CurrentSession.SessionData, player);
        SessionStopped(CurrentSession);
    }
}
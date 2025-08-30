using Meatcorps.Engine.Session.Factories;
using Meatcorps.Engine.Session.ValueTypes;

namespace Meatcorps.Engine.Session.Data;

public class SessionSet<TEnumSession, TEnumPlayer> 
    where TEnumSession : Enum
    where TEnumPlayer : Enum 
{
    private SessionFactory<TEnumSession, TEnumPlayer> _factory;
    private int _maxPlayers;
    public SessionDataBag<TEnumSession> SessionData { get; }
    private List<SessionDataBag<TEnumPlayer>> _playerData { get; } = new();
    public IReadOnlyList<SessionDataBag<TEnumPlayer>> PlayerData => _playerData;

    public int Seed => SessionData.Get<int>(SessionDefaultTypes.SessionSeed);
    public string Started => SessionData.Get<string>(SessionDefaultTypes.SessionStarted);
    
    public int TotalPlayers => _playerData.Count;
    
    public SessionSet(SessionFactory<TEnumSession, TEnumPlayer> factory, int seed = 0, int maxPlayers = 2)
    {
        _maxPlayers = maxPlayers;
        _factory = factory;
        SessionData = factory.GenerateSessionData(seed);
    }

    public bool TryAddPlayer(int playerId, string playerName, out SessionDataBag<TEnumPlayer>? playerData)
    {
        playerData = null;
        if (_playerData.Count >= _maxPlayers)
            return false;

        foreach (var player in _playerData)
        {
            if (player.Get<int>(PlayerDefaultTypes.PlayerId) == playerId)
                return false;
        }
        
        playerData = _factory.GeneratePlayerSessionData(playerId, playerName);
        _playerData.Add(playerData);
        return true;
    }

    public void DropPlayer(int playerId)
    {
        for (var i = 0; i < _playerData.Count; i++)
        {
            if (_playerData[i].Get<int>(PlayerDefaultTypes.PlayerId) == playerId)
            {
                _playerData.RemoveAt(i);
                return;
            }
        }
    }

    public bool TryGetPlayerData(int playerId, out string playerName, out SessionDataBag<TEnumPlayer> data)
    {
        foreach (var player in _playerData)
        {
            if (player.Get<int>(PlayerDefaultTypes.PlayerId) == playerId)
            {
                playerName = player.Get<string>(PlayerDefaultTypes.PlayerName);
                data = player;
                return true;
            }
        }
        playerName = "";
        data = null!;
        return false;
    }

    public void ClearPlayers()
    {
        _playerData.Clear();
    }
    
    public void Reset(bool removePlayers = false)
    {
        SessionData.Reset();
        if (removePlayers)
            _playerData.Clear();
        else
            foreach (var player in _playerData)
                player.Reset();
    }
    
}
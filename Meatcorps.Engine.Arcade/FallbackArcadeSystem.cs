using Meatcorps.Engine.Arcade.Data;
using Meatcorps.Engine.Arcade.Enums;
using Meatcorps.Engine.Arcade.Interfaces;
using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.ObjectManager;

namespace Meatcorps.Engine.Arcade;

public class FallbackArcadeSystem: IArcadePointsMutator, IPlayerCheckin, IBackgroundService
{
    private static FallbackArcadeSystem? _instance;
    public ArcadeGame Game { get; }
    public int GamePrice => Game.PricePoints;
    
    private readonly int _maxPlayers;
    private readonly int _startingPoints;
    private List<ArcadePlayer> _players = new();
    private Queue<ArcadePlayer> _playerQueue = new();
    private int _nextPlayer = 1;
    public int TotalPlayers => _players.Count;

    public bool RemovePlayersAtIdle { get; set; }

    public FallbackArcadeSystem(int totalPlayers = 2, int maxPlayers = 2, int startingPoints = 3000)
    {
        if (_instance != null)
            throw new InvalidOperationException("Only one instance of FallbackArcadePointMutator is allowed");
        _instance = this;
        
        Game = GlobalObjectManager.ObjectManager.Get<ArcadeGame>()!;
        _maxPlayers = maxPlayers;
        _startingPoints = startingPoints;

        for (var i = 0; i < totalPlayers; i++)
            _players.Add(new ArcadePlayer
            {
                Id = Guid.NewGuid(),
                Name = $"Player {_nextPlayer++}",
                Points = startingPoints
            });
    }


    public bool RequestPoints(int player)
    {
        return RequestPoints(player, GamePrice);
    }

    public int GetPoints(int player)
    {
        if (TryGetPlayer(player, out var points))
            return points!.Points;
        
        return 0;
    }

    public bool RequestPoints(int player, int points)
    {
        if (!TryGetPlayer(player, out var current))
            return false;
        
        if (current!.Points < points)
            return false;
        
        current!.Points -= points;
        return true;
    }

    public void SubmitPoints(int player, int points)
    {
        if (!TryGetPlayer(player, out var current))
            throw new InvalidOperationException($"Player {player} has no points");
        
        current!.Points += points;
    }

    public bool IsPlayerCheckedIn(int player, out string name)
    {
        if (TryGetPlayer(player, out var current))
        {
            name = current!.Name;
            return true;
        }

        name = string.Empty;
        return false;
    }

    public string GetPlayerName(int player)
    {
        if (TryGetPlayer(player, out var current))
            return current!.Name;
        
        throw new InvalidOperationException($"Player {player} has no name");
    }

    public void SignPlayerOut(int player)
    {
        if (TryGetPlayer(player, out var current))
        {
            _playerQueue.Enqueue(current!);
            _players.Remove(current!);
        }
    }

    public void SignPlayerIn()
    {
        if (_players.Count >= _maxPlayers)
            return;
        if (_playerQueue.TryDequeue(out var player))
            _players.Add(player);
    }

    private bool TryGetPlayer(int player, out ArcadePlayer? playerData)
    {
        playerData = null;
        
        if (player == 0 || player > _players.Count)
            return false;
        
        playerData = _players[player - 1];
        return true;
    }

    public void PreUpdate(float deltaTime)
    {
    }

    public void Update(float deltaTime)
    {
        if (Game.State == GameState.Idle && _players.Count > 0 && RemovePlayersAtIdle)
        {
            while (_players.Count > 0)
                SignPlayerOut(1);
        }
    }

    public void LateUpdate(float deltaTime)
    {
    }
}
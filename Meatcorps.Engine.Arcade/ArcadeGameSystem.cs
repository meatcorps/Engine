using Meatcorps.Engine.Arcade.Constants;
using Meatcorps.Engine.Arcade.Data;
using Meatcorps.Engine.Arcade.Enums;
using Meatcorps.Engine.Arcade.Interfaces;
using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.MQTT.Enums;
using Meatcorps.Engine.Signals.Data;

namespace Meatcorps.Engine.Arcade;

public sealed class ArcadeGameSystem: IPlayerCheckin, IArcadePointsMutator, IBackgroundService, IDisposable
{
    public ArcadeGame Game => _gameSignal.Value;
    private SignalValue<ArcadeGame, MQTTGroup> _gameSignal;
    private SignalValue<ArcadePlayer, MQTTGroup> _playerSignalIn;
    private List<ArcadePlayer> _players = new();
    private readonly SignalValue<ArcadePointChange, MQTTGroup> _pointChangeSignal;
    private readonly SignalValue<ArcadePlayer, MQTTGroup> _playerSignalOut;
    private FixedTimer _pushTimer = new FixedTimer(1000);
    private int _sessionTotalPlayers = 1;
    
    public void SetTotalPlayerSessions(int total)
    {
        _sessionTotalPlayers = total;
    }

    public int TotalPlayers => _players.Count;
    
    public ArcadeGameSystem()
    {
        _gameSignal = new SignalValue<ArcadeGame, MQTTGroup>(MQTTGroup.Exchange, ArcadeEndpointTopics.REGISTER_GAME);
        _pointChangeSignal = new SignalValue<ArcadePointChange, MQTTGroup>(MQTTGroup.Exchange, ArcadeEndpointTopics.CHANGE_POINTS);
        _playerSignalIn = new SignalValue<ArcadePlayer, MQTTGroup>(MQTTGroup.Exchange, ArcadeEndpointTopics.GAMESESSION_SIGNIN_AND_UPDATE);
        _playerSignalOut = new SignalValue<ArcadePlayer, MQTTGroup>(MQTTGroup.Exchange, ArcadeEndpointTopics.GAMESESSION_SIGNOUT);
        _playerSignalIn.ValueChanged += PlayerSignedIn;
    }

    private void PlayerSignedIn(ArcadePlayer player)
    {
        // To many players kick the last one out no money penality of-course
        if (_players.Count >= _sessionTotalPlayers && player.CurrentGame == Game.Code && !_players.Any(x => x.Id == player.Id))
        {
            Console.WriteLine("KICKING PLAYER: " + player.Name);
            _playerSignalOut.Value = new ArcadePlayer
            {
                Id = player.Id,
                Name = player.Name,
                Points = player.Points,
                CurrentGame = 0,
            };
            return;
        }
        
        var target = _players.FirstOrDefault(x => x.Id == player.Id);
        if (target is not null)
        {
            if (player.CurrentGame != Game.Code)
                _players.Remove(target);
                
            target.Points = player.Points;
            return;
        }
        
        if (player.CurrentGame != Game.Code && Game.MaxPlayers > _players.Count)
            return;
        
        _players.Add(player);
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
            _players.Remove(current!);
            _playerSignalOut.Value = new ArcadePlayer
            {
                Id = current.Id,
                Name = current.Name,
                Points = current.Points,
                CurrentGame = 0,
            };
        }
    }
    
    public void PreUpdate(float deltaTime)
    {
        
    }

    public void Update(float deltaTime)
    {
        if (Game.State == GameState.Idle && _players.Count > 0)
        {
            while (_players.Count > 0)
                SignPlayerOut(1);
        }
        _pushTimer.Update(deltaTime);
        if (_pushTimer.Output)
        {
            _gameSignal.Value.LastReported = DateTime.Now;
            _gameSignal.Push();
        }
    }

    public void LateUpdate(float deltaTime)
    {
    }
    
    private bool TryGetPlayer(int player, out ArcadePlayer? playerData)
    {
        playerData = null;
        
        if (player == 0 || player > _players.Count)
            return false;
        
        playerData = _players[player - 1];
        return true;
    }

    public int GamePrice => Game.PricePoints;
    
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
        _pointChangeSignal.Value = new ArcadePointChange
        {
            Value = -points,
            Id = current.Id,
        };
        return true;
    }

    public bool RequestPoints(int player)
    {
        return RequestPoints(player, GamePrice);
    }

    public void SubmitPoints(int player, int points)
    {
        if (!TryGetPlayer(player, out var current))
            throw new InvalidOperationException($"Player {player} has no points");
        
        current!.Points += points;
        _pointChangeSignal.Value = new ArcadePointChange
        {
            Value = points,
            Id = current.Id,
        };
    }

    public void Dispose()
    {
        _gameSignal.Dispose();
        _playerSignalIn.Dispose();
        _pointChangeSignal.Dispose();
        _playerSignalOut.Dispose();
    }
}
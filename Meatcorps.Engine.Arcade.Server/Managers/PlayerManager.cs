using Meatcorps.Engine.Arcade.Constants;
using Meatcorps.Engine.Arcade.Data;
using Meatcorps.Engine.MQTT.Enums;
using Meatcorps.Engine.Signals.Data;

namespace Meatcorps.Engine.Arcade.Server.Managers;

public class PlayerManager: BaseManager
{
    private readonly SignalValue<ArcadePointChange, MQTTGroup> _pointChangeSignal;
    private readonly SignalValue<ArcadePlayer, MQTTGroup> _playerSignalInAndUpdate;
    private readonly SignalValue<ArcadePlayer, MQTTGroup> _playerSignalOut;
    private readonly SignalValue<ArcadePlayer, MQTTGroup> _playerJoin;
    private readonly SignalValue<ArcadePlayer, MQTTGroup> _playerRegister;
    private object _playerUpdateLock = new();
    
    public PlayerManager()
    {
        // Out Signals
        _playerSignalInAndUpdate = new SignalValue<ArcadePlayer, MQTTGroup>(MQTTGroup.Exchange, ArcadeEndpointTopics.GAMESESSION_SIGNIN_AND_UPDATE);
        
        // In Signals
        _pointChangeSignal = new SignalValue<ArcadePointChange, MQTTGroup>(MQTTGroup.Exchange, ArcadeEndpointTopics.CHANGE_POINTS);
        _playerSignalOut = new SignalValue<ArcadePlayer, MQTTGroup>(MQTTGroup.Exchange, ArcadeEndpointTopics.GAMESESSION_SIGNOUT);
        _playerJoin = new SignalValue<ArcadePlayer, MQTTGroup>(MQTTGroup.Exchange, ArcadeEndpointTopics.JOIN_GAME);
        _playerRegister = new SignalValue<ArcadePlayer, MQTTGroup>(MQTTGroup.Exchange, ArcadeEndpointTopics.REGISTER_PLAYER);

        _pointChangeSignal.ValueChanged += PointChangeSignalOnValueChanged;
        _playerSignalOut.ValueChanged += PlayerSignalOutOnValueChanged;
        _playerJoin.ValueChanged += PlayerJoinOnValueChanged;
        _playerRegister.ValueChanged += PlayerRegisterOnValueChanged;
    }

    private void PlayerRegisterOnValueChanged(ArcadePlayer value)
    {
        var added = false;
        lock (_playerUpdateLock)
        {
            if (Data.Players.All(x => x.Id != value.Id))
            {
                Data.Players.Add(value);
                added = true;
            }
        }
        Push();
        
        if (added)
            _playerSignalInAndUpdate.Value = value;
    }

    private void PlayerJoinOnValueChanged(ArcadePlayer value)
    {
        lock (_playerUpdateLock)
        {
            var player = Data.Players.FirstOrDefault(x => x.Id == value.Id);
            if (player is null)
                return;
            
            player.CurrentGame = value.CurrentGame;
        }
        Push();
        _playerSignalInAndUpdate.Value = value;
    }

    private void PlayerSignalOutOnValueChanged(ArcadePlayer value)
    {
        lock (_playerUpdateLock)
        {
            var player = Data.Players.FirstOrDefault(x => x.Id == value.Id);
            if (player is null)
                return;
            
            player.CurrentGame = 0;
        }
        Push();
        _playerSignalInAndUpdate.Value = value;
    }

    private void PointChangeSignalOnValueChanged(ArcadePointChange value)
    {
        ArcadePlayer player;
        lock (_playerUpdateLock)
        {
            player = Data.Players.FirstOrDefault(x => x.Id == value.Id);
            if (player is null)
                return;
            
            player.Points += value.Value;
            if (player.Points < 0)
                player.Points = 0;
        }
        Push();
        _playerSignalInAndUpdate.Value = new ArcadePlayer
        {
            Id = player.Id,
            Name = player.Name,
            Points = player.Points,
            CurrentGame = player.CurrentGame,
        };
    }

    protected override void OnDispose()
    {
        _pointChangeSignal.Dispose();
        _playerSignalInAndUpdate.Dispose();
        _playerSignalOut.Dispose();
        _playerJoin.Dispose();
        _playerRegister.Dispose();
    }
}
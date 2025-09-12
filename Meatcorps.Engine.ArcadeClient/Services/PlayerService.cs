using System.Reactive.Linq;
using Meatcorps.Engine.Arcade.Constants;
using Meatcorps.Engine.Arcade.Data;
using Meatcorps.Engine.Arcade.Services;
using Meatcorps.Engine.MQTT.Enums;
using Meatcorps.Engine.Signals.Data;

namespace Meatcorps.Engine.ArcadeClient.Services;

public class PlayerService : IDisposable
{
    private readonly ArcadeDataService _arcadeDataService;
    private readonly SignalValue<ArcadePlayer, MQTTGroup> _registerPlayerSignal;
    private readonly SignalValue<ArcadePlayer, MQTTGroup> _joinPlayerSignal;
    private readonly SignalValue<ArcadePointChange, MQTTGroup> _pointChangeSignal;

    public PlayerService(ArcadeDataService arcadeDataService)
    {
        _arcadeDataService = arcadeDataService;
        _registerPlayerSignal = new SignalValue<ArcadePlayer, MQTTGroup>(MQTTGroup.Exchange, ArcadeEndpointTopics.REGISTER_PLAYER);
        _joinPlayerSignal = new SignalValue<ArcadePlayer, MQTTGroup>(MQTTGroup.Exchange, ArcadeEndpointTopics.JOIN_GAME);
        _pointChangeSignal = new SignalValue<ArcadePointChange, MQTTGroup>(MQTTGroup.Exchange, ArcadeEndpointTopics.CHANGE_POINTS);
    }

    public async Task<bool> RegisterPlayerAsync(string id, string name)
    {
        if (_arcadeDataService.TryGetPlayer(id, out var player))
            return true;
        
        var waitForPlayerRegistration = _arcadeDataService
            .PlayerUpdate
            .Where(x => x.Id == id)
            .Timeout(TimeSpan.FromSeconds(5));
        
        _registerPlayerSignal.Value = new ArcadePlayer
        {
            Id = id,
            Name = name,
            Points = 3000,
            CurrentGame = 0,
        };

        try
        {
            await waitForPlayerRegistration.FirstAsync().GetAwaiter();
        } 
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
        
        return true;
    }

    public void MutatePoints(string id, int points)
    {
        if (!_arcadeDataService.TryGetPlayer(id, out var _))
            return;
        
        _pointChangeSignal.Value = new ArcadePointChange
        {
            Id = id,
            Value = points,
        };
    }
    
    public async Task<bool> JoinGameAsync(int gameCode, string id)
    {
        if (!_arcadeDataService.TryGetPlayer(id, out var player))
            return false;
        
        var waitForPlayerJoined = _arcadeDataService
            .PlayerUpdate
            .Where(x => x.Id == id && x.CurrentGame == gameCode)
            .Timeout(TimeSpan.FromSeconds(5));
        
        player!.CurrentGame = gameCode;
        
        _joinPlayerSignal.Value = new ArcadePlayer
        {
            Id = id,
            Name = player.Name,
            Points = player.Points,
            CurrentGame = gameCode,
        };
        
        try
        {
            await waitForPlayerJoined.FirstAsync().GetAwaiter();
        } 
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
        
        return true;
    }

    public void Dispose()
    {
        _pointChangeSignal.Dispose();
        _registerPlayerSignal.Dispose();
    }
}
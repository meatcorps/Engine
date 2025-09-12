using System.Reactive.Linq;
using Meatcorps.Engine.Arcade.Constants;
using Meatcorps.Engine.Arcade.Data;
using Meatcorps.Engine.Arcade.Enums;
using Meatcorps.Engine.MQTT.Enums;
using Meatcorps.Engine.Signals.Data;

namespace Meatcorps.Engine.Arcade.Server.Managers;

public class GamesManager : BaseManager
{
    private readonly SignalValue<ArcadeGame, MQTTGroup> _gameSignal;
    private object _gameUpdateLock = new();
    private IDisposable _gameCheckTimer;
    private readonly SignalValue<ArcadeCentralData, MQTTGroup> _webDataDump;

    public GamesManager()
    {
        _gameSignal = new SignalValue<ArcadeGame, MQTTGroup>(MQTTGroup.Exchange, ArcadeEndpointTopics.REGISTER_GAME);
        _gameSignal.ValueChanged += GameDataUpdate;
        
        _webDataDump = new SignalValue<ArcadeCentralData, MQTTGroup>(MQTTGroup.Exchange, ArcadeEndpointTopics.WEB_ALLDATA);
        
        _gameCheckTimer = Observable.Interval(TimeSpan.FromSeconds(2)).Subscribe(_ =>
        {
            var offlineGames = false;
            foreach (var gamesOffline in Data.Games.Where(x => x.State != GameState.Offline && (DateTime.Now - x.LastReported) > TimeSpan.FromSeconds(3)))
            {
                gamesOffline.State = GameState.Offline;
                foreach (var player in Data.Players.Where(x => x.CurrentGame == gamesOffline.Code))
                    player.CurrentGame = 0;
                offlineGames = true;
            }

            if (offlineGames)
            {
                _webDataDump.Push();
                Push();
            }
        });
    }

    private void GameDataUpdate(ArcadeGame value)
    {
        if (value.Code == 0)
            return;

        var updateClient = false;
        
        lock (_gameUpdateLock)
        {
            var game = Data.Games.FirstOrDefault(x => x.Code == value.Code);
            if (game is null)
                Data.Games.Add(value);
            else
            {
                if (game.State is GameState.Idle or GameState.Offline)
                {
                    foreach (var player in Data.Players.Where(x => x.CurrentGame == game.Code))
                    {
                        player.CurrentGame = 0;
                        updateClient = true;
                    }
                }
                
                game.LastReported = value.LastReported;
                game.State = value.State;
            }
        }
        
        Push();
        if (updateClient)
            _webDataDump.Push();
    }

    protected override void OnDispose()
    {
        _gameSignal.Dispose();
        _gameCheckTimer.Dispose();
        _webDataDump.Dispose();
    }
}
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Meatcorps.Engine.Arcade.Constants;
using Meatcorps.Engine.Arcade.Data;
using Meatcorps.Engine.MQTT.Enums;
using Meatcorps.Engine.Signals.Data;

namespace Meatcorps.Engine.Arcade.Services;

public class ArcadeDataService: IDisposable
{
    private readonly ArcadeCentralData _data = new ArcadeCentralData();
    private readonly object _gameSyncLock = new();
    private readonly object _playerSyncLock = new();
    private SignalValue<ArcadeSystemMessage, MQTTGroup> _message;
    private List<IDisposable> _disposables = new();
    private readonly Subject<ArcadePlayer> _playerUpdateSubject = new();
    public IObservable<ArcadePlayer> PlayerUpdate => _playerUpdateSubject.AsObservable();
    private readonly Subject<ArcadeGame> _gameUpdateSubject = new();
    public IObservable<ArcadeGame> GameUpdate => _gameUpdateSubject.AsObservable();

    public bool DataReady { get; private set; }

    public IObservable<Unit> DataChanged => Observable.Merge(
        PlayerUpdate.Select(x => Unit.Default), 
        GameUpdate.Select(x => Unit.Default)
        ).Throttle(TimeSpan.FromMilliseconds(33)) // ~30fps UI
        .Publish()
        .RefCount();
        
    public ArcadeDataService()
    {
        var allData =
            new SignalValue<ArcadeCentralData, MQTTGroup>(MQTTGroup.Exchange, ArcadeEndpointTopics.WEB_ALLDATA);
        allData.ValueChanged += AllDataOnValueChanged;
        _disposables.Add(allData);
        
        var gameData =
            new SignalValue<ArcadeGame, MQTTGroup>(MQTTGroup.Exchange, ArcadeEndpointTopics.REGISTER_GAME);
        gameData.ValueChanged += SyncGame;
        _disposables.Add(gameData);
        
        var playerData = 
            new SignalValue<ArcadePlayer, MQTTGroup>(MQTTGroup.Exchange, ArcadeEndpointTopics.GAMESESSION_SIGNIN_AND_UPDATE);
        playerData.ValueChanged += SyncPlayer;
        _disposables.Add(playerData);
        
        var joinData = 
            new SignalValue<ArcadePlayer, MQTTGroup>(MQTTGroup.Exchange, ArcadeEndpointTopics.JOIN_GAME);
        joinData.ValueChanged += SyncPlayer;
        _disposables.Add(joinData);

        _message = new SignalValue<ArcadeSystemMessage, MQTTGroup>(MQTTGroup.Exchange, ArcadeEndpointTopics.SYSTEM_MESSAGE);
        _disposables.Add(_message);

        Task.Run(async () =>
        {
            while (!DataReady && _disposables.Count > 0)
            {
                SendMessage(ArcadeSystemMessageCommands.GET_ALL_DATA);
                await Task.Delay(1000);
            }
        });
    }

    public void SendMessage(string message)
    {
        _message.Value = new ArcadeSystemMessage
        {
            Message = message
        };
    }

    public IEnumerable<ArcadeGame> Games()
    {
        lock (_gameSyncLock) return _data.Games.ToArray();
    }
    
    public IEnumerable<ArcadePlayer> Players()
    {
        lock (_playerSyncLock) return _data.Players.ToArray();
    }

    public bool TryGetGame(int code, out ArcadeGame? game)
    {
        lock (_gameSyncLock)
        {
            game = _data.Games.FirstOrDefault(x => x.Code == code);
            return game is not null;
        }
    }

    public bool TryGetPlayer(string id, out ArcadePlayer? player)
    {
        lock (_playerSyncLock)
        {
            player = _data.Players.FirstOrDefault(x => x.Id == id);
            return player is not null;
        }
    }

    private void AllDataOnValueChanged(ArcadeCentralData value)
    {
        foreach (var external in value.Games)
            SyncGame(external);
        
        foreach (var external in value.Players)
            SyncPlayer(external);
        
        DataReady = true;
    }

    private void SyncGame(ArcadeGame external)
    {
        lock (_gameSyncLock)
        {
            var game = _data.Games.FirstOrDefault(x => x.Code == external.Code);
            if (game is null)
            {
                _data.Games.Add(external);
                _gameUpdateSubject.OnNext(external);
            }
            else
            {
                game.LastReported = external.LastReported;
                
                if (game.State == external.State)
                    return;
                game.State = external.State;
                _gameUpdateSubject.OnNext(game);
            }
        }
    }
    
    private void SyncPlayer(ArcadePlayer external)
    {
        lock (_playerSyncLock)
        {
            var player = _data.Players.FirstOrDefault(x => x.Id == external.Id);
            if (player is null)
            {
                _data.Players.Add(external);
                _playerUpdateSubject.OnNext(external);
            }
            else
            {
                
                if (player.CurrentGame == external.CurrentGame && player.Points == external.Points)
                    return;
                player.CurrentGame = external.CurrentGame;
                player.Points = external.Points;
                _playerUpdateSubject.OnNext(player);
            }
        }
    }

    public void Dispose()
    {
        foreach (var disposable in _disposables.ToArray())
            disposable.Dispose();

        if (!_playerUpdateSubject.IsDisposed)
        {
            _playerUpdateSubject.OnCompleted(); 
            _playerUpdateSubject.Dispose();
        }

        if (!_gameUpdateSubject.IsDisposed)
        {
            _gameUpdateSubject.OnCompleted();   
            _gameUpdateSubject.Dispose();
        }

        _disposables.Clear();
    }
}
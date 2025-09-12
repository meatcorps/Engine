using System.Collections.Concurrent;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Meatcorps.Engine.MQTT.Enums;
using Meatcorps.Engine.Signals.Abstractions;

namespace Meatcorps.Engine.MQTT.Services;

public class MQTTSignalValueEvent: BaseSignalValueEvent<MQTTGroup>
{
    private readonly MQTTClient _client;
    private Dictionary<string, (Func<string, object?>, Func<object, string>)> _converterActions = new ();
    private CancellationDisposable _connectionAliveToken = new CancellationDisposable();
    private ConcurrentQueue<Action> _publishTasks = new ();
    private Dictionary<string, bool> _onlyRead = new();
    private Dictionary<string, bool> _onlyWrite = new();
    
    public MQTTSignalValueEvent(MQTTClient client)
    {
        _client = client;
        _client.Connected.Subscribe(SubscribeToTopics, AliveToken);
        _ = RunPublishLoopAsync();
    }

    private async Task RunPublishLoopAsync()
    {
        while (!AliveToken.IsCancellationRequested)
        {
            if (_client.IsConnected)
                while (_publishTasks.Count > 0)
                {
                    try
                    {
                        if (_publishTasks.TryDequeue(out var task))
                            task();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }

            try
            {
                await Task.Delay(10, AliveToken);
            } catch (Exception)
            {
                // ignored
            }
        }
    }

    private void SubscribeToTopics(Unit _)
    {
        if (!_connectionAliveToken.IsDisposed)
            _connectionAliveToken.Dispose();
        
        _connectionAliveToken = new CancellationDisposable();
        
        Task.Run(async () =>
        {
            foreach (var toRegister in _converterActions)
            {
                if (_onlyWrite.ContainsKey(toRegister.Key) && !_onlyWrite[toRegister.Key])
                    continue;
                
                var receiving = await _client.SubscribeToTopic(toRegister.Key);
                receiving
                    .Subscribe(x =>
                        {
                            try
                            {
                                var data = toRegister.Value.Item1(x);
                                SetValue(toRegister.Key, data);
                            } catch (Exception e)
                            {
                                Console.WriteLine(e);
                                return;
                            }
                        }, _connectionAliveToken.Token);
            }
        });
        
    }

    public MQTTSignalValueEvent Register<TValueType>(string topic, Func<object, string> getConverter,
        Func<string, object?> setAction, bool onlyRead = false, bool onlyWrite = false)
    {
        _onlyRead.Add(topic, onlyRead);
        _onlyWrite.Add(topic, onlyRead);
        
        _converterActions.Add(topic, (setAction, getConverter));

        if (!onlyRead)
            GetSubject<TValueType>(topic)
                .Where(payload => payload is not null)
                .Subscribe(payload =>
            {
                _publishTasks.Enqueue(() =>
                {
                    try
                    {
                        _client.Publish(topic, getConverter(payload!)).Wait();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
            }, AliveToken);
        return this;
    }

    public MQTTSignalValueEvent ForcePublish<TValueType>(string topic)
    {
        if (!_onlyRead.ContainsKey(topic) || _onlyRead[topic])
            return this;
        _publishTasks.Enqueue(() =>
        {
            try
            {
                if (TryGetValue<TValueType>(topic, out var payload))
                    _client.Publish(topic, _converterActions[topic].Item2(payload!)).Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        });
        return this;
    }

    public MQTTSignalValueEvent RegisterComplexObject<TValueType>(string topic, bool onlyRead, bool onlyWrite)
    {
        Register<TValueType>(topic, x => JsonSerializer.Serialize(x), x =>
        {
            var data = JsonSerializer.Deserialize<TValueType>(x);
            return data;
        }, onlyRead, onlyWrite);

        return this;
    }
    
    public override MQTTGroup GetGroup()
    {
        return MQTTGroup.Exchange;
    }

    protected override void OnDispose(bool disposing)
    {
        _publishTasks.Clear();
        _connectionAliveToken.Dispose();
    }
    
    
}
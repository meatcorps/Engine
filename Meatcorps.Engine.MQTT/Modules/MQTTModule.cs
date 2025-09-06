using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.Interfaces.Trackers;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.MQTT.Enums;
using Meatcorps.Engine.MQTT.Services;
using Meatcorps.Engine.Signals.Data;
using Meatcorps.Engine.Signals.Interfaces;

namespace Meatcorps.Engine.MQTT.Modules;

public class MQTTModule
{
    private readonly MQTTClient _client;
    private MQTTSignalValueEvent _tracker;
    private readonly IUniversalConfig _settings;
    private const string GROUP = "MQTT";

    public static MQTTModule Load()
    {
        var settings = GlobalObjectManager.ObjectManager.Get<IUniversalConfig>();
        var client = new MQTTClient(settings.GetOrDefault(GROUP, "host", "localhost"));
        GlobalObjectManager.ObjectManager.Register(client);
        return new MQTTModule(client);
    }

    private MQTTModule(MQTTClient client)
    {
        _settings = GlobalObjectManager.ObjectManager.Get<IUniversalConfig>()!;
        _client = client;
        _tracker = new MQTTSignalValueEvent(client);
        GlobalObjectManager.ObjectManager.Register(client);
        GlobalObjectManager.ObjectManager.RegisterSet<ISignalValueEvent<MQTTGroup>>();
        GlobalObjectManager.ObjectManager.Add<ISignalValueEvent<MQTTGroup>>(_tracker);
    }

    public MQTTModule Register<TValueType>(string topic, Func<object, string> getConverter,
        Func<string, object?> setAction, TValueType defaultValue = default)
    {
        var valueTracker = new SignalValue<TValueType, MQTTGroup>(MQTTGroup.Exchange, topic, defaultValue, GlobalObjectManager.ObjectManager);
        GlobalObjectManager.ObjectManager.Add<IDisposable>(valueTracker);
        _tracker.Register<TValueType>(topic, getConverter, setAction);
        if (defaultValue is not null)
            _tracker.ForcePublish<TValueType>(topic);
        
        return this;
    }

    public MQTTModule RegisterComplexObject<TValueType>(string topic, bool onlyRead, TValueType defaultValue = default)
    {
        var valueTracker = new SignalValue<TValueType, MQTTGroup>(MQTTGroup.Exchange, topic, defaultValue, GlobalObjectManager.ObjectManager);
        GlobalObjectManager.ObjectManager.Add<IDisposable>(valueTracker);
        _tracker.RegisterComplexObject<TValueType>(topic, onlyRead);
        if (defaultValue is not null)
            _tracker.ForcePublish<TValueType>(topic); 
        return this;
    }

    public void Create()
    {
        _ = _client.Connect(
            _settings.GetOrDefault(GROUP, "user", "user"), _settings.GetOrDefault(GROUP, "password", "admin"));
    }
}
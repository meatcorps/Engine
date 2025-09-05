using Meatcorps.Engine.MQTT.Enums;
using Meatcorps.Engine.Signals.Abstractions;

namespace Meatcorps.Engine.MQTT.Services;

public class MQTTSignalValueEvent: BaseSignalValueEvent<MQTTGroup>
{
    private readonly MQTTClient _client;
    private Dictionary<string, (Action<string>, Func<string>)> _converterActions = new Dictionary<string, (Action<string>, Func<string>)>();
    
    public MQTTSignalValueEvent(MQTTClient client)
    {
        _client = client;
    }
    
    //public MQTTSignalValueEvent Register<TValueType>(string topic, Func<string> getConverter, Action<string> setAction)
    
    public override MQTTGroup GetGroup()
    {
        return MQTTGroup.Exchange;
    }
    
    
}
namespace Meatcorps.Engine.MQTT.Interfaces;

public abstract class MessageIdentifierBase
{
    public string MessageIdentifier { get; set; } = Guid.NewGuid().ToString();
}
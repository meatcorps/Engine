using Meatcorps.Engine.MQTT.Interfaces;

namespace Meatcorps.Engine.Arcade.Data;

[Serializable]
public class ArcadePointChange: MessageIdentifierBase
{
    public string Id { get; set; } = "";
    public int Value { get; set; }
}
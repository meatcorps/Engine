using Meatcorps.Engine.MQTT.Interfaces;

namespace Meatcorps.Engine.Arcade.Data;

[Serializable]
public class ArcadeSystemMessage: MessageIdentifierBase
{
    public string Message { get; set; } = "";
}
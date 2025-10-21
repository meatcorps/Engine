using Meatcorps.Engine.MQTT.Interfaces;

namespace Meatcorps.Engine.Arcade.Data;


[Serializable]
public class ArcadePlayer: MessageIdentifierBase
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public int Points { get; set; }
    public int CurrentGame { get; set; }
}
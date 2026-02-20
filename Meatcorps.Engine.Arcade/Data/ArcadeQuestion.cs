using Meatcorps.Engine.MQTT.Interfaces;

namespace Meatcorps.Engine.Arcade.Data;

[Serializable]
public class ArcadeQuestion: MessageIdentifierBase
{
    public string Id { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public List<string> Answers { get; set; } = new();
    public int Timeout { get; set; }
    public List<string> IgnorePlayers { get; set; } = new();
}
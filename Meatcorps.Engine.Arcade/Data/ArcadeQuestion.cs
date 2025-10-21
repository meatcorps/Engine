using Meatcorps.Engine.MQTT.Interfaces;

namespace Meatcorps.Engine.Arcade.Data;

[Serializable]
public class ArcadeQuestion: MessageIdentifierBase
{
    public string Id { get; set; }
    public string From { get; set; }
    public string Question { get; set; }
    public List<string> Answers { get; set; }
    public int Timeout { get; set; }
    public List<string> IgnorePlayers { get; set; }
}
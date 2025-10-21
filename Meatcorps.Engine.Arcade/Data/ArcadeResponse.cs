using Meatcorps.Engine.MQTT.Interfaces;

namespace Meatcorps.Engine.Arcade.Data;

[Serializable]
public class ArcadeResponse: MessageIdentifierBase
{
    public string Id { get; set; }
    public string QuestionId { get; set; }
    public string From { get; set; }
    public string Message { get; set; }
}
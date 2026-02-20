using Meatcorps.Engine.MQTT.Interfaces;

namespace Meatcorps.Engine.Arcade.Data;

[Serializable]
public class ArcadeResponse: MessageIdentifierBase
{
    public string Id { get; set; } = string.Empty;
    public string QuestionId { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
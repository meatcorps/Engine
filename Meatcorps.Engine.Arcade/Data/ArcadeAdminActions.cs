using Meatcorps.Engine.MQTT.Interfaces;

namespace Meatcorps.Engine.Arcade.Data;

public class ArcadeAdminActions: MessageIdentifierBase
{
    public string Action { get; init; } = string.Empty;
    public string Param1 { get; init; } = string.Empty;
    public string Param2 { get; init; } = string.Empty;
}
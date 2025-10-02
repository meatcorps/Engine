using Meatcorps.Engine.MQTT.Interfaces;

namespace Meatcorps.Engine.Arcade.Data;

public class ArcadeAdminActions: MessageIdentifierBase
{
    public string Action { get; set; }
    public string Param1 { get; set; }
    public string Param2 { get; set; }
}
using Meatcorps.Engine.MQTT.Interfaces;

namespace Meatcorps.Engine.Arcade.Data;

[Serializable]
public class ArcadeCentralData: MessageIdentifierBase
{
    public List<ArcadeGame> Games { get; set; } = new List<ArcadeGame>();
    public List<ArcadePlayer> Players { get; set; } = new List<ArcadePlayer>();
}
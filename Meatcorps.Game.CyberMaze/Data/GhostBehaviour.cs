using Meatcorps.Engine.Collision.Providers.Bodies;
using Meatcorps.Game.CyberMaze.GameObjects.GhostManagers;

namespace Meatcorps.Game.CyberMaze.Data;

public class GhostBehaviour
{
    public Body Body { get; init; } = null!;
    public AI.GhostLogic Logic { get; init; }
    public GhostType Type { get; init; }

    public bool Enabled { get; set; }
}
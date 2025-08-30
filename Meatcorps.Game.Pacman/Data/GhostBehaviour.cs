using Meatcorps.Engine.Collision.Providers.Bodies;
using Meatcorps.Game.Pacman.GameObjects;
using Meatcorps.Game.Pacman.GameObjects.GhostManagers;

namespace Meatcorps.Game.Pacman.Data;

public class GhostBehaviour
{
    public Body Body { get; init; } = null!;
    public AI.GhostLogic Logic { get; init; }
    public GhostType Type { get; init; }

    public bool Enabled { get; set; }
}
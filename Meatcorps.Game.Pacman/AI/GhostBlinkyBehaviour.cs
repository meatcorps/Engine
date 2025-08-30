using Meatcorps.Game.Pacman.Data;

namespace Meatcorps.Game.Pacman.AI;

public class GhostBlinkyBehaviour: GhostLogic
{
    public GhostBlinkyBehaviour(LevelData levelData) : base(levelData)
    {
    }

    public override int TimeoutBeforeStart => 0;
}
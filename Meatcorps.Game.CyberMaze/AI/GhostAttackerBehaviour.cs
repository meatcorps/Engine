using Meatcorps.Game.CyberMaze.Data;

namespace Meatcorps.Game.CyberMaze.AI;

public class GhostAttackerBehaviour: GhostLogic
{
    public GhostAttackerBehaviour(LevelData levelData) : base(levelData)
    {
    }

    public override int TimeoutBeforeStart => 1000;
}
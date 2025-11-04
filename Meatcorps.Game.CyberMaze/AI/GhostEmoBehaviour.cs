using Meatcorps.Game.CyberMaze.Data;

namespace Meatcorps.Game.CyberMaze.AI;

public class GhostEmoBehaviour: GhostLogic
{
    public override int TimeoutBeforeStart => 4000;
    public GhostEmoBehaviour(LevelData levelData) : base(levelData)
    {
    }

    protected override int OnGetStartDistance()
    {
        return -8;
    }
}
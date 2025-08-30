using Meatcorps.Game.Pacman.Data;

namespace Meatcorps.Game.Pacman.AI;

public class GhostClydeBehaviour: GhostLogic
{
    public override int TimeoutBeforeStart => 3000;
    public GhostClydeBehaviour(LevelData levelData) : base(levelData)
    {
    }

    protected override int OnGetStartDistance()
    {
        return -8;
    }
}
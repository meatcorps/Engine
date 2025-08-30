using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Game.Pacman.Data;

namespace Meatcorps.Game.Pacman.AI;

public class GhostPinkyBehaviour: GhostLogic
{
    private Vector2 _previousVelocity = new Vector2(0, -1);
    public override int TimeoutBeforeStart => 1000;

    public GhostPinkyBehaviour(LevelData levelData) : base(levelData)
    {
    }

    protected override PointInt OnGetChaseTarget()
    {
        var pacManVelocityNormal = LevelData.TargetPacman!.Body.Velocity.NormalizedCopy();
        
        if (pacManVelocityNormal.IsEqualsSafe(Vector2.Zero) || float.IsNaN(pacManVelocityNormal.X) || float.IsNaN(pacManVelocityNormal.Y))
            pacManVelocityNormal = _previousVelocity;
        else 
            _previousVelocity = pacManVelocityNormal;

        
        if (LevelData.AuthenticBug)
            if (pacManVelocityNormal.IsEqualsSafe(new Vector2(0, -1)))
                pacManVelocityNormal = new Vector2(-1, -1);

        var pacMan = LevelData.WorldToCell(LevelData.TargetPacman!.Body.Position);
        var offset = pacManVelocityNormal.ToPointInt() * 4;
        
        return pacMan + offset;
    }
}
using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Game.CyberMaze.Data;

namespace Meatcorps.Game.CyberMaze.AI;

public class GhostAnotherBehaviour: GhostLogic
{
    private Vector2 _previousVelocity = new Vector2(0, -1);
    public override int TimeoutBeforeStart => 2000;

    public GhostAnotherBehaviour(LevelData levelData) : base(levelData)
    {
    }

    protected override PointInt OnGetChaseTarget()
    {
        var playerVelocityNormal = LevelData.TargetCyberPlayer!.Body.Velocity.NormalizedCopy();
        
        if (playerVelocityNormal.IsEqualsSafe(Vector2.Zero) || float.IsNaN(playerVelocityNormal.X) || float.IsNaN(playerVelocityNormal.Y))
            playerVelocityNormal = _previousVelocity;
        else 
            _previousVelocity = playerVelocityNormal;

        
        if (LevelData.AuthenticBug)
            if (playerVelocityNormal.IsEqualsSafe(new Vector2(0, -1)))
                playerVelocityNormal = new Vector2(-1, -1);

        var pacMan = LevelData.WorldToCell(LevelData.TargetCyberPlayer!.Body.Position);
        var offset = playerVelocityNormal.ToPointInt() * 4;
        
        return pacMan + offset;
    }
}
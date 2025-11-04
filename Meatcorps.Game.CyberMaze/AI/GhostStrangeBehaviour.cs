using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Game.CyberMaze.Data;
using Meatcorps.Game.CyberMaze.GameObjects.GhostManagers;

namespace Meatcorps.Game.CyberMaze.AI;

public class GhostStrangeBehaviour : GhostLogic
{
    private GhostBehaviour? _attacker;
    private Vector2 _previousVelocity = new Vector2(0, -1); 
    public override int TimeoutBeforeStart => 3000;
    
    public GhostStrangeBehaviour(LevelData levelData) : base(levelData)
    {
    }

    protected override PointInt OnGetChaseTarget()
    {
        if (_attacker == null)
            _attacker = LevelData.Ghosts.FirstOrDefault(x => x.Type == GhostType.Attacker);
        
        if (_attacker == null)
            return base.OnGetChaseTarget();

        var pacManVelocityNormal = LevelData.TargetCyberPlayer!.Body.Velocity.NormalizedCopy();
        
        if (pacManVelocityNormal.IsEqualsSafe(Vector2.Zero) || float.IsNaN(pacManVelocityNormal.X) || float.IsNaN(pacManVelocityNormal.Y))
            pacManVelocityNormal = _previousVelocity;
        else 
            _previousVelocity = pacManVelocityNormal;
        
        if (LevelData.AuthenticBug)
            if (pacManVelocityNormal.IsEqualsSafe(new Vector2(0, -1)))
                pacManVelocityNormal = new Vector2(-1, -1);
        
        var vectorTrick = LevelData.TargetCyberPlayer!.Body.Position -
                          pacManVelocityNormal.NormalizedCopy() * LevelData.GridSize * 2;
        vectorTrick -= _attacker.Body.Position;
        vectorTrick *= 2;
        vectorTrick += _attacker.Body.Position;
        
        return LevelData.WorldToCell(vectorTrick);
    }
}
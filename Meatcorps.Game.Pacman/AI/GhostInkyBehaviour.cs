using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Game.Pacman.Data;
using Meatcorps.Game.Pacman.GameObjects;
using Meatcorps.Game.Pacman.GameObjects.GhostManagers;

namespace Meatcorps.Game.Pacman.AI;

public class GhostInkyBehaviour : GhostLogic
{
    private GhostBehaviour? _blinky;
    private Vector2 _previousVelocity = new Vector2(0, -1); 
    public override int TimeoutBeforeStart => 2000;
    
    public GhostInkyBehaviour(LevelData levelData) : base(levelData)
    {
    }

    protected override PointInt OnGetChaseTarget()
    {
        if (_blinky == null)
            _blinky = LevelData.Ghosts.FirstOrDefault(x => x.Type == GhostType.Blinky);
        
        if (_blinky == null)
            return base.OnGetChaseTarget();

        var pacManVelocityNormal = LevelData.TargetPacman!.Body.Velocity.NormalizedCopy();
        
        if (pacManVelocityNormal.IsEqualsSafe(Vector2.Zero) || float.IsNaN(pacManVelocityNormal.X) || float.IsNaN(pacManVelocityNormal.Y))
            pacManVelocityNormal = _previousVelocity;
        else 
            _previousVelocity = pacManVelocityNormal;
        
        if (LevelData.AuthenticBug)
            if (pacManVelocityNormal.IsEqualsSafe(new Vector2(0, -1)))
                pacManVelocityNormal = new Vector2(-1, -1);
        
        var vectorTrick = LevelData.TargetPacman!.Body.Position -
                          pacManVelocityNormal.NormalizedCopy() * LevelData.GridSize * 2;
        vectorTrick -= _blinky.Body.Position;
        vectorTrick *= 2;
        vectorTrick += _blinky.Body.Position;
        
        return LevelData.WorldToCell(vectorTrick);
    }
}
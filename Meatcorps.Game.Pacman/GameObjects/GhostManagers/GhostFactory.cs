using System.Numerics;
using Meatcorps.Engine.Collision.Colliders;
using Meatcorps.Engine.Collision.Enums;
using Meatcorps.Engine.Collision.Extensions;
using Meatcorps.Engine.Collision.Services;
using Meatcorps.Engine.Collision.Utilities;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Game.Pacman.AI;
using Meatcorps.Game.Pacman.Data;
using Meatcorps.Game.Pacman.GameEnums;

namespace Meatcorps.Game.Pacman.GameObjects.GhostManagers;

public class GhostFactory
{
    public static Ghost Create(Vector2 startPosition, PointInt corner, GhostType type, LevelData level, WorldService world)
    {
        var ghost = new Ghost();
        
        AI.GhostLogic ghostLogic;
        switch (type)
        {
            case GhostType.Blinky:
                ghostLogic = new GhostBlinkyBehaviour(level);
                break;
            case GhostType.Pinky:
                ghostLogic = new GhostPinkyBehaviour(level);
                break;
            case GhostType.Inky:
                ghostLogic = new GhostInkyBehaviour(level);
                break;
            case GhostType.Clyde:
                ghostLogic = new GhostClydeBehaviour(level);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
        
        ghostLogic.Corner = corner;
        
        var behaviour = new GhostBehaviour
        {
            Body = world.RegisterRectFBody(ghost, new RectF(startPosition, new SizeF(level.GridSize, level.GridSize)))
                .SetType(BodyType.Dynamic)
                .SetLayer(LayerBits.Bit(CollisionLayer.Ghost))
                .SetMask(LayerBits.MaskOf(CollisionLayer.Wall, CollisionLayer.OneWay)),
            Logic = ghostLogic,
            Type = type,
        };

        behaviour.Body.AddCollider(
            new RectCollider(behaviour.Body, new RectF(4, 4, level.GridSize - 8, level.GridSize - 8))
                .SetLayer(LayerBits.Bit(CollisionLayer.Ghost))
                .SetMask(LayerBits.MaskOf(CollisionLayer.PacMan))
                .SetSensor(true)
        );
        
        ghost.SetBehaviour(behaviour);
        level.Ghosts.Add(behaviour);
        return ghost;
    }
}
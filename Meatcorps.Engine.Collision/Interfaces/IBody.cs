using System.Numerics;
using Meatcorps.Engine.Collision.Enums;
using Meatcorps.Engine.Collision.Services;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Interfaces.Grid;

namespace Meatcorps.Engine.Collision.Interfaces;

public interface IBody: IGridItem, IDisposable
{
    BodyType BodyType { get; }
    Vector2 Position { get; set; }
    Vector2 PreviousPosition { get; set; } // world updates each step
    Vector2 Velocity { get; set; }

    IWorldService WorldService { get; }    // or omit if not needed

    IEnumerable<ICollider> Colliders { get; }

    float Mass { get; }                    // Dynamic: >0; Static/Kinematic: treated as immovable
    float Restitution { get; }             // [0..1]
    float Friction { get; }                // [0..1]
    float LinearDamping { get; }           // >= 0
    float MaxSpeed { get; }                // or Vector2 MaxVelocity if you prefer per-axis
    float GravityScale { get; }            // scalar; world provides gravity vector

    bool IsAwake { get; set; }
    bool CanSleep { get; }
    int StableIndex { get; }               // set by world at registration
    bool Enabled { get; set; }             // optional
    float MovementConstraintAngle { get; } // 0 = free, else snap to nearest multiple
}
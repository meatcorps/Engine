using System.Numerics;
using Meatcorps.Engine.Core.Data;

namespace Meatcorps.Engine.Collision.Interfaces;

public interface ICollider
{
    IBody Body { get; }
    bool IsSensor { get; }
    uint Layer { get; set; }           // bit
    uint CollisionMask { get; set; }   // bit

    // RectF-only phase:
    RectF LocalRect { get; }      // in body space
    RectF WorldRect { get; }      // LocalRect translated by Body.Position

    // Optional toggles / metadata:
    bool Enabled { get; set; }         // optional
    uint Tag { get; set; }              // optional

    // Optional convenience (deterministic path should use providers):
    bool CollidesWith(ICollider other);
    void Solve(ICollider other);
}
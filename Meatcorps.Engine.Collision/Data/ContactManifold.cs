using System.Numerics;
using Meatcorps.Engine.Core.Data;

namespace Meatcorps.Engine.Collision.Data;

public struct ContactManifold
{
    public Vector2 Normal { get; init; }       // points from A to B (resolution axis)
    public float Penetration { get; init; }    // absolute overlap on the chosen axis
    public RectF OverlapArea { get; init; } 
}
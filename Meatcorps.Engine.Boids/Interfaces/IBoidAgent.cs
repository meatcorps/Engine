using System.Numerics;

namespace Meatcorps.Engine.Boids.Interfaces;

public interface IBoidAgent
{
    Vector2 Position { get; set; }
    Vector2 Velocity { get; set; }

    float Radius { get; }   // for separation; default 0.5–1 cell
    float Mass { get; }     // default 1
    bool IsActive { get; }  // skip if false
}
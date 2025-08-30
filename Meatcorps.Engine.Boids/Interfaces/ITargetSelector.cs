using System.Numerics;

namespace Meatcorps.Engine.Boids.Interfaces;

public interface ITargetSelector
{
    Vector2? GetTarget(float nowSeconds);
}
namespace Meatcorps.Engine.Collision.Interfaces;

public interface IResolutionPolicy
{
    // Fractions of MTV to apply to A and B (0..1)
    (float pushA, float pushB) Decide(IBody a, IBody b);
}
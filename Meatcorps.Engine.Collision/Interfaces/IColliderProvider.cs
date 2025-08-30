using Meatcorps.Engine.Collision.Data;

namespace Meatcorps.Engine.Collision.Interfaces;

public interface IColliderProvider
{
    // public Type ColliderType1 { get; }
    // public Type ColliderType2 { get; }

    bool CollideWith(ICollider a, ICollider b, out ContactManifold manifold);
    bool Solve(ICollider a, ICollider b, in ContactManifold m, IResolutionPolicy policy);
}
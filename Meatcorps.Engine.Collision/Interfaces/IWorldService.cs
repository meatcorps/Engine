using System.Numerics;
using Meatcorps.Engine.Collision.Data;
using Meatcorps.Engine.Collision.Services;

namespace Meatcorps.Engine.Collision.Interfaces;

public interface IWorldService
{
    IColliderProvider ColliderProviders { get; }
    IResolutionPolicy Policy { get; }

    IEnumerable<(ICollider A, ICollider B, ContactManifold Manifold)>
        QueryContacts(IBody body, Vector2 position, uint collisionMask = uint.MaxValue);

    WorldService UnregisterBody(IBody body);
}
using Meatcorps.Engine.Collision.Abstractions;
using Meatcorps.Engine.Collision.Colliders;
using Meatcorps.Engine.Collision.Interfaces;

namespace Meatcorps.Engine.Collision.Providers.Colliders;

public sealed class RectRectProvider : RectRectProviderBase<RectCollider, RectCollider>
{
    public RectRectProvider(IResolutionPolicy policy) : base(policy) { }

    // Optional: specialize CollideWith/Solve here if some pair needs different behavior.
    // Otherwise, the base default is enough.
}
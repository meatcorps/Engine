using System.Numerics;
using Meatcorps.Engine.Collision.Abstractions;
using Meatcorps.Engine.Collision.Data;
using Meatcorps.Engine.Collision.Interfaces;

namespace Meatcorps.Engine.Collision.Providers.Colliders;

public sealed class OneWayGateProvider : RectRectProviderBase<ICollider, ICollider>
{
    private readonly Vector2 _allowedNormal;   // direction you may pass through
    private readonly float   _cosThreshold;    // e.g., cos(60°) = 0.5f

    public OneWayGateProvider(IResolutionPolicy policy, Vector2 allowedNormal, float degreesTolerance)
        : base(policy)
    {
        _allowedNormal = Vector2.Normalize(allowedNormal);
        _cosThreshold  = MathF.Cos(degreesTolerance * MathF.PI / 180f);
    }

    protected override bool OnAdditionalResolve(ICollider a, ICollider b, in ContactManifold m)
    {
        // Example rule: only allow if A's velocity points “with” the gate normal (tolerance)
        var v = a.Body.Velocity;
        if (v.LengthSquared() <= 1e-9f) return false;

        var dir = Vector2.Normalize(v);
        var cos = Vector2.Dot(dir, _allowedNormal);
        return cos >= _cosThreshold;
    }
}
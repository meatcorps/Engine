using System.Numerics;
using System.Runtime.CompilerServices;
using Meatcorps.Engine.Collision.Data;
using Meatcorps.Engine.Collision.Enums;
using Meatcorps.Engine.Collision.Interfaces;
using Meatcorps.Engine.Core.Data;

namespace Meatcorps.Engine.Collision.Abstractions;

public abstract class RectRectProviderBase<TA, TB> : BaseCollideProvider<TA, TB>
    where TA : class, ICollider
    where TB : class, ICollider
{
    protected readonly IResolutionPolicy Policy;

    protected RectRectProviderBase(IResolutionPolicy policy) => Policy = policy;

    public sealed override bool CollideWith(TA a, TB b, out ContactManifold m)
    {
        var ra = a.WorldRect;
        var rb = b.WorldRect;
        if (!RectF.Intersects(ref ra, ref rb))
        {
            m = default;
            return false;
        }

        var overlap = RectF.Intersection(ra, rb);
        var (normal, depth) = ComputeAabbMtv(in ra, in rb);

        m = new ContactManifold
        {
            Normal = normal,
            Penetration = depth,
            OverlapArea = overlap
        };

        return depth > 0f;
    }

    public sealed override bool Solve(TA a, TB b, in ContactManifold m, IResolutionPolicy policy)
    {
        // 0) Veto?
        if (!OnAdditionalResolve(a, b, m))
            return false;

        // 1) Positional correction (MTV split by policy)
        var (pushA, pushB) = policy.Decide(a.Body, b.Body);

        if (pushA > 0f)
            a.Body.Position += -m.Normal * (m.Penetration * pushA);

        if (pushB > 0f)
            b.Body.Position += m.Normal * (m.Penetration * pushB);

        // 2) Velocity correction (impulses)
        ApplyVelocityResponse(a, b, in m);

        OnResolved(a, b, m);
        return true;
    }

    // Default: accept. Override to implement one-way gates, speed thresholds, etc.
    protected virtual bool OnAdditionalResolve(TA a, TB b, in ContactManifold m) => true;

    // Optional side-effects hook
    protected virtual void OnResolved(TA a, TB b, in ContactManifold m)
    {
    }

    protected static (Vector2 normal, float depth) ComputeAabbMtv(in RectF ra, in RectF rb)
    {
        float ax2 = ra.X + ra.Width, ay2 = ra.Y + ra.Height;
        float bx2 = rb.X + rb.Width, by2 = rb.Y + rb.Height;

        float overlapX = MathF.Min(ax2, bx2) - MathF.Max(ra.X, rb.X);
        float overlapY = MathF.Min(ay2, by2) - MathF.Max(ra.Y, rb.Y);

        var ac = new Vector2(ra.X + ra.Width * 0.5f, ra.Y + ra.Height * 0.5f);
        var bc = new Vector2(rb.X + rb.Width * 0.5f, rb.Y + rb.Height * 0.5f);
        var diff = bc - ac;

        if (overlapX < overlapY)
            return (new Vector2(MathF.Sign(diff.X == 0 ? 1 : diff.X), 0f), MathF.Abs(overlapX));
        else
            return (new Vector2(0f, MathF.Sign(diff.Y == 0 ? 1 : diff.Y)), MathF.Abs(overlapY));
    }
}

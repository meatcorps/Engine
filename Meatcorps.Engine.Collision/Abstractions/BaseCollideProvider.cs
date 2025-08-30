using System.Numerics;
using System.Runtime.CompilerServices;
using Meatcorps.Engine.Collision.Data;
using Meatcorps.Engine.Collision.Enums;
using Meatcorps.Engine.Collision.Interfaces;

namespace Meatcorps.Engine.Collision.Abstractions;

public abstract class BaseCollideProvider<TA, TB> : IColliderProvider
    where TA : class, ICollider
    where TB : class, ICollider
{
    public Type ColliderType1 => typeof(TA);
    public Type ColliderType2 => typeof(TB);
    
    public bool CollideWith(ICollider a, ICollider b, out ContactManifold manifold)
    {
        if (a is TA a2 && b is TB b2) 
            return CollideWith(a2, b2, out manifold);

        manifold = default;
        return false;
    }

    public bool Solve(ICollider a, ICollider b, in ContactManifold m, IResolutionPolicy policy)
    {
        if (a is TA a2 && b is TB b2) 
            return Solve(a2, b2, m, policy);
        
        return false;
    }

    // Narrow-phase test for actual overlap (shape-aware). If true, also output a manifold.
    public abstract bool CollideWith(TA a, TB b, out ContactManifold manifold);

    // Apply resolution using the policy, and allow pair-specific veto/accept.
    // Return true if the pair is considered a “solid contact” this frame (i.e., emit contact events).
    // Return false to suppress contact events (e.g., one-way gate blocked).
    public abstract bool Solve(TA a, TB b, in ContactManifold m, IResolutionPolicy policy);
    
    protected static void ApplyVelocityResponse(ICollider ca, ICollider cb, in ContactManifold m)
    {
        var a = ca.Body;
        var b = cb.Body;

        // Treat Static/Kinematic as infinite mass (invMass = 0)
        var invMA = (a.BodyType is BodyType.Static or BodyType.Kinematic) ? 0f : SafeInvMass(a.Mass);
        var invMB = (b.BodyType is BodyType.Static or BodyType.Kinematic) ? 0f : SafeInvMass(b.Mass);

        // If both immovable, nothing to do
        if (invMA == 0f && invMB == 0f)
            return;

        var n = m.Normal; // A -> B

        // Relative velocity along the contact
        var vRel = b.Velocity - a.Velocity;
        var vn = Vector2.Dot(vRel, n);

        // If separating, skip normal impulse
        if (vn > 0f)
            return;

        // Restitution (bounce): pick max or min; here we use max for snappier arcade feel
        var e = MathF.Max(a.Restitution, b.Restitution);

        // Normal impulse scalar
        var invMassSum = invMA + invMB;
        if (invMassSum <= 0f)
            return;

        var j = -(1f + e) * vn / invMassSum;

        // Apply normal impulse
        var impulseN = j * n;

        if (invMA > 0f)
            a.Velocity -= impulseN * invMA;

        if (invMB > 0f)
            b.Velocity += impulseN * invMB;

        // --- Coulomb friction (optional but cheap) ---
        // Tangential direction
        var vt = vRel - vn * n;
        var vtLenSq = vt.LengthSquared();
        if (vtLenSq > 1e-12f)
        {
            var t = vt / MathF.Sqrt(vtLenSq);

            // Friction coefficient: simple average (tweak to taste)
            var mu = 0.5f * (a.Friction + b.Friction);

            // Tangential impulse scalar
            var vtDot = Vector2.Dot(vRel, t);
            var jt = -vtDot / invMassSum;

            // Clamp to Coulomb cone
            if (MathF.Abs(jt) > mu * j)
                jt = MathF.CopySign(mu * j, jt);

            var impulseT = jt * t;

            if (invMA > 0f)
                a.Velocity -= impulseT * invMA;

            if (invMB > 0f)
                b.Velocity += impulseT * invMB;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float SafeInvMass(float mass)
    {
        return mass <= 0f ? 0f : 1f / mass;
    }
}
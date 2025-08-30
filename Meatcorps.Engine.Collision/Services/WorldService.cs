using System.Numerics;
using System.Runtime.CompilerServices;
using Meatcorps.Engine.Collision.Data;
using Meatcorps.Engine.Collision.Enums;
using Meatcorps.Engine.Collision.Interfaces;
using Meatcorps.Engine.Collision.Providers;
using Meatcorps.Engine.Collision.Providers.Bodies;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Extensions;

namespace Meatcorps.Engine.Collision.Services;

public sealed class WorldService : IWorldService
{
    private readonly IWorldEntityResource _spatial;
    private readonly ColliderProviderRegistry _providers;

    private readonly List<ICollisionEvents> _sinks = new(4);
    private readonly List<IBody> _bodies = new(256);

    private readonly HashSet<ContactPair> _pairsPrev = new();
    private readonly HashSet<ContactPair> _pairsNow = new();

    private readonly Dictionary<ContactPair, ContactManifold> _manifoldThisFrame = new();

    public IResolutionPolicy Policy  { get; private set; } =  new DefaultResolutionPolicy();
    private Vector2 _gravity;
    
    public IColliderProvider ColliderProviders => _providers;

    public WorldService(IWorldEntityResource spatial, ColliderProviderRegistry providers)
    {
        _spatial = spatial ?? throw new ArgumentNullException(nameof(spatial));
        _providers = providers ?? throw new ArgumentNullException(nameof(providers));
    }

    // ---------- Fluent registration ----------

    public WorldService RegisterBody(IBody body)
    {
        if (body == null)
        {
            return this;
        }

        if (!_bodies.Contains(body))
        {
            SetStableIndex(body, _bodies.Count);

            _bodies.Add(body);

            _spatial.Add(body);
        }

        return this;
    }

    public WorldService UnregisterBody(IBody body)
    {
        if (body == null)
        {
            return this;
        }

        if (_bodies.Remove(body))
        {
            _spatial.Remove(body);
        }

        return this;
    }

    public WorldService AddCollisionEvents(ICollisionEvents sink)
    {
        if (sink != null && !_sinks.Contains(sink))
        {
            _sinks.Add(sink);
        }

        return this;
    }

    public WorldService RemoveCollisionEvents(ICollisionEvents sink)
    {
        if (sink != null)
        {
            _sinks.Remove(sink);
        }

        return this;
    }

    public WorldService SetGravity(Vector2 gravity)
    {
        _gravity = gravity;

        return this;
    }

    public WorldService SetResolutionPolicy(IResolutionPolicy policy)
    {
        Policy = policy ?? throw new ArgumentNullException(nameof(policy));

        return this;
    }

    // ---------- Step ----------

    public void Step(float deltaTime)
    {
        // 1) Integrate (Dynamic/Kinematic only). Broadphase stays outside (IWorldEntityResource).
        for (var i = 0; i < _bodies.Count; i++)
        {
            var body = _bodies[i];

            if (!body.Enabled)
                continue;

            if (body.BodyType == BodyType.Static)
                continue;

            var v = body.Velocity + _gravity * body.GravityScale * deltaTime;

            v = ApplyMovementConstraint(v, body.MovementConstraintAngle);

            if (body.MaxSpeed > 0f)
                v = v.LimitMagnitude(body.MaxSpeed);

            if (body.LinearDamping > 0f)
                v *= 1f / (1f + body.LinearDamping * deltaTime);
            
            body.PreviousPosition = body.Position;
            body.Position += v * deltaTime;
            body.Velocity = v;

            _spatial.Update(body);
        }

        _pairsNow.Clear();
        _manifoldThisFrame.Clear();

        // 2) Narrow-phase only (broadphase candidates come from IWorldEntityResource)
        for (var i = 0; i < _bodies.Count; i++)
        {
            var body = _bodies[i];

            if (!body.Enabled)
                continue;

            if (body.BodyType == BodyType.Static)
                continue;

            var candidates = _spatial.Query(body.BoundingBox);

            if (candidates == null || candidates.Count == 0)
                continue;

            foreach (var other in candidates)
            {
                if (ReferenceEquals(other, body))
                    continue;

                if (!other.Enabled)
                    continue;

                if (body.BodyType == BodyType.Static && other.BodyType == BodyType.Static)
                    continue;

                foreach (var a in body.Colliders)
                {
                    if (!a.Enabled)
                        continue;

                    foreach (var b in other.Colliders)
                    {
                        if (!b.Enabled)
                            continue;

                        if ((a.Layer & b.CollisionMask) == 0u)
                            continue;

                        if ((b.Layer & a.CollisionMask) == 0u)
                            continue;

                        // Quick AABB reject using your RectF API
                        var ra = a.WorldRect;

                        var rb = b.WorldRect;

                        if (!RectF.Intersects(ref ra, ref rb))
                            continue;

                        // Sensors → trigger path (no provider used)
                        if (a.IsSensor || b.IsSensor)
                        {
                            var tpair = MakePair(a, b);

                            _pairsNow.Add(tpair);

                            continue;
                        }
                        
                        // Provider does narrow-phase & manifold (may be stricter than AABB)
                        var hit = _providers.CollideWith(a, b, out var m);
                        
                        if (!hit)
                            continue;

                        // Provider resolves (can veto via its additional rules)
                        var accepted = _providers.Solve(a, b, in m, Policy);

                        if (!accepted)
                            continue;

                        var pair = MakePair(a, b);
                        _pairsNow.Add(pair);
                        _manifoldThisFrame[pair] = m;
                    }
                }
            }
        }

        // 3) Emit Enter/Stay/Exit
        EmitPhases();

        // 4) Rotate sets
        _pairsPrev.Clear();

        foreach (var p in _pairsNow)
        {
            _pairsPrev.Add(p);
        }
    }
    
    public IEnumerable<(ICollider A, ICollider B, ContactManifold Manifold)>
        QueryContacts(IBody body, Vector2 position, uint collisionMask = int.MaxValue)
    {
        if (body == null || !body.Enabled)
            yield break;

        // Broadphase: bounding box moved by delta
        var targetBB = new RectF(position.X, position.Y, body.BoundingBox.Width, body.BoundingBox.Height);
        var candidates = _spatial.Query(targetBB);
        if (candidates.Count == 0)
            yield break;

        // Move this body to a projected position temporarily
        var testPosition = position;
        var originalPos  = body.Position;
        body.Position = testPosition;

        try
        {
            foreach (var other in candidates)
            {
                if (ReferenceEquals(other, body) || !other.Enabled)
                    continue;

                // don’t test static vs static
                if (body.BodyType == BodyType.Static && other.BodyType == BodyType.Static)
                    continue;

                foreach (var a in body.Colliders)
                {
                    if (!a.Enabled) continue;

                    foreach (var b in other.Colliders)
                    {
                        if (!b.Enabled) continue;
                        if ((a.Layer & b.CollisionMask) == 0u) continue;
                        if ((b.Layer & a.CollisionMask) == 0u) continue;
                        if ((b.Layer & collisionMask) == 0u) continue;

                        // ignore triggers if you don’t care
                        if (a.IsSensor || b.IsSensor) continue;

                        if (_providers.CollideWith(a, b, out var m))
                            yield return (a, b, m);
                    }
                }
            }
        }
        finally
        {
            // restore position afterwards
            body.Position = originalPos;
        }
    }

    // ---------- Helpers ----------

    private static ContactPair MakePair(ICollider a, ICollider b)
    {
        // Stable ordering by body index, then by reference hash (fast, session-stable)
        var ai = a.Body.StableIndex;
        var bi = b.Body.StableIndex;

        if (ai < bi || (ai == bi && GetRefHash(a) <= GetRefHash(b)))
            return new ContactPair(a, b);

        return new ContactPair(b, a);
    }

    private void EmitPhases()
    {
        // Enter / Stay
        foreach (var pair in _pairsNow)
        {
            var was = _pairsPrev.Contains(pair);
            if (pair.A.IsSensor || pair.B.IsSensor)
            {
                foreach (var s in _sinks)
                {
                    if (s is ICollisionEventsFiltered)
                    {
                        if (!pair.ContainsOwner(s))
                            continue;
                    }
                        
                    s.OnTrigger(was ? ContactPhase.Stay : ContactPhase.Enter, pair);
                }

                continue;
            }

            var hasM = _manifoldThisFrame.TryGetValue(pair, out var mNow);
            var manifold = hasM ? mNow : default;

            foreach (var s in _sinks)
            {
                if (s is ICollisionEventsFiltered)
                {
                    if (!pair.ContainsOwner(s))
                        continue;
                }
                
                s.OnContact(was ? ContactPhase.Stay : ContactPhase.Enter, pair, manifold);
            }
        }

        // Exit
        foreach (var oldPair in _pairsPrev)
        {
            if (_pairsNow.Contains(oldPair))
                continue;

            if (oldPair.A.IsSensor || oldPair.B.IsSensor)
            {
                foreach (var s in _sinks)
                {
                    if (s is ICollisionEventsFiltered)
                    {
                        if (!oldPair.ContainsOwner(s))
                            continue;
                    }
                    
                    s.OnTrigger(ContactPhase.Exit, oldPair);
                }

                continue;
            }

            foreach (var s in _sinks)
            {
                if (s is ICollisionEventsFiltered)
                {
                    if (!oldPair.ContainsOwner(s))
                        continue;
                }
                
                s.OnContact(ContactPhase.Exit, oldPair, default);
            }
        }
    }

    private static Vector2 ApplyMovementConstraint(Vector2 v, float angleDeg)
    {
        if (angleDeg <= 0f)
            return v;

        if (v.LengthSquared() <= 1e-12f)
            return v;

        var ang = MathF.Atan2(v.Y, v.X) * 180f / MathF.PI;
        var snapped = MathF.Round(ang / angleDeg) * angleDeg;
        var r = snapped * MathF.PI / 180f;
        var s = v.Length();

        return new Vector2(MathF.Cos(r), MathF.Sin(r)) * s;
    }

    private static void SetStableIndex(IBody body, int idx)
    {
        // Give your concrete body an internal setter and call it here.
        (body as IInternalBody)?.SetStableIndex(idx);
    }

    private static int GetRefHash(object o)
    {
        return RuntimeHelpers.GetHashCode(o);
    }
}
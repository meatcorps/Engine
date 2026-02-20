using System.Numerics;
using Meatcorps.Engine.Boids.Data;
using Meatcorps.Engine.Boids.Enums;
using Meatcorps.Engine.Boids.Interfaces;
using Meatcorps.Engine.Boids.Utilities;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Interfaces.Grid;

namespace Meatcorps.Engine.Boids.Services;

public class FlockController
{
    private readonly ISpatialEntityGrid _grid;
    private readonly BoidConfig _cfg;

    private readonly List<(IBoidAgent agent, IGridItem item)> _agents = new();
    private readonly List<IBoidAgent> _neighbors = new(64);

    private RectF? _bounds;
    private BoidBoundsPolicy _policy = BoidBoundsPolicy.Ignore;
    private ITargetSelector? _selector;

    private float _retargetTimer;
    private Vector2? _cachedTarget;
    private const float NearTargetEpsilon = 8f; 
    private const float SlowArrivalRadius = 64f; 

    private readonly Dictionary<IBoidAgent, BurstState> _burst = new();
    private readonly Random _rng = new(1337);

    public event Action<IBoidAgent, Vector2>? OnNearTarget;

    private float _frenzy; 

    private static float Mix(float a, float b, float t) => a + (b - a) * t;

    public FlockController(BoidConfig cfg, ISpatialEntityGrid grid)
    {
        _cfg = cfg;
        _grid = grid;
        _retargetTimer = 0f;
    }

    public void SetFrenzy(float level)
    {
        _frenzy = MathF.Max(0f, MathF.Min(1f, level));
    }

    public void SetBounds(RectF? worldBounds, BoidBoundsPolicy policy)
    {
        _bounds = worldBounds;
        _policy = policy;
    }

    public void SetTargetSelector(ITargetSelector selector)
    {
        _selector = selector;
        _retargetTimer = 0f; 
    }

    public void AddAgent(IBoidAgent agent, IGridItem gridItemForAgent)
    {
        _agents.Add((agent, gridItemForAgent));
        _grid.Add(gridItemForAgent);

        var (minI, maxI) = _cfg.BurstIntervalSeconds;
        _burst[agent] = new BurstState
        {
            NextBurstAt = _rng.NextSingle() * (maxI - minI) + minI,
            BurstEndsAt = -1f,
            Dir = Vector2.Zero
        };
    }

    public void RemoveAgent(IBoidAgent agent)
    {
        for (var i = _agents.Count - 1; i >= 0; i--)
        {
            if (!ReferenceEquals(_agents[i].agent, agent))
            {
                continue;
            }

            _grid.Remove(_agents[i].item);
            _agents.RemoveAt(i);
            break;
        }

        _burst.Remove(agent);
    }

    public void Clear()
    {
        foreach (var (_, item) in _agents)
            _grid.Remove(item);

        _agents.Clear();
        _burst.Clear();
    }

    public void Update(float deltaTime, float nowSeconds)
    {
        var effMaxSpeed = Mix(_cfg.MaxSpeed, _cfg.MaxSpeed * 1.10f, _frenzy);
        var effMaxForce = Mix(_cfg.MaxForce, _cfg.MaxForce * 1.50f, _frenzy);

        var effWSeek = Mix(_cfg.WeightSeek, _cfg.WeightSeek * 1.25f, _frenzy);
        var effWSep = Mix(_cfg.WeightSeparation, _cfg.WeightSeparation * 0.90f, _frenzy);
        var effWAlign = Mix(_cfg.WeightAlignment, _cfg.WeightAlignment * 0.35f, _frenzy);
        var effWCoh = Mix(_cfg.WeightCohesion, _cfg.WeightCohesion * 0.40f, _frenzy);
        var effWWander = Mix(_cfg.WeightWander, _cfg.WeightWander * 1.50f, _frenzy);

        var effWanderJit = Mix(_cfg.WanderJitter, _cfg.WanderJitter * 1.40f, _frenzy);
        var effTangentW = Mix(_cfg.TangentSeekNearWeight, _cfg.TangentSeekNearWeight * 1.20f, _frenzy);

        var effRetarget = Mix(_cfg.RetargetIntervalSeconds, _cfg.RetargetIntervalSeconds * 0.60f, _frenzy);

        var effBurstStr = Mix(_cfg.BurstStrength, _cfg.BurstStrength * 1.40f, _frenzy);
        var effBurstDur = Mix(_cfg.BurstDurationSeconds, _cfg.BurstDurationSeconds * 1.20f, _frenzy);
        var effBurstMin = Mix(_cfg.BurstIntervalSeconds.min, _cfg.BurstIntervalSeconds.min * 0.60f, _frenzy);
        var effBurstMax = Mix(_cfg.BurstIntervalSeconds.max, _cfg.BurstIntervalSeconds.max * 0.70f, _frenzy);

        _retargetTimer -= deltaTime;
        if (_retargetTimer <= 0f)
        {
            _retargetTimer = MathF.Max(0.05f, effRetarget);
            _cachedTarget = _selector?.GetTarget(nowSeconds);
        }

        var nr = _cfg.NeighborRadius; 
        var nr2 = nr * 2f;

        foreach (var (agent, item) in _agents)
        {
            if (!agent.IsActive) continue;

            var aabb = new RectF(agent.Position.X - nr, agent.Position.Y - nr, nr2, nr2);
            var hits = _grid.Query(aabb);

            _neighbors.Clear();
            foreach (var h in hits)
            {
                if (ReferenceEquals(h, item)) continue;
                if (h is IBoidAgent b && b.IsActive) _neighbors.Add(b);
            }

            var seek = Vector2.Zero;
            if (_cachedTarget.HasValue)
            {
                var toTarget = _cachedTarget.Value - agent.Position;
                if (toTarget.LengthSquared() < SlowArrivalRadius * SlowArrivalRadius)
                    seek = BoidBehaviors.Arrive(agent.Position, agent.Velocity, _cachedTarget.Value, SlowArrivalRadius,
                        effMaxSpeed);
                else
                    seek = BoidBehaviors.Seek(agent.Position, agent.Velocity, _cachedTarget.Value, effMaxSpeed);
            }

            var sep = BoidBehaviors.Separation(agent.Position, _neighbors, _cfg.DesiredSeparation, effMaxSpeed);
            var ali = BoidBehaviors.Alignment(agent.Position, _neighbors, _cfg.NeighborRadius, effMaxSpeed);
            var coh = BoidBehaviors.Cohesion(agent.Position, _neighbors, _cfg.NeighborRadius, effMaxSpeed);
            var wan = BoidBehaviors.Wander(agent.Velocity, effWanderJit, nowSeconds);

            var tangentSteer = Vector2.Zero;
            if (_cachedTarget.HasValue)
            {
                var to = _cachedTarget.Value - agent.Position;
                var dSq = to.LengthSquared();
                var r = _cfg.NearTargetRadius;

                if (dSq > 1e-8f && dSq < r * r)
                {
                    var dir = to / MathF.Sqrt(dSq);
                    var perp = new Vector2(-dir.Y, dir.X);
                    var sign = _rng.Next(0, 2) == 0 ? -1f : 1f;

                    var tangentDesired = perp * sign * effMaxSpeed; // desired sideways velocity
                    tangentSteer = (tangentDesired - agent.Velocity).LimitMagnitude(effMaxForce);
                }
            }

            var bst = _burst[agent];

            if (nowSeconds >= bst.NextBurstAt)
            {
                bst.BurstEndsAt = nowSeconds + effBurstDur;
                var angle = _rng.NextSingle() * MathF.Tau;
                bst.Dir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                bst.NextBurstAt = nowSeconds + _rng.NextSingle() * (effBurstMax - effBurstMin) + effBurstMin;
            }

            var burstAccel = Vector2.Zero;
            if (nowSeconds < bst.BurstEndsAt)
            {
                burstAccel = bst.Dir * (effMaxForce * effBurstStr);
                seek *= 0.65f; 
            }

            _burst[agent] = bst;

            var accel =
                (seek * effWSeek) +
                (sep * effWSep) +
                (ali * effWAlign) +
                (coh * effWCoh) +
                (wan * effWWander);

            accel = accel.LimitMagnitude(effMaxForce);

            if (tangentSteer != Vector2.Zero)
                accel += tangentSteer * effTangentW;

            accel += burstAccel;

            var newVelocity = (agent.Velocity + accel * deltaTime).LimitMagnitude(effMaxSpeed);
            var newPosition = agent.Position + newVelocity * deltaTime;

            if (_bounds.HasValue)
            {
                if (_policy == BoidBoundsPolicy.Clamp)
                    newPosition = _bounds.Value.ClampPoint(newPosition);
                else if (_policy == BoidBoundsPolicy.Wrap)
                    newPosition = _bounds.Value.WrapPoint(newPosition);
            }

            agent.Velocity = newVelocity;
            agent.Position = newPosition;
            _grid.Update(item);

            if (_cachedTarget.HasValue)
            {
                var dSq = agent.Position.DistanceSquared(_cachedTarget.Value);
                if (dSq <= NearTargetEpsilon * NearTargetEpsilon)
                    OnNearTarget?.Invoke(agent, _cachedTarget.Value);
            }
        }
    }

    private struct BurstState
    {
        public float NextBurstAt;
        public float BurstEndsAt;
        public Vector2 Dir; 
    }
}
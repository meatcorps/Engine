using System.Numerics;
using Meatcorps.Engine.Collision.Enums;
using Meatcorps.Engine.Collision.Interfaces;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Interfaces.Grid;

namespace Meatcorps.Engine.Collision.Providers.Bodies;

public sealed class Body : IBody, IInternalBody
{
    private readonly List<ICollider> _colliders = new List<ICollider>(4);

    private RectF _bbox;
    private bool _bboxDirty = true;
    private Vector2 _position;

    public BodyType BodyType { get; private set; } = BodyType.Dynamic;
    public Vector2 Position
    {
        get => _position;
        set
        {
            if (_position.X.EqualsSafe(value.X) && _position.Y.EqualsSafe(value.Y))
                return;
            
            _position = value;
            _bboxDirty = true;
        }
    }
    public Vector2 PreviousPosition { get; set; }
    public Vector2 Velocity { get; set; }
    public object Owner { get; set; }
    public IWorldService WorldService { get; }
    public IEnumerable<ICollider> Colliders
    {
        get { return _colliders; }
    }

    public float Mass { get; private set; } = 1f;

    public float Restitution { get; private set; } = 0f;

    public float Friction { get; private set; } = 0f;

    public float LinearDamping { get; private set; } = 0f;

    public float MaxSpeed { get; private set; } = 0f;

    public float GravityScale { get; private set; } = 0f;

    public bool IsAwake { get; set; } = true;

    public bool CanSleep { get; private set; } = false;

    public int StableIndex { get; private set; } = -1;

    public bool Enabled { get; set; } = true;

    public float MovementConstraintAngle { get; private set; } = 0f;

    public RectF BoundingBox
    {
        get
        {
            if (_bboxDirty)
            {
                RecalculateBoundingBox();
            }

            return _bbox;
        }
    }

    public Body(IWorldService world, object owner = null)
    {
        WorldService = world;
        Owner = owner;
    }

    // --------- Fluent config ---------

    public Body SetType(BodyType type)
    {
        BodyType = type;
        return this;
    }

    public Body SetMass(float mass)
    {
        Mass = MathF.Max(0f, mass);
        return this;
    }

    public Body SetRestitution(float r)
    {
        Restitution = Math.Clamp(r, 0f, 1f);
        return this;
    }

    public Body SetFriction(float f)
    {
        Friction = Math.Clamp(f, 0f, 1f);
        return this;
    }

    public Body SetLinearDamping(float d)
    {
        LinearDamping = MathF.Max(0f, d);
        return this;
    }

    public Body SetMaxSpeed(float s)
    {
        MaxSpeed = MathF.Max(0f, s);
        return this;
    }

    public Body SetGravityScale(float g)
    {
        GravityScale = g;
        return this;
    }

    public Body SetCanSleep(bool can)
    {
        CanSleep = can;
        return this;
    }

    public Body SetMovementConstraintAngle(float degrees)
    {
        MovementConstraintAngle = MathF.Abs(degrees);
        return this;
    }

    public Body SetEnabled(bool enabled)
    {
        Enabled = enabled;
        return this;
    }

    // --------- Collider management ---------

    public Body AddCollider(ICollider collider)
    {
        if (collider == null)
            return this;

        if (_colliders.Contains(collider))
            return this;

        if (!ReferenceEquals(collider.Body, this))
            throw new InvalidOperationException("Collider.Body must point to this Body.");

        _colliders.Add(collider);
        _bboxDirty = true;
        return this;
    }

    public Body RemoveCollider(ICollider collider)
    {
        if (collider == null)
            return this;

        if (_colliders.Remove(collider))
            _bboxDirty = true;

        return this;
    }

    public Body SetLayer(uint layer)
    {
        foreach (var collider in _colliders)
            collider.Layer = layer;
        
        return this;
    }

    public Body SetMask(uint mask)
    {
        foreach (var collider in _colliders)
            collider.CollisionMask = mask;
        
        return this;
    }

    // -------- internals --------
    void IInternalBody.SetStableIndex(int index)
    {
        StableIndex = index;
    }

    void IInternalBody.MarkBoundsDirty()
    {
        _bboxDirty = true;
    }

    private void RecalculateBoundingBox()
    {
        // If no colliders, create a tiny box at position (still valid)
        var first = true;
        var minX = 0f;
        var minY = 0f;
        var maxX = 0f;
        var maxY = 0f;

        foreach (var c in _colliders)
        {
            if (!c.Enabled)
                continue;

            var r = c.WorldRect;

            if (first)
            {
                minX = r.X;
                minY = r.Y;
                maxX = r.X + r.Width;
                maxY = r.Y + r.Height;
                first = false;
            }
            else
            {
                if (r.X < minX)
                    minX = r.X;

                if (r.Y < minY)
                    minY = r.Y;

                if (r.X + r.Width > maxX)
                    maxX = r.X + r.Width;

                if (r.Y + r.Height > maxY)
                    maxY = r.Y + r.Height;
            }
        }

        if (first)
        {
            // no colliders; zero-size at current position
            _bbox = new RectF(Position.X, Position.Y, 0f, 0f);
            _bboxDirty = false;
            return;
        }

        _bbox = new RectF(minX, minY, maxX - minX, maxY - minY);
        _bboxDirty = false;
    }

    public void Dispose()
    {
        WorldService.UnregisterBody(this);
    }
}
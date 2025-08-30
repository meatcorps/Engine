using System.Numerics;
using Meatcorps.Engine.Collision.Data;
using Meatcorps.Engine.Collision.Interfaces;
using Meatcorps.Engine.Collision.Providers.Bodies;
using Meatcorps.Engine.Core.Data;

namespace Meatcorps.Engine.Collision.Colliders;

public class RectCollider : ICollider
{
    private RectF _localRect;
    private bool _enabled = true;

    public IBody Body { get; }
    public bool IsSensor { get; private set; }
    public uint Layer { get; set; }
    public uint CollisionMask { get; set; }

    public uint Tag { get; set; }
    
    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (_enabled == value)
                return;

            _enabled = value;
            MarkBoundsDirty();
        }
    }

    public RectF LocalRect
    {
        get => _localRect;
        set
        {
            if (_localRect.Equals(value))
                return;

            _localRect = value;
            MarkBoundsDirty();
        }
    }

    public RectF WorldRect => new RectF(
        Body.Position.X + _localRect.X,
        Body.Position.Y + _localRect.Y,
        _localRect.Width,
        _localRect.Height);

    public RectCollider(IBody body, RectF localRect)
    {
        Body = body;
        _localRect = localRect;
        MarkBoundsDirty();
    }

    private void MarkBoundsDirty()
    {
        if (Body is IInternalBody ib)
            ib.MarkBoundsDirty();
    }

    // fluent setters
    public RectCollider SetSensor(bool sensor)
    {
        IsSensor = sensor;
        return this;
    }

    public RectCollider SetLayer(uint layer)
    {
        Layer = layer;
        return this;
    }

    public RectCollider SetMask(uint mask)
    {
        CollisionMask = mask;
        return this;
    }
    
    public bool CollidesWith(ICollider other)
    {
        var ra = WorldRect;
        var rb = other.WorldRect;
        return RectF.Intersects(ref ra, ref rb);
    }

    public void Solve(ICollider other)
    {
        if (Body.WorldService.ColliderProviders.CollideWith(this, other, out var manifold))
            Body.WorldService.ColliderProviders.Solve(this, other, manifold, Body.WorldService.Policy);
    }
}
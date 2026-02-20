using Meatcorps.Engine.Collision.Abstractions;
using Meatcorps.Engine.Collision.Data;
using Meatcorps.Engine.Collision.Interfaces;
using Meatcorps.Engine.Core.Data;

namespace Meatcorps.Engine.Collision.Services;

public sealed class ColliderProviderRegistry : IColliderProviderRegistry, IColliderProvider
{
    private readonly Dictionary<ColliderSet, IColliderProvider?> _map = new();
    private readonly Dictionary<Type, int> _ids = new();
    private int _next = 1;

    public ColliderProviderRegistry Register<T1, T2>(BaseCollideProvider<T1, T2> p)
        where T1 : class, ICollider where T2 : class, ICollider
    {
        if (!typeof(T1).IsClass || typeof(T1).IsAbstract)
            throw new ArgumentException($"{typeof(T1).FullName} is not a concrete collider provider");
        if (!typeof(T2).IsClass || typeof(T2).IsAbstract)
            throw new ArgumentException($"{typeof(T2).FullName} is not a concrete collider provider");
        var key = new ColliderSet(GetId(typeof(T1)), GetId(typeof(T2)));
        _map[key] = p;
        return this;
    }

    public bool TryGet(ICollider a, ICollider b, out IColliderProvider? provider, out bool swapped)
    {
        var idA = GetId(a.GetType());
        var idB = GetId(b.GetType());
        var key = new ColliderSet(idA, idB);
        if (_map.TryGetValue(key, out provider))
        {
            swapped = idA > idB;
            return true;
        }

        swapped = false;
        return false;
    }

    private int GetId(Type t)
    {
        if (_ids.TryGetValue(t, out var id)) return id;
        id = _next++;
        _ids[t] = id;
        return id;
    }

    public bool CollideWith(ICollider a, ICollider b, out ContactManifold manifold)
    {
        manifold = default;
        if (!RectF.Intersects(a.WorldRect, b.WorldRect))
            return false;

        if (!TryGet(a, b, out var provider, out var swapped))
            return false;
        
        return swapped
            ? provider?.CollideWith(b, a, out manifold) ?? false
            : provider?.CollideWith(a, b, out manifold) ?? false;
    }

    public bool Solve(ICollider a, ICollider b, in ContactManifold m, IResolutionPolicy policy)
    {
        if (!RectF.Intersects(a.WorldRect, b.WorldRect))
            return false;

        if (!TryGet(a, b, out var provider, out var swapped))
            return false;
        
        return swapped
            ? provider?.Solve(b, a, m, policy) ?? false
            : provider?.Solve(a, b, m, policy) ?? false;
    }
}
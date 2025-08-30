using System.Numerics;
using Meatcorps.Engine.Collision.Interfaces;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.GridSystem;
using Meatcorps.Engine.Core.Interfaces.Grid;

namespace Meatcorps.Engine.Collision.Providers.WorldEntityResource;

public class SpatialEntityGridResource: IWorldEntityResource
{
    private readonly ISpatialEntityGrid _grid;
    private ThreadLocal<HashSet<IBody>> _collidersThreadLocal = new(() => new HashSet<IBody>());

    public SpatialEntityGridResource(ISpatialEntityGrid? grid)
    {
        grid ??= new SpatialEntityGrid(32);
        _grid = grid;
    }
    
    public void Add(IBody collider)
    {
        _grid.Add(collider);
    }

    public void Remove(IBody collider)
    {
        _grid.Remove(collider);
    }

    public void Update(IBody collider)
    {
        _grid.Update(collider);
    }

    public HashSet<IBody> Query(RectF queryAABB)
    {
        _collidersThreadLocal.Value!.Clear();
        foreach (var gridItem in _grid.Query(queryAABB))
        {
            if (gridItem is IBody body)
                _collidersThreadLocal.Value.Add(body);
        }
        return _collidersThreadLocal.Value;
    }

    public HashSet<IBody> Query(Vector2 position)
    {
        _collidersThreadLocal.Value!.Clear();
        foreach (var gridItem in _grid.Query(position))
        {
            if (gridItem is IBody body)
                _collidersThreadLocal.Value.Add(body);
        }
        return _collidersThreadLocal.Value;
    }
}
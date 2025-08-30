using System.Buffers;
using System.Collections.Concurrent;
using System.Drawing;
using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Interfaces.Grid;
using Meatcorps.Engine.Core.Utilities;

namespace Meatcorps.Engine.Core.GridSystem;

public class SpatialEntityGrid : ISpatialEntityGrid
{
    public float CellSize { get; }
    private readonly ConcurrentDictionary<(int, int), ThreadSafeList<IGridItem>> _grid;
    private readonly ConcurrentDictionary<IGridItem, RectF> _previousPositions;
    private readonly ThreadLocal<List<(int, int)>> _overlappingCells = new(() => new List<(int, int)>());
    private readonly ThreadLocal<HashSet<IGridItem>> _queryColliders = new(() => new HashSet<IGridItem>());
     
    public SpatialEntityGrid(float cellSize)
    {
        CellSize = cellSize;
        _grid = new ConcurrentDictionary<(int, int), ThreadSafeList<IGridItem>>();
        _previousPositions = new ConcurrentDictionary<IGridItem, RectF>();
        var _test = new HashSet<int>();
       // _test.GetEnumerator(5)
    }

    public void Add(IGridItem collider)
    {
        _previousPositions.TryAdd(collider, collider.BoundingBox);
        GetOverlappingCells(collider.BoundingBox);
        foreach (var cell in _overlappingCells.Value!)
        {
            if (!_grid.TryGetValue(cell, out var colliders))
            {
                colliders = new ThreadSafeList<IGridItem>(); 
                _grid[cell] = colliders;
            }

            colliders.Add(collider);
        }
    }

    public void Remove(IGridItem collider)
    {
        _previousPositions.TryRemove(collider, out _);
        GetOverlappingCells(collider.BoundingBox);
        DoRemove(collider);
    }

    private void DoRemove(IGridItem collider)
    {
        foreach (var cell in _overlappingCells.Value!)
        {
            if (_grid.TryGetValue(cell, out var colliders))
            {
                colliders.Remove(collider);
                if (colliders.Count == 0)
                    _grid.TryRemove(cell, out _);
            }
        }
    }

    public void Update(IGridItem collider)
    {
        var target = _previousPositions[collider];
        if (target == collider.BoundingBox)
            return;
        
        GetOverlappingCells(_previousPositions[collider]);
        _previousPositions.TryRemove(collider, out _);
        DoRemove(collider);
        Add(collider);
    }

    public HashSet<IGridItem> Query(RectF queryAABB)
    {
        _queryColliders.Value!.Clear();
        GetOverlappingCells(queryAABB);
        foreach (var cell in _overlappingCells.Value!)
        {
            if (_grid.TryGetValue(cell, out var colliders))
            {
                using var items = colliders.GetEnumerator();
                while (items.MoveNext())
                {
                    if (queryAABB.Intersects(items.Current.BoundingBox))
                        _queryColliders.Value!.Add(items.Current);
                }
            }
        }
        return _queryColliders.Value!;
    }
    
    public HashSet<IGridItem> Query(Vector2 position)
    {
        var x = (int)Math.Floor(position.X / CellSize);
        var y = (int)Math.Floor(position.Y / CellSize);
        _queryColliders.Value!.Clear();

        if (_grid.TryGetValue((x, y), out var colliders))
        {
            using var items = colliders.GetEnumerator();
            while (items.MoveNext())
            {
                _queryColliders.Value!.Add(items.Current);
            }
        }

        return _queryColliders.Value!;
    }

    private void GetOverlappingCells(RectF aabb)
    {
        var minX = (int)Math.Floor(aabb.Left / CellSize);
        var minY = (int)Math.Floor(aabb.Top / CellSize);
        var maxX = (int)Math.Floor(aabb.Right / CellSize);
        var maxY = (int)Math.Floor(aabb.Bottom / CellSize);
        _overlappingCells.Value!.Clear();
        
        for (var x = minX; x <= maxX; x++)
        {
            for (var y = minY; y <= maxY; y++)
            {
                _overlappingCells.Value!.Add((x, y));
            }
        }
    }
}
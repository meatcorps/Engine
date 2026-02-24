using System.Collections.Concurrent;
using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Interfaces.Grid;
using Meatcorps.Engine.Core.Utilities;

namespace Meatcorps.Engine.Core.GridSystem;

/// <summary>
/// A thread-safe broad-phase spatial hash grid. Entities are bucketed by fixed-size cells so only nearby items need to be compared during collision or range queries. Uses ConcurrentDictionary and ThreadLocal query buffers for safe multi-threaded access.
/// </summary>
public class SpatialEntityGrid : ISpatialEntityGrid
{
    /// <summary>The world-space size of each grid cell. Larger cells reduce bucket count but increase items per query.</summary>
    public float CellSize { get; }
    private readonly ConcurrentDictionary<(int, int), ThreadSafeList<IGridItem>> _grid;
    private readonly ConcurrentDictionary<IGridItem, RectF> _previousPositions;
    private readonly ThreadLocal<List<(int, int)>> _overlappingCells = new(() => new List<(int, int)>());
    private readonly ThreadLocal<HashSet<IGridItem>> _queryColliders = new(() => new HashSet<IGridItem>());

    /// <param name="cellSize">The world-space size of each spatial hash cell.</param>
    public SpatialEntityGrid(float cellSize)
    {
        CellSize = cellSize;
        _grid = new ConcurrentDictionary<(int, int), ThreadSafeList<IGridItem>>();
        _previousPositions = new ConcurrentDictionary<IGridItem, RectF>();
    }

    /// <summary>Inserts an item into all cells overlapping its BoundingBox.</summary>
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

    /// <summary>Removes an item from all cells it currently occupies.</summary>
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

    /// <summary>Efficiently repositions an item after its BoundingBox has changed. More efficient than Remove+Add.</summary>
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

    /// <summary>Returns all items whose BoundingBoxes overlap with the given AABB.</summary>
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

    /// <summary>Returns all items in the cell containing the given world-space position.</summary>
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
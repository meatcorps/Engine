using Meatcorps.Engine.Core.Data;

namespace Meatcorps.Engine.Core.GridSystem;

/// <summary>
/// A flat dictionary-based grid that maps PointInt cells to a single entity of type T. Only one entity per cell is allowed — registering a new entity at the same cell overwrites the previous one.
/// </summary>
public class SingleEntityGrid<T>
{
    private readonly Dictionary<PointInt, T> _entities = new();

    public void Clear() => _entities.Clear();

    /// <summary>Registers an entity at a single cell. Overwrites any existing entity.</summary>
    public void Register(PointInt cell, T entity)
    {
        _entities[cell] = entity;
    }

    /// <summary>Registers the same entity across all cells covered by the rectangle.</summary>
    public void Register(Rect cells, T entity)
    {
        for (var x = cells.Left; x < cells.Right; x++)
        {
            for (var y = cells.Top; y < cells.Bottom; y++)
            {
                _entities[new PointInt(x, y)] = entity;
            }
        }
    }

    /// <summary>Removes the entity at the given cell.</summary>
    public void Remove(PointInt cell)
    {
        _entities.Remove(cell);
    }

    /// <summary>Removes entities from all cells covered by the rectangle.</summary>
    public void Remove(Rect cells)
    {
        for (var x = cells.Left; x < cells.Right; x++)
        {
            for (var y = cells.Top; y < cells.Bottom; y++)
            {
                _entities.Remove(new PointInt(x, y));
            }
        }
    }

    /// <summary>Returns true if a cell has a registered entity.</summary>
    public bool IsOccupied(PointInt cell) => _entities.ContainsKey(cell);

    /// <summary>Attempts to retrieve the entity at the given cell.</summary>
    public bool TryGet(PointInt cell, out T entity)
    {
        return _entities.TryGetValue(cell, out entity!);
    }

    /// <summary>Returns the entity at the given cell, or null. When stayInGrid is true, wraps the cell index modulo 100.</summary>
    public T? Get(PointInt cell, bool stayInGrid = false)
    {
        if (stayInGrid)
        {
            cell = new PointInt(cell.X % 100, cell.Y % 100);
        }


        _entities.TryGetValue(cell, out var entity);
        return entity;
    }

    /// <summary>Read-only view of all registered cell→entity mappings.</summary>
    public IReadOnlyDictionary<PointInt, T> Entities => _entities;

    public T? this[PointInt point] => Get(point);
}
using Meatcorps.Engine.Core.Data;

namespace Meatcorps.Engine.Core.GridSystem;

public class SingleEntityGrid<T>
{
    private readonly Dictionary<PointInt, T> _entities = new();
    
    public void Clear() => _entities.Clear();

    public void Register(PointInt cell, T entity)
    {
        _entities[cell] = entity;
    }
    
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
    
    public void Remove(PointInt cell)
    {
        _entities.Remove(cell);
    }
    
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

    public bool IsOccupied(PointInt cell) => _entities.ContainsKey(cell);

    public bool TryGet(PointInt cell, out T entity)
    {
        return _entities.TryGetValue(cell, out entity!);
    }

    public T? Get(PointInt cell, bool stayInGrid = false)
    {
        if (stayInGrid)
        {
            cell = new PointInt(cell.X % 100, cell.Y % 100);
        }
            
        
        _entities.TryGetValue(cell, out var entity);
        return entity;
    }

    public IReadOnlyDictionary<PointInt, T> Entities => _entities;
    
    public T? this[PointInt point] => Get(point);
}
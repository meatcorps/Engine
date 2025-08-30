using Meatcorps.Engine.Core.Data;

namespace Meatcorps.Engine.Core.GridSystem;

public class GridAnalyzerYX<T>
{
    private readonly List<List<T>> _grid;
    private readonly PointInt _size;
    private PointInt _position;
    private static readonly EqualityComparer<T> Eq = EqualityComparer<T>.Default;
    private List<PointInt> _neighborsVisited = new();
    private PointInt[] _neighborDirections = new[] { new PointInt(0, -1), new PointInt(0, 1), new PointInt(-1, 0), new PointInt(1, 0) };
    
    public GridAnalyzerYX(List<List<T>> grid)
    {
        if (grid is null || grid.Count == 0 || grid[0].Count == 0)
            throw new ArgumentException("Grid cannot be empty", nameof(grid));

        _grid = grid;
        _size = new PointInt(_grid[0].Count, _grid.Count);

        foreach (var row in _grid)
            if (row.Count != _size.X)
                throw new ArgumentException("Grid is not rectangular");
    }

    public PointInt Size => _size;

    private bool InBounds(PointInt p) => p.X >= 0 && p.Y >= 0 && p.X < _size.X && p.Y < _size.Y;

    public void SetPosition(PointInt position)
    {
        if (!InBounds(position)) throw new ArgumentOutOfRangeException(nameof(position));
        _position = position;
    }

    public PointInt GetPosition() => _position;

    public T Get(PointInt point)
    {
        if (!InBounds(point)) throw new ArgumentOutOfRangeException(nameof(point));
        return _grid[point.Y][point.X];
    }

    public bool TryGet(PointInt p, out T value)
    {
        if (!InBounds(p)) { value = default!; return false; }
        value = _grid[p.Y][p.X]; return true;
    }

    public bool Search(T value, out PointInt point)
    {
        for (var y = 0; y < _size.Y; y++)
        for (var x = 0; x < _size.X; x++)
            if (Eq.Equals(_grid[y][x], value))
            { point = new PointInt(x, y); return true; }
        point = default; return false;
    }

    public bool Neighbor(PointInt direction, out T value, bool changePosition = true)
    {
        var np = _position + direction;
        if (!InBounds(np)) { value = default!; return false; }
        value = _grid[np.Y][np.X];
        if (changePosition) _position = np;
        return true;
    }

    public bool Neighbor(PointInt position, PointInt direction, out T value)
    {
        var np = position + direction;
        if (!InBounds(np)) { value = default!; return false; }
        value = _grid[np.Y][np.X];
        return true;
    }
    
    public IEnumerable<PointInt> NeighborEqualSearch(T value, PointInt? position)
    {
        var totalJumps = 0;
        var maxJumps = _size.X * _size.Y;
        if (position is not null && InBounds(position.Value))
            _position = position.Value;
        _neighborsVisited.Clear();
        var neighborFound = false;
        while (true)
        {
            totalJumps++;
            if (totalJumps > maxJumps) 
                break;
            neighborFound = false;
            foreach (var direction in _neighborDirections)
            {
                if (Neighbor(direction, out var neighbor, false) && neighbor!.Equals(value) && !_neighborsVisited.Contains(_position + direction))
                {
                    _position += direction;
                    _neighborsVisited.Add(_position);
                    yield return _position;
                    neighborFound = true;
                    continue;
                }
            }
            if (!neighborFound)
                break;
        }
    }

    public IEnumerable<PointInt> SearchAll(T value)
    {
        for (var y = 0; y < _size.Y; y++)
        for (var x = 0; x < _size.X; x++)
            if (Eq.Equals(_grid[y][x], value))
                yield return new PointInt(x, y);
    }

    public IEnumerable<(PointInt, T)> IterateAll()
    {
        for (var y = 0; y < _size.Y; y++)
        for (var x = 0; x < _size.X; x++)
            yield return (new PointInt(x, y), _grid[y][x]);
    }
}
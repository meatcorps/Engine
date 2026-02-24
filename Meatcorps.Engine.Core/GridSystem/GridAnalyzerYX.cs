using Meatcorps.Engine.Core.Data;

namespace Meatcorps.Engine.Core.GridSystem;

/// <summary>
/// A cursor-based analyzer for a rectangular 2D grid stored as List&lt;List&lt;T&gt;&gt; in [Y][X] order. Supports navigation, neighbor queries, flood-fill-style searches, and full iteration. The grid must be rectangular.
/// </summary>
public class GridAnalyzerYX<T>
{
    private readonly List<List<T>> _grid;
    private readonly PointInt _size;
    private PointInt _position;
    private static readonly EqualityComparer<T> Eq = EqualityComparer<T>.Default;
    private readonly List<PointInt> _neighborsVisited = new();
    private readonly PointInt[] _neighborDirections = [new(0, -1), new(0, 1), new(-1, 0), new(1, 0)
    ];

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

    /// <summary>Width (X) and height (Y) of the grid.</summary>
    public PointInt Size => _size;

    private bool InBounds(PointInt p) => p.X >= 0 && p.Y >= 0 && p.X < _size.X && p.Y < _size.Y;

    /// <summary>Moves the internal cursor to the given cell. Throws if out of bounds.</summary>
    public void SetPosition(PointInt position)
    {
        if (!InBounds(position)) throw new ArgumentOutOfRangeException(nameof(position));
        _position = position;
    }

    /// <summary>Returns the current cursor position.</summary>
    public PointInt GetPosition() => _position;

    /// <summary>Returns the value at the given cell. Throws if out of bounds.</summary>
    public T Get(PointInt point)
    {
        if (!InBounds(point)) throw new ArgumentOutOfRangeException(nameof(point));
        return _grid[point.Y][point.X];
    }

    /// <summary>Attempts to get the value at the given cell. Returns false if out of bounds.</summary>
    public bool TryGet(PointInt p, out T value)
    {
        if (!InBounds(p)) { value = default!; return false; }
        value = _grid[p.Y][p.X]; return true;
    }

    /// <summary>Finds the first cell containing value. Returns false if not found.</summary>
    public bool Search(T value, out PointInt point)
    {
        for (var y = 0; y < _size.Y; y++)
        for (var x = 0; x < _size.X; x++)
            if (Eq.Equals(_grid[y][x], value))
            { point = new PointInt(x, y); return true; }
        point = default; return false;
    }

    /// <summary>Steps from the cursor in the given direction. Optionally advances the cursor. Returns false if the step would go out of bounds.</summary>
    public bool Neighbor(PointInt direction, out T value, bool changePosition = true)
    {
        var np = _position + direction;
        if (!InBounds(np)) { value = default!; return false; }
        value = _grid[np.Y][np.X];
        if (changePosition) _position = np;
        return true;
    }

    /// <summary>Steps from an explicit position in the given direction without moving the cursor.</summary>
    public bool Neighbor(PointInt position, PointInt direction, out T value)
    {
        var np = position + direction;
        if (!InBounds(np)) { value = default!; return false; }
        value = _grid[np.Y][np.X];
        return true;
    }

    /// <summary>Walks connected neighbors that equal value starting from position (or the current cursor). Yields each visited cell.</summary>
    public IEnumerable<PointInt> NeighborEqualSearch(T value, PointInt? position)
    {
        var totalJumps = 0;
        var maxJumps = _size.X * _size.Y;
        if (position is not null && InBounds(position.Value))
            _position = position.Value;
        _neighborsVisited.Clear();
        while (true)
        {
            totalJumps++;
            if (totalJumps > maxJumps)
                break;
            var neighborFound = false;
            foreach (var direction in _neighborDirections)
            {
                if (Neighbor(direction, out var neighbor, false) && neighbor!.Equals(value) && !_neighborsVisited.Contains(_position + direction))
                {
                    _position += direction;
                    _neighborsVisited.Add(_position);
                    yield return _position;
                    neighborFound = true;
                }
            }
            if (!neighborFound)
                break;
        }
    }

    /// <summary>Yields all cells in the grid that contain value.</summary>
    public IEnumerable<PointInt> SearchAll(T value)
    {
        for (var y = 0; y < _size.Y; y++)
        for (var x = 0; x < _size.X; x++)
            if (Eq.Equals(_grid[y][x], value))
                yield return new PointInt(x, y);
    }

    /// <summary>Yields every cell in the grid as (PointInt, T) pairs, in Y-then-X order.</summary>
    public IEnumerable<(PointInt, T)> IterateAll()
    {
        for (var y = 0; y < _size.Y; y++)
        for (var x = 0; x < _size.X; x++)
            yield return (new PointInt(x, y), _grid[y][x]);
    }
}
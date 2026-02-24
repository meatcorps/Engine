using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Pathfinding.Interfaces;

namespace Meatcorps.Engine.Pathfinding.Utilities;

/// <summary>
/// Utility class for calculating grid distances and managing the traversal state.
/// </summary>
public class GridDistanceCalculator
{
    /// <summary>
    /// Gets the readonly distance resource used for distance calculations and validation.
    /// </summary>
    /// <value>The readonly interface to the distance resource.</value>
    public IReadonlyDistanceResource Resource => _resource;
    
    /// <summary>
    /// Gets the set of visited points during the distance calculation process.
    /// </summary>
    /// <value>A readonly set containing all points that have been visited during traversal.</value>
    public IReadOnlySet<PointInt> Visited => _visited;
    
    private IGridDistanceResource _resource { get; }
    private PointInt[] _allowedDirections = [];
    private readonly Queue<(int, PointInt)> _nextToVisit = new();
    private readonly HashSet<PointInt> _visited = new();
    
    /// <summary>
    /// Gets the maximum distance calculated during the last distance calculation operation.
    /// </summary>
    /// <value>The maximum distance value found during traversal.</value>
    public int MaxDistance { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GridDistanceCalculator"/> class.
    /// </summary>
    /// <param name="resource">The grid distance resource used for distance calculations and validation.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="resource"/> is null.</exception>
    public GridDistanceCalculator(IGridDistanceResource resource)
    {
        _resource = resource;
    }

    /// <summary>
    /// Sets the allowed movement directions for distance calculations.
    /// </summary>
    /// <param name="directions">An array of <see cref="PointInt"/> values representing valid movement directions.</param>
    /// <returns>The current <see cref="GridDistanceCalculator"/> instance for method chaining.</returns>
    public GridDistanceCalculator SetAllowedDirections(params PointInt[] directions)
    {
        _allowedDirections = directions;
        return this;   
    }

    /// <summary>
    /// Sets the allowed movement directions to the 4 cardinal directions (up, down, left, right).
    /// </summary>
    /// <param name="directions">This parameter is not used but maintained for consistency with the method signature.</param>
    /// <returns>The current <see cref="GridDistanceCalculator"/> instance for method chaining.</returns>
    /// <remarks>
    /// The cardinal directions are: (-1,0), (1,0), (0,-1), (0,1) representing left, right, up, and down respectively.
    /// </remarks>
    public GridDistanceCalculator Set4AllowedDirections(params PointInt[] directions)
    {
        return SetAllowedDirections(new PointInt(-1, 0), new PointInt(1, 0), new PointInt(0, -1), new PointInt(0, 1));   
    }
    
    /// <summary>
    /// Sets the allowed movement directions to all 8 directions (cardinal and diagonal).
    /// </summary>
    /// <param name="directions">This parameter is not used but maintained for consistency with the method signature.</param>
    /// <returns>The current <see cref="GridDistanceCalculator"/> instance for method chaining.</returns>
    /// <remarks>
    /// The 8 directions include both cardinal directions (up, down, left, right) and diagonal directions.
    /// Cardinal: (-1,0), (1,0), (0,-1), (0,1)
    /// Diagonal: (-1,-1), (-1,1), (1,-1), (1,1)
    /// </remarks>
    public GridDistanceCalculator Set8AllowedDirections(params PointInt[] directions)
    {
        return SetAllowedDirections(new PointInt(-1, 0), new PointInt(1, 0), new PointInt(0, -1), new PointInt(0, 1), new PointInt(-1, -1), new PointInt(-1, 1), new PointInt(1, -1), new PointInt(1, 1));   
    }

    /// <summary>
    /// Calculates distances from the starting point using a breadth-first search algorithm.
    /// </summary>
    /// <param name="start">The starting point for distance calculation.</param>
    /// <param name="keepDistance">The initial distance value to maintain at the starting point. Default is 0.</param>
    /// <param name="maxDistance">The maximum distance to calculate before stopping traversal. Default is <see cref="int.MaxValue"/>.</param>
    /// <remarks>
    /// This method performs a breadth-first traversal starting from the specified point, calculating and storing
    /// distances to all reachable points within the allowed directions. The algorithm respects the maximum distance
    /// limit and updates the internal distance resource with calculated values.
    /// 
    /// If the starting point is not valid according to the resource, the method will find the nearest valid point
    /// using brute force search before beginning the calculation.
    /// </remarks>
    public void Calculate(PointInt start, int keepDistance = 0, int maxDistance = int.MaxValue)
    {
        _nextToVisit.Clear();
        _visited.Clear();
        start = GetNearestWithBruteForce(start);
        _nextToVisit.Enqueue((-keepDistance, start));
        MaxDistance = 0;
        _resource.Set(start, -keepDistance);
        while (_nextToVisit.Count > 0)
        {
            _nextToVisit.TryDequeue(out var item);
            var currentDistance = item.Item1;
            var position = item.Item2;
            foreach (var direction in _allowedDirections)
            {
                var neighbor = position + direction;
                if (_resource.TryGet(neighbor, direction, out var distance, out var additionalCost, out var realNeighbor))
                {
                    
                    if (_visited.Contains(realNeighbor))
                        continue;

                    if (_visited.Add(realNeighbor))
                        distance = int.MaxValue;
                    
                    if (Math.Abs(currentDistance) <= distance)
                    {
                        var totalCost = currentDistance + (currentDistance >= 0 ? additionalCost: -additionalCost) + 1;

                        _resource.Set(realNeighbor, Math.Min(totalCost, maxDistance));
                        MaxDistance = Math.Max(MaxDistance, Math.Abs(totalCost));
                        
                        if (totalCost > maxDistance)
                            continue;
                        
                        _nextToVisit.Enqueue((totalCost, realNeighbor));
                    }
                }
            }
        }
        _resource.Set(start, -keepDistance);
        if (keepDistance > 0)
            foreach (var position in _visited)
            {
                var distance = _resource.Get(position);
                if (distance < 0)
                    _resource.Set(position, Math.Abs(distance));
            }
    }

    /// <summary>
    /// Finds the nearest valid point to the specified point using a brute force search algorithm.
    /// </summary>
    /// <param name="from">The reference point to search from.</param>
    /// <returns>
    /// The nearest valid point as determined by the distance resource. If the input point is already valid,
    /// it returns the input point unchanged.
    /// </returns>
    /// <remarks>
    /// This method performs an exhaustive search across the entire bounds of the resource to find the closest
    /// valid point. It uses squared distance calculation for performance optimization and converts between
    /// <see cref="PointInt"/> and <see cref="Vector2"/> for distance calculations.
    /// 
    /// The search complexity is O(width * height) of the resource bounds, so it should be used judiciously
    /// for large grids.
    /// </remarks>
    public PointInt GetNearestWithBruteForce(PointInt from)
    {
        if (_resource.IsValid(from))
            return from;
        
        var fromPoint = from.ToVector2();
        var closest = new Vector2(float.MaxValue, float.MaxValue);
        var previousDistance = float.MaxValue;
        
        for (var x = 0; x < _resource.Bounds.Width; x++)
        for (var y = 0; y < _resource.Bounds.Height; y++)
        {
            if (_resource.IsValid(new PointInt(x, y)))
            {
                var possibleDistance = fromPoint.DistanceSquared(new Vector2(x, y));
                if (possibleDistance < previousDistance)
                {
                    closest = new Vector2(x, y);
                    previousDistance = possibleDistance;   
                }
            }
        }
        
        return closest.ToPointInt();   
    }
}
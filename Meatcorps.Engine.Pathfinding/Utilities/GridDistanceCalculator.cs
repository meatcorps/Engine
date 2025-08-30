using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Pathfinding.Interfaces;

namespace Meatcorps.Engine.Pathfinding.Utilities;

public class GridDistanceCalculator
{
    public IReadonlyDistanceResource Resource => _resource;
    public IReadOnlySet<PointInt> Visited => _visited;
    private IGridDistanceResource _resource { get; }
    private PointInt[] _allowedDirections = [];
    private Queue<(int, PointInt)> _nextToVisit = new();
    private HashSet<PointInt> _visited = new();
    public int MaxDistance { get; private set; }

    public GridDistanceCalculator(IGridDistanceResource resource)
    {
        _resource = resource;
    }

    public GridDistanceCalculator SetAllowedDirections(params PointInt[] directions)
    {
        _allowedDirections = directions;
        return this;   
    }

    public GridDistanceCalculator Set4AllowedDirections(params PointInt[] directions)
    {
        return SetAllowedDirections(new PointInt(-1, 0), new PointInt(1, 0), new PointInt(0, -1), new PointInt(0, 1));   
    }
    
    public GridDistanceCalculator Set8AllowedDirections(params PointInt[] directions)
    {
        return SetAllowedDirections(new PointInt(-1, 0), new PointInt(1, 0), new PointInt(0, -1), new PointInt(0, 1), new PointInt(-1, -1), new PointInt(-1, 1), new PointInt(1, -1), new PointInt(1, 1));   
    }

    public void Calculate(PointInt start, int keepDistance = 0, int maxDistance = int.MaxValue)
    {
        _nextToVisit.Clear();
        _visited.Clear();
        start = GetNearestWithBruteForce(start);
        _nextToVisit.Enqueue((-keepDistance, start));
        MaxDistance = 0;
        _resource.Set(start, -keepDistance);
        var iterations = 0;
        while (_nextToVisit.Count > 0)
        {
            iterations++;
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

                    if (!_visited.Contains(realNeighbor))
                    {
                        _visited.Add(realNeighbor);
                        distance = int.MaxValue;
                    }
                    
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
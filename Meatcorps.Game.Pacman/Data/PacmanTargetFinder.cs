using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Pathfinding.ResourceBinder;

namespace Meatcorps.Game.Pacman.Data;

public class PacmanTargetFinder: BaseGridDistanceResource
{
    public override Rect Bounds { get; }
    private readonly LevelData _levelData;
    private PointInt[] _directions = new[] { new PointInt(0, 1), new PointInt(1, 0), new PointInt(0, -1), new PointInt(-1, 0) };
    public PacmanTargetFinder(LevelData levelData)
    {
        _levelData = levelData;
        Bounds = new Rect(PointInt.Zero, new PointInt(levelData.LevelWidth, levelData.LevelHeight));
    }
    
    protected override bool OnIsValid(PointInt point)
    {
        if (!_levelData.Map.TryGet(point, out var item))
            return false;
        
        return item.Walkable;
    }
    
    protected override PointInt MutatePosition(PointInt from)
    {
        return from.Warp(_levelData.LevelWidth, _levelData.LevelHeight);
    }

    public void GetPath(PointInt from, PointInt to, List<PointInt> path, int maxPath = 50)
    {
        path.Clear();
        path.Add(from);
        var current = from;
        var count = 0;
        while (current != to && count < maxPath)
        {
            count++;
            var distance = int.MaxValue;
            var bestNeighbor = PointInt.Zero;
            foreach (var direction in _directions)
            {
                if (!IsValid(current + direction))
                    continue;
                
                var neighborDistance = Get(current + direction);
                if (neighborDistance < distance)
                {
                    distance = neighborDistance;
                    bestNeighbor = current + direction;
                }
            }
            path.Add(bestNeighbor);
            current = bestNeighbor;
        }
    } 

    protected override bool OnTryGet(PointInt point, PointInt direction, out int additionalCost)
    {
        additionalCost = 0;
        if (!_levelData.Map.TryGet(point, out var item))
            return false;
        
        if (point.X == 0 || point.X == _levelData.LevelWidth - 1 || point.Y == 0 || point.Y == _levelData.LevelHeight - 1)
            additionalCost = 5;
        
        return item.Walkable;
    }
}
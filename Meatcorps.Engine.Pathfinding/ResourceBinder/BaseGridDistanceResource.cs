using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.GridSystem;
using Meatcorps.Engine.Pathfinding.Interfaces;

namespace Meatcorps.Engine.Pathfinding.ResourceBinder;

public abstract class BaseGridDistanceResource: IGridDistanceResource
{
    public SingleEntityGrid<int> DistanceMap { get; } = new(); 

    public bool IsValid(PointInt point)
    {
        return OnIsValid(MutatePosition(point));
    }
    
    protected abstract bool OnIsValid(PointInt point);

    protected virtual PointInt MutatePosition(PointInt from)
    {
        return from;
    } 
    
    public int Get(PointInt point)
    {
        return DistanceMap.Get(MutatePosition(point));
    }

    public abstract Rect Bounds { get; }
    public bool TryGet(PointInt point, PointInt direction, out int? distance, out int additionalCost, out PointInt pointInt)
    {
        distance = int.MaxValue;
        additionalCost = 0;
        pointInt = MutatePosition(point);
        if (!OnTryGet(pointInt, direction, out additionalCost))
            return false;
        distance = Get(pointInt);
        return true;
    }
    
    protected abstract bool OnTryGet(PointInt point, PointInt direction, out int additionalCost);

    public void Set(PointInt point, int distance)
    {
        var realPosition = MutatePosition(point);
        DistanceMap.Remove(realPosition);
        DistanceMap.Register(realPosition, distance);
    }
}
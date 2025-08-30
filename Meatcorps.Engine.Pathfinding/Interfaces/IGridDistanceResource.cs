using Meatcorps.Engine.Core.Data;

namespace Meatcorps.Engine.Pathfinding.Interfaces;

public interface IGridDistanceResource : IReadonlyDistanceResource
{
    Rect Bounds { get; }
    bool TryGet(PointInt point, PointInt direction, out int? distance, out int additionalCost, out PointInt realPoint);
    void Set(PointInt point, int distance);
}

public interface IReadonlyDistanceResource
{
    bool IsValid(PointInt point);
    int Get(PointInt point);
}
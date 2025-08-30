using System.Numerics;
using Meatcorps.Engine.Core.Data;

namespace Meatcorps.Engine.Core.Interfaces.Grid;

public interface ISpatialEntityGrid
{
    void Add(IGridItem collider);
    void Remove(IGridItem collider);
    void Update(IGridItem collider);
    HashSet<IGridItem> Query(RectF queryAABB);
    HashSet<IGridItem> Query(Vector2 position);
}
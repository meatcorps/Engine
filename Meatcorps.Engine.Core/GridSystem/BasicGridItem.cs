using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Interfaces.Grid;

namespace Meatcorps.Engine.Core.GridSystem;

public class BasicGridItem : IGridItem
{
    public BasicGridItem(object owner, Vector2 position, SpatialEntityGrid grid)
    {
        BoundingBox = new RectF(position, new SizeF(grid.CellSize, grid.CellSize));
        Owner = owner;
    }
    
    public RectF BoundingBox { get; set; }

    public object Owner { get; }
}
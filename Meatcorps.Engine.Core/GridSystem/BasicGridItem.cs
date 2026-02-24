using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Interfaces.Grid;

namespace Meatcorps.Engine.Core.GridSystem;

/// <summary>
/// A simple IGridItem implementation that creates a cell-sized bounding box at the given position. Suitable for point-in-cell lookups where the item occupies exactly one grid cell.
/// </summary>
public class BasicGridItem : IGridItem
{
    public BasicGridItem(object owner, Vector2 position, SpatialEntityGrid grid)
    {
        BoundingBox = new RectF(position, new SizeF(grid.CellSize, grid.CellSize));
        Owner = owner;
    }

    public RectF BoundingBox { get; set; }

    public object? Owner { get; }
}
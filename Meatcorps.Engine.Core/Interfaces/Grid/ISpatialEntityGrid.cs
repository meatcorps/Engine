using System.Numerics;
using Meatcorps.Engine.Core.Data;

namespace Meatcorps.Engine.Core.Interfaces.Grid;

/// <summary>
/// A broad-phase spatial hash grid for efficient proximity queries.
/// Items are bucketed by grid cell so only nearby items need to be tested during collision or range queries.
/// </summary>
public interface ISpatialEntityGrid
{
    /// <summary>Inserts an item into the grid at its current <see cref="IGridItem.BoundingBox"/> position.</summary>
    void Add(IGridItem collider);

    /// <summary>Removes an item from the grid.</summary>
    void Remove(IGridItem collider);

    /// <summary>
    /// Repositions an item in the grid after its <see cref="IGridItem.BoundingBox"/> has changed.
    /// More efficient than calling <see cref="Remove"/> followed by <see cref="Add"/>.
    /// </summary>
    void Update(IGridItem collider);

    /// <summary>Returns all items whose bounding boxes overlap with <paramref name="queryAABB"/>.</summary>
    HashSet<IGridItem> Query(RectF queryAABB);

    /// <summary>Returns all items registered in the grid cell that contains <paramref name="position"/>.</summary>
    HashSet<IGridItem> Query(Vector2 position);
}
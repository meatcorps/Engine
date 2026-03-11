using Meatcorps.Engine.Core.Data;

namespace Meatcorps.Engine.Core.Interfaces.Grid;

/// <summary>Represents an item that can be registered in a spatial grid.</summary>
public interface ISpecificGridItem<out T>
{
    /// <summary>The axis-aligned bounding box used for spatial bucketing and overlap tests.</summary>
    RectF BoundingBox { get; }

    /// <summary>The source object this grid item belongs to. Retrieved from query results to identify the entity.</summary>
    T? Owner { get; }
}
using Meatcorps.Engine.Core.Data;

namespace Meatcorps.Engine.Pathfinding.Interfaces;

/// <summary>
/// Represents a grid-based resource that stores distances and provides methods to calculate or retrieve them.
/// Extends <see cref="IReadonlyDistanceResource"/> with additional functionalities for updating distances and handling bounds.
/// </summary>
public interface IGridDistanceResource : IReadonlyDistanceResource
{
    /// <summary>
    /// Gets the rectangular bounds of the grid within which distance-related computations are valid.
    /// The bounds represent the valid area of the grid, defined by its position (X, Y) and size (Width, Height).
    /// </summary>
    Rect Bounds { get; }

    /// <summary>
    /// Attempts to retrieve the distance and additional cost for a grid point in a specified direction.
    /// </summary>
    /// <param name="point">The starting point on the grid from which to calculate.</param>
    /// <param name="direction">The direction to check relative to the starting point.</param>
    /// <param name="distance">
    /// When this method returns, contains the distance value for the grid point in the specified direction, if found; otherwise, null.
    /// </param>
    /// <param name="additionalCost">
    /// When this method returns, contains the additional cost associated with the operation, if applicable.
    /// </param>
    /// <param name="realPoint">
    /// When this method returns, contains the actual grid point being considered after applying calculations such as direction offset.
    /// </param>
    /// <returns>
    /// True if the distance and additional cost were successfully retrieved for the specified parameters; otherwise, false.
    /// </returns>
    bool TryGet(PointInt point, PointInt direction, out int? distance, out int additionalCost, out PointInt realPoint);

    /// <summary>
    /// Updates the distance value for a specific grid point.
    /// </summary>
    /// <param name="point">The grid point whose distance value is to be updated.</param>
    /// <param name="distance">The new distance value to set for the specified grid point.</param>
    void Set(PointInt point, int distance);
}

/// <summary>
/// Represents a read-only grid-based resource that stores distances.
/// </summary>
public interface IReadonlyDistanceResource
{
    /// <summary>
    /// Determines whether a specified grid point is valid within the resource's context.
    /// </summary>
    /// <param name="point">The grid point to validate.</param>
    /// <returns>
    /// True if the specified grid point is valid according to the resource's definition; otherwise, false.
    /// </returns>
    bool IsValid(PointInt point);

    /// <summary>
    /// Retrieves the distance value associated with a specified grid point.
    /// </summary>
    /// <param name="point">The grid point for which the distance value is to be retrieved.</param>
    /// <returns>
    /// The distance value associated with the specified grid point.
    /// </returns>
    int Get(PointInt point);
}
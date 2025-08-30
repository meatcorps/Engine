using Meatcorps.Engine.Core.Data;

namespace Meatcorps.Engine.Core.Extensions;

public static class PointIntExtensions
{
    public static PointInt Warp(this PointInt point, int width, int height)
    {
        return new PointInt(
            point.X.Wrap(width),
            point.Y.Wrap(height)
        );
    }

    public static PointInt Warp(this PointInt point, Rect bounds)
    {
        return new PointInt(
            bounds.X + (point.X - bounds.X).Wrap(bounds.Width),
            bounds.Y + (point.Y - bounds.Y).Wrap(bounds.Height)
        );
    }
}
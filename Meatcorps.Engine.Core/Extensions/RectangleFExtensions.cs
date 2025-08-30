using System.Drawing;
using System.Numerics;
using Meatcorps.Engine.Core.Data;

namespace Meatcorps.Engine.Core.Extensions;

public static class RectangleFExtensions
{
    public static bool Intersects(this RectangleF first, RectangleF second)
    {
        return first.X < second.X + second.Width && first.X + first.Width > second.X && first.Y < second.Y + second.Height && first.Y + first.Height > second.Y;
    }
    
    /// <summary>Clamp a point to the rectangle bounds.</summary>
    public static Vector2 ClampPoint(this RectF rect, Vector2 p)
    {
        var x = p.X;
        var y = p.Y;

        if (x < rect.X)
        {
            x = rect.X;
        }

        var maxX = rect.X + rect.Width;
        if (x > maxX)
        {
            x = maxX;
        }

        if (y < rect.Y)
        {
            y = rect.Y;
        }

        var maxY = rect.Y + rect.Height;
        if (y > maxY)
        {
            y = maxY;
        }

        return new Vector2(x, y);
    }

    /// <summary>Wrap a point around rectangle (toroidal).</summary>
    public static Vector2 WrapPoint(this RectF rect, Vector2 p)
    {
        var x = p.X;
        var y = p.Y;

        var minX = rect.X;
        var maxX = rect.X + rect.Width;
        var minY = rect.Y;
        var maxY = rect.Y + rect.Height;

        if (x < minX)
        {
            x = maxX;
        }
        else if (x > maxX)
        {
            x = minX;
        }

        if (y < minY)
        {
            y = maxY;
        }
        else if (y > maxY)
        {
            y = minY;
        }

        return new Vector2(x, y);
    }

    public static RectF ToRectF(this Rect rect)
    {
        return new RectF(rect.X, rect.Y, rect.Width, rect.Height);
    }
}
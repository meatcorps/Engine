using System.Drawing;
using System.Numerics;
using Meatcorps.Engine.Core.Data;
using SizeF = Meatcorps.Engine.Core.Data.SizeF;

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

    public static RectF GetInnerRect(this RectF rect, PaddingF padding)
    {
        return rect + padding;
    }

    public static RectF GetOuterRect(this RectF rect, MarginF margin)
    {
        return rect + margin;
    }

    public static RectF Align(this RectF outerRect, RectF innerRect, Vector2 uv)
    {
        var position = outerRect.GetCornerBasedOnUV(uv);
        return innerRect.SetPositionWithUV(position, uv);
    }
    
    public static Vector2 GetCornerBasedOnUV(this RectF rect, Vector2 normal)
    {
        return new Vector2(rect.Position.X + normal.X * rect.Width, rect.Position.Y + normal.Y * rect.Height);
    }

    public static IEnumerable<RectF> GetSegments(this RectF rect, int horizontalItems, int verticalItems, float gap = 0, int maxItems = int.MaxValue)
    {
        if (horizontalItems <= 0 || verticalItems <= 0)
            yield break;

        var segmentWidth = rect.Width / horizontalItems;
        var segmentHeight = rect.Height / verticalItems;
        var items = 0;
        for (var x = 0; x < horizontalItems; x++)
        for (var y = 0; y < verticalItems; y++)
        {
            yield return new RectF(rect.Position.X + x * segmentWidth + gap, rect.Position.Y + y * segmentHeight + gap,
                segmentWidth - gap * 2, segmentHeight - gap * 2);
            
            items++;
            if (items >= maxItems) 
                yield break;
        }
    }
    
    public static IEnumerable<RectF> GetSegments(this RectF rect, SizeF elementSize, int horizontalItems, int verticalItems, float gap = 0, Vector2 uv = default, int maxItems = int.MaxValue)
    {
        if (horizontalItems <= 0 || verticalItems <= 0)
            yield break;
        
        var elementRect = new RectF(rect.X, rect.Y, elementSize.Width * horizontalItems + (gap * horizontalItems - 1), elementSize.Height * verticalItems + (gap * horizontalItems - 1));

        rect = rect.Align(elementRect, uv);
        
        var segmentWidth = rect.Width / horizontalItems;
        var segmentHeight = rect.Height / verticalItems;

        for (var x = 0; x < horizontalItems; x++)
        for (var y = 0; y < verticalItems; y++)
            yield return new RectF(rect.Position.X + x * segmentWidth + gap, rect.Position.Y + y * segmentHeight + gap,
                segmentWidth - gap * 2, segmentHeight - gap * 2);
    }

    public static RectF NextPosition(this RectF fromRect, RectF targetRect, Vector2 direction, float gap = 0, Vector2 uv = default)
    {
        var size = new Vector2(direction.X > 0 ? fromRect.Width : targetRect.Width, direction.Y > 0 ? fromRect.Height : targetRect.Height);
        var step = size * direction; 
        step += gap * direction.Sign();
        step += direction.Sign();
        var startPosition = fromRect.GetCornerBasedOnUV(uv);
        
        return new RectF(Vector2.Zero, targetRect.Size).SetPositionWithUV(startPosition + step, uv);
    }
}
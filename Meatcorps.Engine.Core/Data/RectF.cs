using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Utilities;

namespace Meatcorps.Engine.Core.Data;

[DataContract]
public struct RectF : IEquatable<RectF>
{
    public static readonly RectF Empty = new ();
    [DataMember] public float X;
    [DataMember] public float Y;
    [DataMember] public float Width;
    [DataMember] public float Height;

    public float Left => X;

    public float Right => X + Width;

    public float Top => Y;

    public float Bottom => Y + Height;

    public bool IsEmpty => Width.Equals(0.0f) && Height.Equals(0.0f) && X.Equals(0.0f) && Y.Equals(0.0f);

    public Vector2 Position
    {
        get => new Vector2(X, Y);
        set
        {
            X = value.X;
            Y = value.Y;
        }
    }

    public RectF BoundingRectangle => this;

    public SizeF Size
    {
        get => new SizeF(Width, Height);
        set
        {
            Width = value.Width;
            Height = value.Height;
        }
    }

    public Vector2 Center => new Vector2(X + Width * 0.5f, Y + Height * 0.5f);

    public Vector2 TopLeft => new Vector2(X, Y);

    public Vector2 TopRight => new Vector2(X + Width, Y);

    public Vector2 BottomLeft => new Vector2(X, Y + Height);

    public Vector2 BottomRight => new Vector2(X + Width, Y + Height);

    public RectF(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public RectF(Vector2 position, SizeF size)
    {
        X = position.X;
        Y = position.Y;
        Width = size.Width;
        Height = size.Height;
    }

    public static void CreateFrom(Vector2 minimum, Vector2 maximum, out RectF result)
    {
        result.X = minimum.X;
        result.Y = minimum.Y;
        result.Width = maximum.X - minimum.X;
        result.Height = maximum.Y - minimum.Y;
    }

    public static RectF CreateFrom(Vector2 minimum, Vector2 maximum)
    {
        CreateFrom(minimum, maximum, out var result);
        return result;
    }

    public RectF Translated(Vector2 position)
    {
        return new RectF(Position + position, Size);
    }
    
    public static void Union(ref RectF first, ref RectF second, out RectF result)
    {
        result.X = Math.Min(first.X, second.X);
        result.Y = Math.Min(first.Y, second.Y);
        result.Width = Math.Max(first.Right, second.Right) - result.X;
        result.Height = Math.Max(first.Bottom, second.Bottom) - result.Y;
    }
    
    public static RectF Union(RectF first, RectF second)
    {
        Union(ref first, ref second, out var result);
        return result;
    }

    public RectF Union(RectF rectangle)
    {
        Union(ref this, ref rectangle, out var result);
        return result;
    }

    public static void Intersection(
        ref RectF first,
        ref RectF second,
        out RectF result)
    {
        var topLeft1 = first.TopLeft;
        var bottomRight1 = first.BottomRight;
        var topLeft2 = second.TopLeft;
        var bottomRight2 = second.BottomRight;
        var second1 = topLeft2;
        var maximumVector2 = topLeft1.Max(second1);
        var minimumVector2 = bottomRight1.Min(bottomRight2);
        if (minimumVector2.X < maximumVector2.X || minimumVector2.Y < maximumVector2.Y)
            result = new RectF();
        else
            result = RectF.CreateFrom(maximumVector2, minimumVector2);
    }

    public static RectF Intersection(RectF first, RectF second)
    {
        Intersection(ref first, ref second, out var result);
        return result;
    }

    public RectF Intersection(RectF rectangle)
    {
        Intersection(ref this, ref rectangle, out var result);
        return result;
    }

    public static bool Intersects(ref RectF first, ref RectF second)
    {
        return first.X < second.X + second.Width && first.X + first.Width > second.X &&
               first.Y < second.Y + second.Height && first.Y + first.Height > second.Y;
    }

    public static bool Intersects(RectF first, RectF second)
    {
        return Intersects(ref first, ref second);
    }

    public bool Intersects(RectF rectangle) => RectF.Intersects(ref this, ref rectangle);

    public static bool Contains(ref RectF rectangle, ref Vector2 point)
    {
        return rectangle.X <= point.X && point.X < rectangle.X + rectangle.Width && rectangle.Y <= point.Y &&
               point.Y < rectangle.Y + rectangle.Height;
    }

    public static bool Contains(RectF rectangle, Vector2 point)
    {
        return RectF.Contains(ref rectangle, ref point);
    }

    public bool Contains(Vector2 point) => RectF.Contains(ref this, ref point);

    public float SquaredDistanceTo(Vector2 point)
    {
        return PrimitivesHelper.SquaredDistanceToPointFromRectangle(TopLeft, BottomRight, point);
    }

    public float DistanceTo(Vector2 point)
    {
        return MathF.Sqrt(SquaredDistanceTo(point));
    }

    public Vector2 ClosestPointTo(Vector2 point)
    {
        PrimitivesHelper.ClosestPointToPointFromRectangle(TopLeft, BottomRight, point, out var result);
        return result;
    }

    public void Inflate(float horizontalAmount, float verticalAmount)
    {
        X -= horizontalAmount;
        Y -= verticalAmount;
        Width += horizontalAmount * 2f;
        Height += verticalAmount * 2f;
    }

    public void Offset(float offsetX, float offsetY)
    {
        X += offsetX;
        Y += offsetY;
    }

    public void Offset(Vector2 amount)
    {
        X += amount.X;
        Y += amount.Y;
    }

    public static bool operator ==(RectF first, RectF second) => first.Equals(ref second);

    public static bool operator !=(RectF first, RectF second) => !(first == second);

    public bool Equals(RectF rectangle) => Equals(ref rectangle);

    public bool Equals(ref RectF rectangle)
    {
        return X.EqualsSafe(rectangle.X) && Y.EqualsSafe(rectangle.Y) &&
                Width.EqualsSafe(rectangle.Width) && Height.EqualsSafe(rectangle.Height);
    }
    
    public override bool Equals(object? obj)
    {
        return obj is RectF rectangle && Equals(rectangle);
    }
    
    public override int GetHashCode()
    {
        return ((X.GetHashCode() * 397 ^ Y.GetHashCode()) * 397 ^ Width.GetHashCode()) * 397 ^
               Height.GetHashCode();
    }
    
    public static implicit operator RectF(Rect rectangle)
    {
        return new RectF()
        {
            X = rectangle.X,
            Y = rectangle.Y,
            Width = rectangle.Width,
            Height = rectangle.Height
        };
    }
    
    public static explicit operator Rect(RectF rectangle)
    {
        return new Rect((int)rectangle.X, (int)rectangle.Y, (int)rectangle.Width, (int)rectangle.Height);
    }

    public override string ToString()
    {
        var interpolatedStringHandler = new DefaultInterpolatedStringHandler(28, 4);
        interpolatedStringHandler.AppendLiteral("{X: ");
        interpolatedStringHandler.AppendFormatted(X);
        interpolatedStringHandler.AppendLiteral(", Y: ");
        interpolatedStringHandler.AppendFormatted(Y);
        interpolatedStringHandler.AppendLiteral(", Width: ");
        interpolatedStringHandler.AppendFormatted(Width);
        interpolatedStringHandler.AppendLiteral(", Height: ");
        interpolatedStringHandler.AppendFormatted(Height);
        return interpolatedStringHandler.ToStringAndClear();
    }

    internal string DebugDisplayString => X + "  " + Y + "  " + Width + "  " + Height;
}
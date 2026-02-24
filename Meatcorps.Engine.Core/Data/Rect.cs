using System.Numerics;
using System.Runtime.Serialization;

namespace Meatcorps.Engine.Core.Data;

/// <summary>
/// An integer axis-aligned rectangle defined by position (X, Y) and dimensions (Width, Height).
/// </summary>
[DataContract]
public struct Rect : IEquatable<Rect>
{
    /// <summary>The X coordinate of the left edge.</summary>
    [DataMember] public int X;

    /// <summary>The Y coordinate of the top edge.</summary>
    [DataMember] public int Y;

    /// <summary>The width of the rectangle.</summary>
    [DataMember] public int Width;

    /// <summary>The height of the rectangle.</summary>
    [DataMember] public int Height;

    /// <summary>The top-left position as a <see cref="PointInt"/>.</summary>
    public PointInt Position => new PointInt(X, Y);

    /// <summary>An empty <see cref="Rect"/> at the origin with zero size.</summary>
    public static Rect Empty { get; } = new (0, 0, 0, 0);

    /// <summary>The X coordinate of the left edge.</summary>
    public int Left => X;

    /// <summary>The X coordinate of the right edge (X + Width).</summary>
    public int Right => X + Width;

    /// <summary>The Y coordinate of the top edge.</summary>
    public int Top => Y;

    /// <summary>The Y coordinate of the bottom edge (Y + Height).</summary>
    public int Bottom => Y + Height;

    /// <summary>Returns <see langword="true"/> if all fields are zero.</summary>
    public bool IsEmpty => Width == 0 && Height == 0 && X == 0 && Y == 0;

    /// <summary>Gets or sets the top-left position of the rectangle.</summary>
    public PointInt Location
    {
        get => new PointInt(X, Y);
        set
        {
            X = value.X;
            Y = value.Y;
        }
    }

    /// <summary>Gets or sets the size of the rectangle as a <see cref="PointInt"/>.</summary>
    public PointInt Size
    {
        get => new PointInt(Width, Height);
        set
        {
            Width = value.X;
            Height = value.Y;
        }
    }

    /// <summary>The center point of the rectangle.</summary>
    public PointInt Center => new PointInt(X + Width / 2, Y + Height / 2);

    internal string DebugDisplayString
    {
        get
        {
            return X.ToString() + "  " + Y + "  " + Width + "  " + Height;
        }
    }

    /// <summary>
    /// Initializes a new <see cref="Rect"/> with explicit position and size.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public Rect(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Initializes a new <see cref="Rect"/> from a location and size.
    /// </summary>
    /// <param name="location">The top-left position.</param>
    /// <param name="size">The size (X = width, Y = height).</param>
    public Rect(PointInt location, PointInt size)
    {
        X = location.X;
        Y = location.Y;
        Width = size.X;
        Height = size.Y;
    }

    /// <summary>Returns <see langword="true"/> if two <see cref="Rect"/> values are equal.</summary>
    public static bool operator ==(Rect a, Rect b)
    {
        return a.X == b.X && a.Y == b.Y && a.Width == b.Width && a.Height == b.Height;
    }

    /// <summary>Returns <see langword="true"/> if two <see cref="Rect"/> values are not equal.</summary>
    public static bool operator !=(Rect a, Rect b) => !(a == b);

    /// <summary>Returns <see langword="true"/> if the point (x, y) is inside this rectangle.</summary>
    /// <param name="x">The X coordinate to test.</param>
    /// <param name="y">The Y coordinate to test.</param>
    public bool Contains(int x, int y)
    {
        return X <= x && x < X + Width && Y <= y && y < Y + Height;
    }

    /// <summary>Returns <see langword="true"/> if the float point (x, y) is inside this rectangle.</summary>
    /// <param name="x">The X coordinate to test.</param>
    /// <param name="y">The Y coordinate to test.</param>
    public bool Contains(float x, float y)
    {
        return X <= x && x < (X + Width) &&
               Y <= y && y < (Y + Height);
    }

    /// <summary>Returns <see langword="true"/> if the given <see cref="PointInt"/> is inside this rectangle.</summary>
    /// <param name="value">The point to test.</param>
    public bool Contains(PointInt value)
    {
        return X <= value.X && value.X < X + Width && Y <= value.Y &&
               value.Y < Y + Height;
    }

    /// <summary>Tests whether a <see cref="PointInt"/> is contained in this rectangle, returning the result by <see langword="out"/> parameter.</summary>
    /// <param name="value">The point to test.</param>
    /// <param name="result">Set to <see langword="true"/> if the point is inside.</param>
    public void Contains(ref PointInt value, out bool result)
    {
        result = X <= value.X && value.X < X + Width && Y <= value.Y &&
                 value.Y < Y + Height;
    }

    /// <summary>Returns <see langword="true"/> if the given <see cref="Vector2"/> is inside this rectangle.</summary>
    /// <param name="value">The point to test.</param>
    public bool Contains(Vector2 value)
    {
        return X <= value.X && value.X < (X + Width) &&
               Y <= value.Y && value.Y < (Y + Height);
    }

    /// <summary>Tests whether a <see cref="Vector2"/> is contained in this rectangle, returning the result by <see langword="out"/> parameter.</summary>
    /// <param name="value">The point to test.</param>
    /// <param name="result">Set to <see langword="true"/> if the point is inside.</param>
    public void Contains(ref Vector2 value, out bool result)
    {
        result = X <= value.X && value.X < (X + Width) &&
                 Y <= value.Y && value.Y < (Y + Height);
    }

    /// <summary>Returns <see langword="true"/> if the given <see cref="Rect"/> is entirely contained within this rectangle.</summary>
    /// <param name="value">The rectangle to test.</param>
    public bool Contains(Rect value)
    {
        return X <= value.X && value.X + value.Width <= X + Width && Y <= value.Y &&
               value.Y + value.Height <= Y + Height;
    }

    /// <summary>Tests whether a <see cref="Rect"/> is entirely contained within this rectangle, returning the result by <see langword="out"/> parameter.</summary>
    /// <param name="value">The rectangle to test.</param>
    /// <param name="result">Set to <see langword="true"/> if <paramref name="value"/> is fully inside.</param>
    public void Contains(ref Rect value, out bool result)
    {
        result = X <= value.X && value.X + value.Width <= X + Width && Y <= value.Y &&
                 value.Y + value.Height <= Y + Height;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Rect rectangle && this == rectangle;

    /// <summary>Returns <see langword="true"/> if this rectangle equals another <see cref="Rect"/>.</summary>
    /// <param name="other">The rectangle to compare to.</param>
    public bool Equals(Rect other) => this == other;

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return (((17 * 23 + X.GetHashCode()) * 23 + Y.GetHashCode()) * 23 + Width.GetHashCode()) * 23 +
               Height.GetHashCode();
    }

    /// <summary>Expands this rectangle outward by the given amounts.</summary>
    /// <param name="horizontalAmount">Pixels to expand left and right.</param>
    /// <param name="verticalAmount">Pixels to expand up and down.</param>
    public void Inflate(int horizontalAmount, int verticalAmount)
    {
        X -= horizontalAmount;
        Y -= verticalAmount;
        Width += horizontalAmount * 2;
        Height += verticalAmount * 2;
    }

    /// <summary>Expands this rectangle outward by the given float amounts (truncated to int).</summary>
    /// <param name="horizontalAmount">Pixels to expand left and right.</param>
    /// <param name="verticalAmount">Pixels to expand up and down.</param>
    public void Inflate(float horizontalAmount, float verticalAmount)
    {
        X -= (int)horizontalAmount;
        Y -= (int)verticalAmount;
        Width += (int)horizontalAmount * 2;
        Height += (int)verticalAmount * 2;
    }

    /// <summary>Returns <see langword="true"/> if this rectangle overlaps with <paramref name="value"/>.</summary>
    /// <param name="value">The rectangle to test against.</param>
    public bool Intersects(Rect value)
    {
        return value.Left < Right && Left < value.Right && value.Top < Bottom && Top < value.Bottom;
    }

    /// <summary>Tests whether this rectangle intersects another, returning the result by <see langword="out"/> parameter.</summary>
    /// <param name="value">The rectangle to test against.</param>
    /// <param name="result">Set to <see langword="true"/> if the rectangles overlap.</param>
    public void Intersects(ref Rect value, out bool result)
    {
        result = value.Left < Right && Left < value.Right && value.Top < Bottom &&
                 Top < value.Bottom;
    }

    /// <summary>Returns the intersection of two rectangles, or an empty rectangle if they do not overlap.</summary>
    /// <param name="value1">The first rectangle.</param>
    /// <param name="value2">The second rectangle.</param>
    public static Rect Intersect(Rect value1, Rect value2)
    {
        Intersect(ref value1, ref value2, out var result);
        return result;
    }

    /// <summary>Computes the intersection of two rectangles by reference, or an empty rectangle if they do not overlap.</summary>
    /// <param name="value1">The first rectangle.</param>
    /// <param name="value2">The second rectangle.</param>
    /// <param name="result">The intersecting region.</param>
    public static void Intersect(ref Rect value1, ref Rect value2, out Rect result)
    {
        if (value1.Intersects(value2))
        {
            int num1 = Math.Min(value1.X + value1.Width, value2.X + value2.Width);
            int x = Math.Max(value1.X, value2.X);
            int y = Math.Max(value1.Y, value2.Y);
            int num2 = Math.Min(value1.Y + value1.Height, value2.Y + value2.Height);
            result = new Rect(x, y, num1 - x, num2 - y);
        }
        else
            result = new Rect(0, 0, 0, 0);
    }

    /// <summary>Clamps a <see cref="PointInt"/> so it lies within this rectangle's bounds.</summary>
    /// <param name="point">The point to clamp.</param>
    /// <returns>The clamped point.</returns>
    public PointInt Clamp(PointInt point)
    {
        var clampedX = Math.Clamp(point.X, X, Right - 1);
        var clampedY = Math.Clamp(point.Y, Y, Bottom - 1);
        return new PointInt(clampedX, clampedY);
    }

    /// <summary>Moves this rectangle by the given offset.</summary>
    /// <param name="offsetX">Horizontal offset.</param>
    /// <param name="offsetY">Vertical offset.</param>
    public void Offset(int offsetX, int offsetY)
    {
        X += offsetX;
        Y += offsetY;
    }

    /// <summary>Moves this rectangle by the given float offset (truncated to int).</summary>
    /// <param name="offsetX">Horizontal offset.</param>
    /// <param name="offsetY">Vertical offset.</param>
    public void Offset(float offsetX, float offsetY)
    {
        X += (int)offsetX;
        Y += (int)offsetY;
    }

    /// <summary>Moves this rectangle by a <see cref="PointInt"/> amount.</summary>
    /// <param name="amount">The offset to apply.</param>
    public void Offset(PointInt amount)
    {
        X += amount.X;
        Y += amount.Y;
    }

    /// <summary>Moves this rectangle by a <see cref="Vector2"/> amount (truncated to int).</summary>
    /// <param name="amount">The offset to apply.</param>
    public void Offset(Vector2 amount)
    {
        X += (int)amount.X;
        Y += (int)amount.Y;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return "{X:" + X.ToString() + " Y:" + Y.ToString() + " Width:" + Width.ToString() + " Height:" +
               Height.ToString() + "}";
    }

    /// <summary>Returns the smallest <see cref="Rect"/> that contains both input rectangles.</summary>
    /// <param name="value1">The first rectangle.</param>
    /// <param name="value2">The second rectangle.</param>
    public static Rect Union(Rect value1, Rect value2)
    {
        var x = Math.Min(value1.X, value2.X);
        var y = Math.Min(value1.Y, value2.Y);
        return new Rect(x, y, Math.Max(value1.Right, value2.Right) - x, Math.Max(value1.Bottom, value2.Bottom) - y);
    }

    /// <summary>Computes the union of two rectangles by reference.</summary>
    /// <param name="value1">The first rectangle.</param>
    /// <param name="value2">The second rectangle.</param>
    /// <param name="result">The bounding rectangle containing both inputs.</param>
    public static void Union(ref Rect value1, ref Rect value2, out Rect result)
    {
        result.X = Math.Min(value1.X, value2.X);
        result.Y = Math.Min(value1.Y, value2.Y);
        result.Width = Math.Max(value1.Right, value2.Right) - result.X;
        result.Height = Math.Max(value1.Bottom, value2.Bottom) - result.Y;
    }

    /// <summary>Implicitly converts a <see cref="Rect"/> to a <see cref="RectF"/>.</summary>
    public static implicit operator RectF(Rect r) => new(new Vector2(r.X, r.Y), new SizeF(r.Width, r.Height));

    /// <summary>Deconstructs the rectangle into its components.</summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public void Deconstruct(out int x, out int y, out int width, out int height)
    {
        x = X;
        y = Y;
        width = Width;
        height = Height;
    }
}
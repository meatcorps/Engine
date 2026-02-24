using System.Globalization;
using Meatcorps.Engine.Core.Extensions;

namespace Meatcorps.Engine.Core.Data;

/// <summary>
/// Represents outer spacing (margin) on all four sides of a rectangle, using floating-point values.
/// All side values are clamped to a minimum of zero.
/// </summary>
public struct MarginF
{
    private float _left, _top, _right, _bottom;

    /// <summary>The left margin. Clamped to zero or greater.</summary>
    public float Left { get => _left; set => _left = Math.Max(0, value); }

    /// <summary>The top margin. Clamped to zero or greater.</summary>
    public float Top { get => _top; set => _top = Math.Max(0, value); }

    /// <summary>The right margin. Clamped to zero or greater.</summary>
    public float Right { get => _right; set => _right = Math.Max(0, value); }

    /// <summary>The bottom margin. Clamped to zero or greater.</summary>
    public float Bottom { get => _bottom; set => _bottom = Math.Max(0, value); }

    /// <summary>Initializes a new <see cref="MarginF"/> with all sides set to zero.</summary>
    public MarginF()
    {
        Left = Top = Right = Bottom = 0;
    }

    /// <summary>Initializes a new <see cref="MarginF"/> with all sides set to the same value.</summary>
    /// <param name="all">The margin value for all sides.</param>
    public MarginF(float all)
    {
        Left = Top = Right = Bottom = all;
    }

    /// <summary>Initializes a new <see cref="MarginF"/> with separate horizontal and vertical values.</summary>
    /// <param name="horizontal">The margin for the left and right sides.</param>
    /// <param name="vertical">The margin for the top and bottom sides.</param>
    public MarginF(float horizontal, float vertical)
    {
        Left = Right = horizontal;
        Top = Bottom = vertical;
    }

    /// <summary>Initializes a new <see cref="MarginF"/> with explicit values for each side.</summary>
    /// <param name="left">The left margin.</param>
    /// <param name="top">The top margin.</param>
    /// <param name="right">The right margin.</param>
    /// <param name="bottom">The bottom margin.</param>
    public MarginF(float left, float top, float right, float bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    /// <summary>Creates a <see cref="MarginF"/> with all sides set to <paramref name="all"/>.</summary>
    /// <param name="all">The margin value for all sides.</param>
    public static MarginF All(float all) => new(all);

    /// <summary>Creates a <see cref="MarginF"/> with equal left and right margin and zero vertical margin.</summary>
    /// <param name="horizontal">The margin for the left and right sides.</param>
    public static MarginF Horizontal(float horizontal) => new(horizontal, 0);

    /// <summary>Creates a <see cref="MarginF"/> with equal top and bottom margin and zero horizontal margin.</summary>
    /// <param name="vertical">The margin for the top and bottom sides.</param>
    public static MarginF Vertical(float vertical) => new(0, vertical);

    /// <summary>Creates a <see cref="MarginF"/> with separate horizontal and vertical values.</summary>
    /// <param name="horizontal">The margin for the left and right sides.</param>
    /// <param name="vertical">The margin for the top and bottom sides.</param>
    public static MarginF HorizontalVertical(float horizontal, float vertical) => new(horizontal, vertical);

    /// <summary>A <see cref="MarginF"/> with all sides set to zero.</summary>
    public static MarginF Zero => new();

    /// <summary>Adds two <see cref="MarginF"/> values component-wise.</summary>
    public static MarginF operator +(MarginF value1, MarginF value2)
    {
        return new MarginF(value1.Left + value2.Left, value1.Top + value2.Top, value1.Right + value2.Right, value1.Bottom + value2.Bottom);
    }

    /// <summary>Subtracts one <see cref="MarginF"/> from another component-wise.</summary>
    public static MarginF operator -(MarginF value1, MarginF value2)
    {
        return new MarginF(value1.Left - value2.Left, value1.Top - value2.Top, value1.Right - value2.Right, value1.Bottom - value2.Bottom);
    }

    /// <summary>Multiplies two <see cref="MarginF"/> values component-wise.</summary>
    public static MarginF operator *(MarginF value1, MarginF value2)
    {
        return new MarginF(value1.Left * value2.Left, value1.Top * value2.Top, value1.Right * value2.Right, value1.Bottom * value2.Bottom);
    }

    /// <summary>Divides a <see cref="MarginF"/> by another component-wise.</summary>
    public static MarginF operator /(MarginF value1, MarginF value2)
    {
        return new MarginF(value1.Left / value2.Left, value1.Top / value2.Top, value1.Right / value2.Right, value1.Bottom / value2.Bottom);
    }

    /// <summary>Adds a scalar float to all sides of a <see cref="MarginF"/>.</summary>
    public static MarginF operator +(MarginF value1, float value2)
    {
        return new MarginF(value1.Left + value2, value1.Top + value2, value1.Right + value2, value1.Bottom + value2);
    }

    /// <summary>
    /// Expands a <see cref="RectF"/> outward by the margin amounts.
    /// </summary>
    public static RectF operator +(RectF value1, MarginF value2)
    {
        return new RectF(value1.Left - value2.Left, value1.Top - value2.Top, value1.Width + value2._left - value2.Right, value1.Height + value2.Top - value2.Bottom);
    }

    /// <summary>Subtracts a scalar integer from all sides of a <see cref="MarginF"/>.</summary>
    public static MarginF operator -(MarginF value1, int value2)
    {
        return new MarginF(value1.Left - value2, value1.Top - value2, value1.Right - value2, value1.Bottom - value2);
    }

    /// <summary>Multiplies all sides of a <see cref="MarginF"/> by a scalar integer.</summary>
    public static MarginF operator *(MarginF value1, int value2)
    {
        return new MarginF(value1.Left * value2, value1.Top * value2, value1.Right * value2, value1.Bottom * value2);
    }

    /// <summary>Divides all sides of a <see cref="MarginF"/> by a scalar integer.</summary>
    public static MarginF operator /(MarginF value1, int value2)
    {
        return new MarginF(value1.Left / value2, value1.Top / value2, value1.Right / value2, value1.Bottom / value2);
    }

    /// <summary>Returns <see langword="true"/> if two <see cref="MarginF"/> values are equal.</summary>
    public static bool operator ==(MarginF a, MarginF b) => a.Equals(b);

    /// <summary>Returns <see langword="true"/> if two <see cref="MarginF"/> values are not equal.</summary>
    public static bool operator !=(MarginF a, MarginF b) => !a.Equals(b);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is MarginF other && Equals(other);

    /// <summary>Returns <see langword="true"/> if this margin equals another <see cref="MarginF"/>.</summary>
    /// <param name="other">The margin to compare to.</param>
    public bool Equals(MarginF other) => Left.EqualsSafe(other.Left) && Top.EqualsSafe(other.Top) && Right.EqualsSafe(other.Right) && Bottom.EqualsSafe(other.Bottom);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(_left, _top, _right, _bottom);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return "Padding: {L:" + _left.ToString(CultureInfo.InvariantCulture) + " R:" + _right.ToString(CultureInfo.InvariantCulture)  + " T:" + _top.ToString(CultureInfo.InvariantCulture)  + " B:" + _bottom.ToString(CultureInfo.InvariantCulture)  + "}";
    }
}
using Meatcorps.Engine.Core.Extensions;

namespace Meatcorps.Engine.Core.Data;

/// <summary>
/// Represents inner spacing (padding) on all four sides of a rectangle, using floating-point values.
/// All side values are clamped to a minimum of zero.
/// </summary>
public struct PaddingF
{
    private float _left, _top, _right, _bottom;

    /// <summary>The left padding. Clamped to zero or greater.</summary>
    public float Left { get => _left; set => _left = Math.Max(0, value); }

    /// <summary>The top padding. Clamped to zero or greater.</summary>
    public float Top { get => _top; set => _top = Math.Max(0, value); }

    /// <summary>The right padding. Clamped to zero or greater.</summary>
    public float Right { get => _right; set => _right = Math.Max(0, value); }

    /// <summary>The bottom padding. Clamped to zero or greater.</summary>
    public float Bottom { get => _bottom; set => _bottom = Math.Max(0, value); }

    /// <summary>Initializes a new <see cref="PaddingF"/> with all sides set to zero.</summary>
    public PaddingF()
    {
        Left = Top = Right = Bottom = 0;
    }

    /// <summary>Initializes a new <see cref="PaddingF"/> with all sides set to the same value.</summary>
    /// <param name="all">The padding value for all sides.</param>
    public PaddingF(float all)
    {
        Left = Top = Right = Bottom = all;
    }

    /// <summary>Initializes a new <see cref="PaddingF"/> with separate horizontal and vertical values.</summary>
    /// <param name="horizontal">The padding for the left and right sides.</param>
    /// <param name="vertical">The padding for the top and bottom sides.</param>
    public PaddingF(float horizontal, float vertical)
    {
        Left = Right = horizontal;
        Top = Bottom = vertical;
    }

    /// <summary>Initializes a new <see cref="PaddingF"/> with explicit values for each side.</summary>
    /// <param name="left">The left padding.</param>
    /// <param name="top">The top padding.</param>
    /// <param name="right">The right padding.</param>
    /// <param name="bottom">The bottom padding.</param>
    public PaddingF(float left, float top, float right, float bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    /// <summary>Creates a <see cref="PaddingF"/> with all sides set to <paramref name="all"/>.</summary>
    /// <param name="all">The padding value for all sides.</param>
    public static PaddingF All(float all) => new(all);

    /// <summary>Creates a <see cref="PaddingF"/> with equal left and right padding and zero vertical padding.</summary>
    /// <param name="horizontal">The padding for the left and right sides.</param>
    public static PaddingF Horizontal(float horizontal) => new(horizontal, 0);

    /// <summary>Creates a <see cref="PaddingF"/> with equal top and bottom padding and zero horizontal padding.</summary>
    /// <param name="vertical">The padding for the top and bottom sides.</param>
    public static PaddingF Vertical(float vertical) => new(0, vertical);

    /// <summary>Creates a <see cref="PaddingF"/> with separate horizontal and vertical values.</summary>
    /// <param name="horizontal">The padding for the left and right sides.</param>
    /// <param name="vertical">The padding for the top and bottom sides.</param>
    public static PaddingF HorizontalVertical(float horizontal, float vertical) => new(horizontal, vertical);

    /// <summary>A <see cref="PaddingF"/> with all sides set to zero.</summary>
    public static PaddingF Zero => new();

    /// <summary>Adds two <see cref="PaddingF"/> values component-wise.</summary>
    public static PaddingF operator +(PaddingF value1, PaddingF value2)
    {
        return new PaddingF(value1.Left + value2.Left, value1.Top + value2.Top, value1.Right + value2.Right, value1.Bottom + value2.Bottom);
    }

    /// <summary>Subtracts one <see cref="PaddingF"/> from another component-wise.</summary>
    public static PaddingF operator -(PaddingF value1, PaddingF value2)
    {
        return new PaddingF(value1.Left - value2.Left, value1.Top - value2.Top, value1.Right - value2.Right, value1.Bottom - value2.Bottom);
    }

    /// <summary>Multiplies two <see cref="PaddingF"/> values component-wise.</summary>
    public static PaddingF operator *(PaddingF value1, PaddingF value2)
    {
        return new PaddingF(value1.Left * value2.Left, value1.Top * value2.Top, value1.Right * value2.Right, value1.Bottom * value2.Bottom);
    }

    /// <summary>Divides a <see cref="PaddingF"/> by another component-wise.</summary>
    public static PaddingF operator /(PaddingF value1, PaddingF value2)
    {
        return new PaddingF(value1.Left / value2.Left, value1.Top / value2.Top, value1.Right / value2.Right, value1.Bottom / value2.Bottom);
    }

    /// <summary>Adds a scalar float to all sides of a <see cref="PaddingF"/>.</summary>
    public static PaddingF operator +(PaddingF value1, float value2)
    {
        return new PaddingF(value1.Left + value2, value1.Top + value2, value1.Right + value2, value1.Bottom + value2);
    }

    /// <summary>
    /// Applies padding to a <see cref="RectF"/>, shrinking it inward by the padding amounts.
    /// </summary>
    public static RectF operator +(RectF value1, PaddingF value2)
    {
        return new RectF(value1.Left + value2.Left, value1.Top + value2.Top, Math.Max(0, value1.Width - value2._left - value2.Right), Math.Max(0, value1.Height - value2.Top - value2.Bottom));
    }

    /// <summary>Subtracts a scalar integer from all sides of a <see cref="PaddingF"/>.</summary>
    public static PaddingF operator -(PaddingF value1, int value2)
    {
        return new PaddingF(value1.Left - value2, value1.Top - value2, value1.Right - value2, value1.Bottom - value2);
    }

    /// <summary>Multiplies all sides of a <see cref="PaddingF"/> by a scalar integer.</summary>
    public static PaddingF operator *(PaddingF value1, int value2)
    {
        return new PaddingF(value1.Left * value2, value1.Top * value2, value1.Right * value2, value1.Bottom * value2);
    }

    /// <summary>Divides all sides of a <see cref="PaddingF"/> by a scalar integer.</summary>
    public static PaddingF operator /(PaddingF value1, int value2)
    {
        return new PaddingF(value1.Left / value2, value1.Top / value2, value1.Right / value2, value1.Bottom / value2);
    }

    /// <summary>Returns <see langword="true"/> if two <see cref="PaddingF"/> values are equal.</summary>
    public static bool operator ==(PaddingF a, PaddingF b) => a.Equals(b);

    /// <summary>Returns <see langword="true"/> if two <see cref="PaddingF"/> values are not equal.</summary>
    public static bool operator !=(PaddingF a, PaddingF b) => !a.Equals(b);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is PaddingF other && Equals(other);

    /// <summary>Returns <see langword="true"/> if this padding equals another <see cref="PaddingF"/>.</summary>
    /// <param name="other">The padding to compare to.</param>
    public bool Equals(PaddingF other) => Left.EqualsSafe(other.Left) && Top.EqualsSafe(other.Top) && Right.EqualsSafe(other.Right) && Bottom.EqualsSafe(other.Bottom);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(_left, _top, _right, _bottom);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return "Padding: {L:" + _left + " R:" + _right + " T:" + _top + " B:" + _bottom + "}";
    }
}
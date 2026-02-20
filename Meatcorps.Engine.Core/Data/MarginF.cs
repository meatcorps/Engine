using System.Globalization;
using Meatcorps.Engine.Core.Extensions;

namespace Meatcorps.Engine.Core.Data;

public struct MarginF
{
    private float _left, _top, _right, _bottom;
    public float Left { get => _left; set => _left = Math.Max(0, value); }
    public float Top { get => _top; set => _top = Math.Max(0, value); }
    public float Right { get => _right; set => _right = Math.Max(0, value); }
    public float Bottom { get => _bottom; set => _bottom = Math.Max(0, value); }

    public MarginF()
    {
        Left = Top = Right = Bottom = 0;    
    }
    
    public MarginF(float all)
    {
        Left = Top = Right = Bottom = all;    
    }

    public MarginF(float horizontal, float vertical)
    {
        Left = Right = horizontal;
        Top = Bottom = vertical;   
    }
    
    public MarginF(float left, float top, float right, float bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;    
    }
    
    public static MarginF All(float all) => new(all);
    
    public static MarginF Horizontal(float horizontal) => new(horizontal, 0);
    public static MarginF Vertical(float vertical) => new(0, vertical);
    public static MarginF HorizontalVertical(float horizontal, float vertical) => new(horizontal, vertical);
    
    public static MarginF Zero => new();
    
    public static MarginF operator +(MarginF value1, MarginF value2)
    {
        return new MarginF(value1.Left + value2.Left, value1.Top + value2.Top, value1.Right + value2.Right, value1.Bottom + value2.Bottom);
    }

    public static MarginF operator -(MarginF value1, MarginF value2)
    {
        return new MarginF(value1.Left - value2.Left, value1.Top - value2.Top, value1.Right - value2.Right, value1.Bottom - value2.Bottom);
    }

    public static MarginF operator *(MarginF value1, MarginF value2)
    {
        return new MarginF(value1.Left * value2.Left, value1.Top * value2.Top, value1.Right * value2.Right, value1.Bottom * value2.Bottom);
    }

    public static MarginF operator /(MarginF value1, MarginF value2)
    {
        return new MarginF(value1.Left / value2.Left, value1.Top / value2.Top, value1.Right / value2.Right, value1.Bottom / value2.Bottom);
    }
    
    public static MarginF operator +(MarginF value1, float value2)
    {
        return new MarginF(value1.Left + value2, value1.Top + value2, value1.Right + value2, value1.Bottom + value2);
    }
    
    public static RectF operator +(RectF value1, MarginF value2)
    {
        return new RectF(value1.Left - value2.Left, value1.Top - value2.Top, value1.Width + value2._left - value2.Right, value1.Height + value2.Top - value2.Bottom);   
    }

    public static MarginF operator -(MarginF value1, int value2)
    {
        return new MarginF(value1.Left - value2, value1.Top - value2, value1.Right - value2, value1.Bottom - value2);
    }

    public static MarginF operator *(MarginF value1, int value2)
    {
        return new MarginF(value1.Left * value2, value1.Top * value2, value1.Right * value2, value1.Bottom * value2);
    }

    public static MarginF operator /(MarginF value1, int value2)
    {
        return new MarginF(value1.Left / value2, value1.Top / value2, value1.Right / value2, value1.Bottom / value2);
    }

    public static bool operator ==(MarginF a, MarginF b) => a.Equals(b);

    public static bool operator !=(MarginF a, MarginF b) => !a.Equals(b);
    
    public override bool Equals(object? obj) => obj is MarginF other && Equals(other);

    public bool Equals(MarginF other) => Left.EqualsSafe(other.Left) && Top.EqualsSafe(other.Top) && Right.EqualsSafe(other.Right) && Bottom.EqualsSafe(other.Bottom);

    public override int GetHashCode()
    {
        return HashCode.Combine(_left, _top, _right, _bottom);
    }

    public override string ToString()
    {
        return "Padding: {L:" + _left.ToString(CultureInfo.InvariantCulture) + " R:" + _right.ToString(CultureInfo.InvariantCulture)  + " T:" + _top.ToString(CultureInfo.InvariantCulture)  + " B:" + _bottom.ToString(CultureInfo.InvariantCulture)  + "}";
    }
}
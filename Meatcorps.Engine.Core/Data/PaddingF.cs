using Meatcorps.Engine.Core.Extensions;

namespace Meatcorps.Engine.Core.Data;

public struct PaddingF
{
    private float _left, _top, _right, _bottom;
    public float Left { get => _left; set => _left = Math.Max(0, value); }
    public float Top { get => _top; set => _top = Math.Max(0, value); }
    public float Right { get => _right; set => _right = Math.Max(0, value); }
    public float Bottom { get => _bottom; set => _bottom = Math.Max(0, value); }

    public PaddingF()
    {
        Left = Top = Right = Bottom = 0;    
    }
    
    public PaddingF(float all)
    {
        Left = Top = Right = Bottom = all;    
    }

    public PaddingF(float horizontal, float vertical)
    {
        Left = Right = horizontal;
        Top = Bottom = vertical;   
    }
    
    public PaddingF(float left, float top, float right, float bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;    
    }
    
    public static PaddingF All(float all) => new(all);
    
    public static PaddingF Horizontal(float horizontal) => new(horizontal, 0);
    public static PaddingF Vertical(float vertical) => new(0, vertical);
    public static PaddingF HorizontalVertical(float horizontal, float vertical) => new(horizontal, vertical);
    
    public static PaddingF Zero => new();
    
    public static PaddingF operator +(PaddingF value1, PaddingF value2)
    {
        return new PaddingF(value1.Left + value2.Left, value1.Top + value2.Top, value1.Right + value2.Right, value1.Bottom + value2.Bottom);
    }

    public static PaddingF operator -(PaddingF value1, PaddingF value2)
    {
        return new PaddingF(value1.Left - value2.Left, value1.Top - value2.Top, value1.Right - value2.Right, value1.Bottom - value2.Bottom);
    }

    public static PaddingF operator *(PaddingF value1, PaddingF value2)
    {
        return new PaddingF(value1.Left * value2.Left, value1.Top * value2.Top, value1.Right * value2.Right, value1.Bottom * value2.Bottom);
    }

    public static PaddingF operator /(PaddingF value1, PaddingF value2)
    {
        return new PaddingF(value1.Left / value2.Left, value1.Top / value2.Top, value1.Right / value2.Right, value1.Bottom / value2.Bottom);
    }
    
    public static PaddingF operator +(PaddingF value1, float value2)
    {
        return new PaddingF(value1.Left + value2, value1.Top + value2, value1.Right + value2, value1.Bottom + value2);
    }
    
    public static RectF operator +(RectF value1, PaddingF value2)
    {
        return new RectF(value1.Left + value2.Left, value1.Top + value2.Top, Math.Max(0, value1.Width - value2._left - value2.Right), Math.Max(0, value1.Height - value2.Top - value2.Bottom));   
    }

    public static PaddingF operator -(PaddingF value1, int value2)
    {
        return new PaddingF(value1.Left - value2, value1.Top - value2, value1.Right - value2, value1.Bottom - value2);
    }

    public static PaddingF operator *(PaddingF value1, int value2)
    {
        return new PaddingF(value1.Left * value2, value1.Top * value2, value1.Right * value2, value1.Bottom * value2);
    }

    public static PaddingF operator /(PaddingF value1, int value2)
    {
        return new PaddingF(value1.Left / value2, value1.Top / value2, value1.Right / value2, value1.Bottom / value2);
    }

    public static bool operator ==(PaddingF a, PaddingF b) => a.Equals(b);

    public static bool operator !=(PaddingF a, PaddingF b) => !a.Equals(b);
    
    public override bool Equals(object? obj) => obj is PaddingF other && Equals(other);

    public bool Equals(PaddingF other) => Left.EqualsSafe(other.Left) && Top.EqualsSafe(other.Top) && Right.EqualsSafe(other.Right) && Bottom.EqualsSafe(other.Bottom);

    public override int GetHashCode()
    {
        return HashCode.Combine(_left, _top, _right, _bottom);
    }

    public override string ToString()
    {
        return "Padding: {L:" + _left + " R:" + _right + " T:" + _top + " B:" + _bottom + "}";
    }
}
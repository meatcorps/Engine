using System.Numerics;
using System.Runtime.Serialization;

namespace Meatcorps.Engine.Core.Data;

[DataContract]
public struct Rect : IEquatable<Rect>
{
    [DataMember] public int X;

    [DataMember] public int Y;

    [DataMember] public int Width;

    [DataMember] public int Height;
    
    public PointInt Position => new PointInt(X, Y);

    public static Rect Empty { get; } = new (0, 0, 0, 0);

    public int Left => X;

    public int Right => X + Width;

    public int Top => Y;

    public int Bottom => Y + Height;

    public bool IsEmpty => Width == 0 && Height == 0 && X == 0 && Y == 0;

    public PointInt Location
    {
        get => new PointInt(X, Y);
        set
        {
            X = value.X;
            Y = value.Y;
        }
    }

    public PointInt Size
    {
        get => new PointInt(Width, Height);
        set
        {
            Width = value.X;
            Height = value.Y;
        }
    }

    public PointInt Center => new PointInt(X + Width / 2, Y + Height / 2);

    internal string DebugDisplayString
    {
        get
        {
            return X.ToString() + "  " + Y + "  " + Width + "  " + Height;
        }
    }

    public Rect(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public Rect(PointInt location, PointInt size)
    {
        X = location.X;
        Y = location.Y;
        Width = size.X;
        Height = size.Y;
    }

    public static bool operator ==(Rect a, Rect b)
    {
        return a.X == b.X && a.Y == b.Y && a.Width == b.Width && a.Height == b.Height;
    }

    public static bool operator !=(Rect a, Rect b) => !(a == b);

    public bool Contains(int x, int y)
    {
        return X <= x && x < X + Width && Y <= y && y < Y + Height;
    }

    public bool Contains(float x, float y)
    {
        return X <= x && x < (X + Width) &&
               Y <= y && y < (Y + Height);
    }

    public bool Contains(PointInt value)
    {
        return X <= value.X && value.X < X + Width && Y <= value.Y &&
               value.Y < Y + Height;
    }

    public void Contains(ref PointInt value, out bool result)
    {
        result = X <= value.X && value.X < X + Width && Y <= value.Y &&
                 value.Y < Y + Height;
    }

    public bool Contains(Vector2 value)
    {
        return X <= value.X && value.X < (X + Width) &&
               Y <= value.Y && value.Y < (Y + Height);
    }

    public void Contains(ref Vector2 value, out bool result)
    {
        result = X <= value.X && value.X < (X + Width) &&
                 Y <= value.Y && value.Y < (Y + Height);
    }

    public bool Contains(Rect value)
    {
        return X <= value.X && value.X + value.Width <= X + Width && Y <= value.Y &&
               value.Y + value.Height <= Y + Height;
    }

    public void Contains(ref Rect value, out bool result)
    {
        result = X <= value.X && value.X + value.Width <= X + Width && Y <= value.Y &&
                 value.Y + value.Height <= Y + Height;
    }

    public override bool Equals(object? obj) => obj is Rect rectangle && this == rectangle;

    public bool Equals(Rect other) => this == other;

    public override int GetHashCode()
    {
        return (((17 * 23 + X.GetHashCode()) * 23 + Y.GetHashCode()) * 23 + Width.GetHashCode()) * 23 +
               Height.GetHashCode();
    }

    public void Inflate(int horizontalAmount, int verticalAmount)
    {
        X -= horizontalAmount;
        Y -= verticalAmount;
        Width += horizontalAmount * 2;
        Height += verticalAmount * 2;
    }

    public void Inflate(float horizontalAmount, float verticalAmount)
    {
        X -= (int)horizontalAmount;
        Y -= (int)verticalAmount;
        Width += (int)horizontalAmount * 2;
        Height += (int)verticalAmount * 2;
    }

    public bool Intersects(Rect value)
    {
        return value.Left < Right && Left < value.Right && value.Top < Bottom && Top < value.Bottom;
    }

    public void Intersects(ref Rect value, out bool result)
    {
        result = value.Left < Right && Left < value.Right && value.Top < Bottom &&
                 Top < value.Bottom;
    }
    
    public static Rect Intersect(Rect value1, Rect value2)
    {
        Rect.Intersect(ref value1, ref value2, out var result);
        return result;
    }

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
    
    
    public PointInt Clamp(PointInt point)
    {
        var clampedX = Math.Clamp(point.X, X, Right - 1);
        var clampedY = Math.Clamp(point.Y, Y, Bottom - 1);
        return new PointInt(clampedX, clampedY);
    }

    public void Offset(int offsetX, int offsetY)
    {
        X += offsetX;
        Y += offsetY;
    }

    public void Offset(float offsetX, float offsetY)
    {
        X += (int)offsetX;
        Y += (int)offsetY;
    }

    public void Offset(PointInt amount)
    {
        X += amount.X;
        Y += amount.Y;
    }

    public void Offset(Vector2 amount)
    {
        X += (int)amount.X;
        Y += (int)amount.Y;
    }

    public override string ToString()
    {
        return "{X:" + X.ToString() + " Y:" + Y.ToString() + " Width:" + Width.ToString() + " Height:" +
               Height.ToString() + "}";
    }
    
    public static Rect Union(Rect value1, Rect value2)
    {
        var x = Math.Min(value1.X, value2.X);
        var y = Math.Min(value1.Y, value2.Y);
        return new Rect(x, y, Math.Max(value1.Right, value2.Right) - x, Math.Max(value1.Bottom, value2.Bottom) - y);
    }
    
    public static void Union(ref Rect value1, ref Rect value2, out Rect result)
    {
        result.X = Math.Min(value1.X, value2.X);
        result.Y = Math.Min(value1.Y, value2.Y);
        result.Width = Math.Max(value1.Right, value2.Right) - result.X;
        result.Height = Math.Max(value1.Bottom, value2.Bottom) - result.Y;
    }
    
    public static implicit operator RectF(Rect r) => new(new Vector2(r.X, r.Y), new SizeF(r.Width, r.Height));
    
    public void Deconstruct(out int x, out int y, out int width, out int height)
    {
        x = X;
        y = Y;
        width = Width;
        height = Height;
    }
}
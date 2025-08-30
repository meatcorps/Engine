using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Meatcorps.Engine.Core.Data;

[DataContract]
public struct PointInt : IEquatable<PointInt>
{
    private static readonly PointInt ZeroPointInt = new();

    [DataMember] public int X;

    [DataMember] public int Y;

    public static PointInt Zero => ZeroPointInt;

    public PointInt(int x, int y)
    {
        X = x;
        Y = y;
    }

    public PointInt(int value)
    {
        X = value;
        Y = value;
    }

    public static PointInt operator +(PointInt value1, PointInt value2)
    {
        return new PointInt(value1.X + value2.X, value1.Y + value2.Y);
    }

    public static PointInt operator -(PointInt value1, PointInt value2)
    {
        return new PointInt(value1.X - value2.X, value1.Y - value2.Y);
    }

    public static PointInt operator *(PointInt value1, PointInt value2)
    {
        return new PointInt(value1.X * value2.X, value1.Y * value2.Y);
    }

    public static PointInt operator /(PointInt source, PointInt divisor)
    {
        return new PointInt(source.X / divisor.X, source.Y / divisor.Y);
    }
    
    public static PointInt operator +(PointInt value1, int value2)
    {
        return new PointInt(value1.X + value2, value1.Y + value2);
    }

    public static PointInt operator -(PointInt value1, int value2)
    {
        return new PointInt(value1.X - value2, value1.Y - value2);
    }

    public static PointInt operator *(PointInt value1, int value2)
    {
        return new PointInt(value1.X * value2, value1.Y * value2);
    }

    public static PointInt operator /(PointInt source, int divisor)
    {
        return new PointInt(source.X / divisor, source.Y / divisor);
    }
    
    public static PointInt operator -(PointInt value)
    {
        return new PointInt(-value.X, -value.Y);
    }

    public static bool operator ==(PointInt a, PointInt b) => a.Equals(b);

    public static bool operator !=(PointInt a, PointInt b) => !a.Equals(b);
    
    public override bool Equals(object? obj) => obj is PointInt other && Equals(other);

    public bool Equals(PointInt other) => X == other.X && Y == other.Y;

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public override string ToString()
    {
        return "{X:" + X + " Y:" + Y + "}";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 ToVector2() => new(X, Y);

    public void Deconstruct(out int x, out int y)
    {
        x = X;
        y = Y;
    }
}
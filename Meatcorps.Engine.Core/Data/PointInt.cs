using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Meatcorps.Engine.Core.Data;

/// <summary>
/// An integer 2D point with X and Y components.
/// </summary>
[DataContract]
public struct PointInt : IEquatable<PointInt>
{
    private static readonly PointInt ZeroPointInt = new();

    /// <summary>The X component of the point.</summary>
    [DataMember] public int X;

    /// <summary>The Y component of the point.</summary>
    [DataMember] public int Y;

    /// <summary>A <see cref="PointInt"/> with both components set to zero.</summary>
    public static PointInt Zero => ZeroPointInt;

    /// <summary>
    /// Initializes a new <see cref="PointInt"/> with the specified X and Y values.
    /// </summary>
    /// <param name="x">The X component.</param>
    /// <param name="y">The Y component.</param>
    public PointInt(int x, int y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Initializes a new <see cref="PointInt"/> with both components set to the same value.
    /// </summary>
    /// <param name="value">The value for both X and Y.</param>
    public PointInt(int value)
    {
        X = value;
        Y = value;
    }

    /// <summary>Adds two <see cref="PointInt"/> values component-wise.</summary>
    public static PointInt operator +(PointInt value1, PointInt value2)
    {
        return new PointInt(value1.X + value2.X, value1.Y + value2.Y);
    }

    /// <summary>Subtracts one <see cref="PointInt"/> from another component-wise.</summary>
    public static PointInt operator -(PointInt value1, PointInt value2)
    {
        return new PointInt(value1.X - value2.X, value1.Y - value2.Y);
    }

    /// <summary>Multiplies two <see cref="PointInt"/> values component-wise.</summary>
    public static PointInt operator *(PointInt value1, PointInt value2)
    {
        return new PointInt(value1.X * value2.X, value1.Y * value2.Y);
    }

    /// <summary>Divides a <see cref="PointInt"/> by another component-wise.</summary>
    public static PointInt operator /(PointInt source, PointInt divisor)
    {
        return new PointInt(source.X / divisor.X, source.Y / divisor.Y);
    }

    /// <summary>Adds a scalar integer to both components of a <see cref="PointInt"/>.</summary>
    public static PointInt operator +(PointInt value1, int value2)
    {
        return new PointInt(value1.X + value2, value1.Y + value2);
    }

    /// <summary>Subtracts a scalar integer from both components of a <see cref="PointInt"/>.</summary>
    public static PointInt operator -(PointInt value1, int value2)
    {
        return new PointInt(value1.X - value2, value1.Y - value2);
    }

    /// <summary>Multiplies both components of a <see cref="PointInt"/> by a scalar integer.</summary>
    public static PointInt operator *(PointInt value1, int value2)
    {
        return new PointInt(value1.X * value2, value1.Y * value2);
    }

    /// <summary>Divides both components of a <see cref="PointInt"/> by a scalar integer.</summary>
    public static PointInt operator /(PointInt source, int divisor)
    {
        return new PointInt(source.X / divisor, source.Y / divisor);
    }

    /// <summary>Negates both components of a <see cref="PointInt"/>.</summary>
    public static PointInt operator -(PointInt value)
    {
        return new PointInt(-value.X, -value.Y);
    }

    /// <summary>Returns <see langword="true"/> if two <see cref="PointInt"/> values are equal.</summary>
    public static bool operator ==(PointInt a, PointInt b) => a.Equals(b);

    /// <summary>Returns <see langword="true"/> if two <see cref="PointInt"/> values are not equal.</summary>
    public static bool operator !=(PointInt a, PointInt b) => !a.Equals(b);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is PointInt other && Equals(other);

    /// <summary>Returns <see langword="true"/> if this point equals another <see cref="PointInt"/>.</summary>
    /// <param name="other">The point to compare to.</param>
    public bool Equals(PointInt other) => X == other.X && Y == other.Y;

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return "{X:" + X + " Y:" + Y + "}";
    }

    /// <summary>Converts this <see cref="PointInt"/> to a <see cref="Vector2"/>.</summary>
    /// <returns>A <see cref="Vector2"/> with the same X and Y values.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 ToVector2() => new(X, Y);

    /// <summary>Deconstructs the point into its X and Y components.</summary>
    /// <param name="x">The X component.</param>
    /// <param name="y">The Y component.</param>
    public void Deconstruct(out int x, out int y)
    {
        x = X;
        y = Y;
    }
}
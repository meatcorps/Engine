using System.Numerics;
using System.Runtime.Serialization;
using Meatcorps.Engine.Core.Extensions;

namespace Meatcorps.Engine.Core.Data;

/// <summary>
/// Represents a 2D size with floating-point width and height components.
/// Both dimensions are clamped to zero or greater.
/// </summary>
[DataContract]
public struct SizeF : IEquatable<SizeF>
{
    private float _width;

    /// <summary>The width. Clamped to zero or greater.</summary>
    [DataMember]
    public float Width { get => _width; set => _width = Math.Max(0, value); }

    private float _height;

    /// <summary>The height. Clamped to zero or greater.</summary>
    [DataMember]
    public float Height { get => _height; set => _height = Math.Max(0, value); }

    /// <summary>
    /// Initializes a new <see cref="SizeF"/> with the specified width and height.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public SizeF(float width, float height)
    {
        Width = width;
        Height = height;
    }

    /// <summary>Converts this <see cref="SizeF"/> to a <see cref="Vector2"/>.</summary>
    /// <returns>A <see cref="Vector2"/> with X equal to Width and Y equal to Height.</returns>
    public Vector2 ToVector2() => new Vector2(Width, Height);

    // --- Operators: SizeF <-> SizeF -----------------------------------------

    /// <summary>Adds two <see cref="SizeF"/> values component-wise.</summary>
    public static SizeF operator +(SizeF a, SizeF b)
        => new SizeF(a.Width + b.Width, a.Height + b.Height);

    /// <summary>Subtracts one <see cref="SizeF"/> from another component-wise.</summary>
    public static SizeF operator -(SizeF a, SizeF b)
        => new SizeF(a.Width - b.Width, a.Height - b.Height);

    // --- Operators: SizeF <-> float -----------------------------------------

    /// <summary>Multiplies both dimensions of a <see cref="SizeF"/> by a scalar float.</summary>
    public static SizeF operator *(SizeF a, float scalar)
        => new SizeF(a.Width * scalar, a.Height * scalar);

    /// <summary>Divides both dimensions of a <see cref="SizeF"/> by a scalar float. Returns the original size if the scalar is zero.</summary>
    public static SizeF operator /(SizeF a, float scalar)
        => scalar.EqualsSafe(0) ? a : new SizeF(a.Width / scalar, a.Height / scalar);

    /// <summary>Multiplies both dimensions of a <see cref="SizeF"/> by a scalar float (scalar on left).</summary>
    // Allow float * SizeF
    public static SizeF operator *(float scalar, SizeF a) => a * scalar;

    // --- Operators: SizeF <-> Vector2 ---------------------------------------

    /// <summary>Adds a <see cref="Vector2"/> to a <see cref="SizeF"/> component-wise.</summary>
    public static SizeF operator +(SizeF s, Vector2 v)
        => new SizeF(s.Width + v.X, s.Height + v.Y);

    /// <summary>Subtracts a <see cref="Vector2"/> from a <see cref="SizeF"/> component-wise.</summary>
    public static SizeF operator -(SizeF s, Vector2 v)
        => new SizeF(s.Width - v.X, s.Height - v.Y);

    /// <summary>Multiplies a <see cref="SizeF"/> by a <see cref="Vector2"/> component-wise.</summary>
    public static SizeF operator *(SizeF s, Vector2 v)
        => new SizeF(s.Width * v.X, s.Height * v.Y);

    /// <summary>Divides a <see cref="SizeF"/> by a <see cref="Vector2"/> component-wise. Zero components in the vector leave the corresponding dimension unchanged.</summary>
    public static SizeF operator /(SizeF s, Vector2 v)
        => new SizeF(
            v.X.EqualsSafe(0) ? s.Width : s.Width / v.X,
            v.Y.EqualsSafe(0) ? s.Height : s.Height / v.Y
        );

    // --- Conversions ---------------------------------------------------------

    /// <summary>Implicitly converts a <see cref="SizeF"/> to a <see cref="Vector2"/>.</summary>
    public static implicit operator Vector2(SizeF s)
        => new Vector2(s.Width, s.Height);

    /// <summary>Implicitly converts a <see cref="Vector2"/> to a <see cref="SizeF"/>.</summary>
    public static implicit operator SizeF(Vector2 v)
        => new SizeF(v.X, v.Y);

    // --- Equality ------------------------------------------------------------

    /// <summary>Returns <see langword="true"/> if this size equals another <see cref="SizeF"/>.</summary>
    /// <param name="other">The size to compare to.</param>
    public bool Equals(SizeF other)
        => Width.Equals(other.Width) && Height.Equals(other.Height);

    /// <inheritdoc/>
    public override bool Equals(object? obj)
        => obj is SizeF other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
        => HashCode.Combine(Width, Height);

    /// <inheritdoc/>
    public override string ToString() => $"{{Width:{Width} Height:{Height}}}";

    /// <summary>A <see cref="SizeF"/> with both dimensions set to zero.</summary>
    public static readonly SizeF Zero = new(0, 0);

    /// <summary>A <see cref="SizeF"/> with both dimensions set to one.</summary>
    public static readonly SizeF One  = new(1, 1);
}
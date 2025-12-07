using System.Numerics;
using System.Runtime.Serialization;
using Meatcorps.Engine.Core.Extensions;

namespace Meatcorps.Engine.Core.Data;

[DataContract]
public struct SizeF : IEquatable<SizeF>
{
    private float _width;
    [DataMember]
    public float Width { get => _width; set => _width = Math.Max(0, value); }
    
    private float _height;
    [DataMember]
    public float Height { get => _height; set => _height = Math.Max(0, value); }

    public SizeF(float width, float height)
    {
        Width = width;
        Height = height;
    }

    public Vector2 ToVector2() => new Vector2(Width, Height);

    // --- Operators: SizeF <-> SizeF -----------------------------------------

    public static SizeF operator +(SizeF a, SizeF b)
        => new SizeF(a.Width + b.Width, a.Height + b.Height);

    public static SizeF operator -(SizeF a, SizeF b)
        => new SizeF(a.Width - b.Width, a.Height - b.Height);

    // --- Operators: SizeF <-> float -----------------------------------------

    public static SizeF operator *(SizeF a, float scalar)
        => new SizeF(a.Width * scalar, a.Height * scalar);

    public static SizeF operator /(SizeF a, float scalar)
        => scalar.EqualsSafe(0) ? a : new SizeF(a.Width / scalar, a.Height / scalar);

    // Allow float * SizeF
    public static SizeF operator *(float scalar, SizeF a) => a * scalar;

    // --- Operators: SizeF <-> Vector2 ---------------------------------------

    public static SizeF operator +(SizeF s, Vector2 v)
        => new SizeF(s.Width + v.X, s.Height + v.Y);

    public static SizeF operator -(SizeF s, Vector2 v)
        => new SizeF(s.Width - v.X, s.Height - v.Y);

    public static SizeF operator *(SizeF s, Vector2 v)
        => new SizeF(s.Width * v.X, s.Height * v.Y);

    public static SizeF operator /(SizeF s, Vector2 v)
        => new SizeF(
            v.X.EqualsSafe(0) ? s.Width : s.Width / v.X,
            v.Y.EqualsSafe(0) ? s.Height : s.Height / v.Y
        );

    // --- Conversions ---------------------------------------------------------

    public static implicit operator Vector2(SizeF s)
        => new Vector2(s.Width, s.Height);

    public static implicit operator SizeF(Vector2 v)
        => new SizeF(v.X, v.Y);

    // --- Equality ------------------------------------------------------------

    public bool Equals(SizeF other)
        => Width.Equals(other.Width) && Height.Equals(other.Height);

    public override bool Equals(object? obj)
        => obj is SizeF other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(Width, Height);

    public override string ToString() => $"{{Width:{Width} Height:{Height}}}";
    
    public static readonly SizeF Zero = new(0, 0);
    public static readonly SizeF One  = new(1, 1);
}
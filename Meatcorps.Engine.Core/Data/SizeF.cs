using System.Runtime.Serialization;

namespace Meatcorps.Engine.Core.Data;

[DataContract]
public struct SizeF : IEquatable<SizeF>
{
    [DataMember]
    public float Width { get; set; }
    
    [DataMember]
    public float Height { get; set; }

    public SizeF(float width, float height)
    {
        Width = width;
        Height = height;
    }
    
    public bool Equals(SizeF other)
    {
        return Width.Equals(other.Width) && Height.Equals(other.Height);
    }

    public override bool Equals(object? obj)
    {
        return obj is SizeF other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Width, Height);
    }
}
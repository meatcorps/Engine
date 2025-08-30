using Meatcorps.Engine.Session.Utilities;

namespace Meatcorps.Engine.Session.ValueTypes;

public struct ValueType<T>: IValueType, IEquatable<T>
{
    private static readonly int _hash = SessionUtil.Fnv1aHash(nameof(T));

    public override string ToString()
    {
        return nameof(T);
    }

    // All PlayerId tags are equal (no internal state)
    public bool Equals(T other)
    {
        return true;
    }

    public override bool Equals(object? obj)
    {
        return obj is T;
    }

    public override int GetHashCode()
    {
        return _hash; // stable, precomputed; no ToString allocation per call
    }
}
using Meatcorps.Engine.Session.Utilities;

namespace Meatcorps.Engine.Session.ValueTypes;

public readonly struct PlayerId : IEquatable<PlayerId>, IValueType
{
    private static readonly int _hash = SessionUtil.Fnv1aHash(nameof(PlayerId));

    public override string ToString()
    {
        return nameof(PlayerId);
    }

    // All PlayerId tags are equal (no internal state)
    public bool Equals(PlayerId other)
    {
        return true;
    }

    public override bool Equals(object? obj)
    {
        return obj is PlayerId;
    }

    public override int GetHashCode()
    {
        return _hash; // stable, precomputed; no ToString allocation per call
    }
}


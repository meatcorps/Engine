using Meatcorps.Engine.Session.Utilities;

namespace Meatcorps.Engine.Session.ValueTypes;

public readonly struct SessionSeed : IEquatable<SessionSeed>, IValueType
{
    private static readonly int _hash = SessionUtil.Fnv1aHash(nameof(PlayerId));

    public override string ToString()
    {
        return nameof(SessionSeed);
    }

    // All PlayerId tags are equal (no internal state)
    public bool Equals(SessionSeed other)
    {
        return true;
    }

    public override bool Equals(object? obj)
    {
        return obj is SessionSeed;
    }

    public override int GetHashCode()
    {
        return _hash; // stable, precomputed; no ToString allocation per call
    }
}
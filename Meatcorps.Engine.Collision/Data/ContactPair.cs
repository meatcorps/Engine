using System.Runtime.CompilerServices;
using Meatcorps.Engine.Collision.Interfaces;

namespace Meatcorps.Engine.Collision.Data;

public readonly struct ContactPair : IEquatable<ContactPair>
{
    public readonly ICollider A; // ordered (stable)
    public readonly ICollider B;

    public ContactPair(ICollider a, ICollider b)
    {
        // Order by body StableIndex, then collider index to make hashing stable
        if (BodyIndex(a) < BodyIndex(b) || (BodyIndex(a) == BodyIndex(b) && ColliderIndex(a) <= ColliderIndex(b)))
        {
            A = a;
            B = b;
        }
        else
        {
            A = b;
            B = a;
        }
    }

    public bool Equals(ContactPair other)
    {
        return ReferenceEquals(A, other.A) && ReferenceEquals(B, other.B);
    }

    public override bool Equals(object? obj)
    {
        return obj is ContactPair other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(A, B);
    }
    
    public bool ContainsOwner(object owner) => A.Body.Owner == owner || B.Body.Owner == owner;

    public bool TryGetOwner<T>(out T owner)
    {
        owner = default;
        if (A.Body.Owner is T t)
        {
            owner = t;
            return true;
        }

        if (B.Body.Owner is T t2)
        {
            owner = t2;
            return true;
        }
        return false;
    }

    private static int BodyIndex(ICollider c) => c.Body.StableIndex;
    private static int ColliderIndex(ICollider c)
    {
        // If you can expose a collider index, use it.
        // Otherwise, fall back to reference hash; stable enough for runtime session
        return RuntimeHelpers.GetHashCode(c);
    }
}
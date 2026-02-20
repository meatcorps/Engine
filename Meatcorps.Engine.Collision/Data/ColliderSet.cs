// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace Meatcorps.Engine.Collision.Data;

public readonly record struct ColliderSet()
{
    public int A { get; init; }
    public int B { get; init; }

    public ColliderSet(int t1, int t2) : this() => (A, B) = t1 <= t2 ? (t1, t2) : (t2, t1);
}
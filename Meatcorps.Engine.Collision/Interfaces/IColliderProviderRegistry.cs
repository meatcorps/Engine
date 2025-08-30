namespace Meatcorps.Engine.Collision.Interfaces;

public interface IColliderProviderRegistry
{
    bool TryGet(ICollider a, ICollider b, out IColliderProvider provider, out bool swapped);
}
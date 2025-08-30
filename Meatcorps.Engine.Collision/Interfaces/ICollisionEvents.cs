using Meatcorps.Engine.Collision.Data;
using Meatcorps.Engine.Collision.Enums;

namespace Meatcorps.Engine.Collision.Interfaces;

public interface ICollisionEvents
{
    // Fired for solid contacts after narrow-phase; manifold present on Enter/Stay
    void OnContact(ContactPhase phase, in ContactPair pair, in ContactManifold manifold);

    // Fired for sensors; no manifold required (you can still pass OverlapArea if useful)
    void OnTrigger(ContactPhase phase, in ContactPair pair);
}

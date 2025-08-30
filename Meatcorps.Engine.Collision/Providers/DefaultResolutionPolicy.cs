using Meatcorps.Engine.Collision.Enums;
using Meatcorps.Engine.Collision.Interfaces;

namespace Meatcorps.Engine.Collision.Providers;

public class DefaultResolutionPolicy: IResolutionPolicy
{
    public (float pushA, float pushB) Decide(IBody a, IBody b)
    {
        bool aImm = a.BodyType is BodyType.Static or BodyType.Kinematic;
        bool bImm = b.BodyType is BodyType.Static or BodyType.Kinematic;

        if (aImm && bImm) return (0f, 0f);
        if (aImm && !bImm) return (0f, 1f);
        if (!aImm && bImm) return (1f, 0f);

        // Dynamic vs Dynamic: split by mass
        var ma = MathF.Max(1e-6f, a.Mass);
        var mb = MathF.Max(1e-6f, b.Mass);
        var total = ma + mb;
        return (mb / total, ma / total);
    }
}
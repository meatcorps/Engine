using System.Numerics;
using Meatcorps.Engine.Core.Data;

namespace Meatcorps.Engine.Collision.Interfaces;

public interface IWorldEntityResource
{
    void Add(IBody collider);
    void Remove(IBody collider);
    void Update(IBody collider);
    HashSet<IBody> Query(RectF queryAABB);
    HashSet<IBody> Query(Vector2 position);
}
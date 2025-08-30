using System.Numerics;
using Meatcorps.Engine.Collision.Colliders;
using Meatcorps.Engine.Collision.Data;
using Meatcorps.Engine.Collision.Enums;
using Meatcorps.Engine.Collision.Interfaces;
using Meatcorps.Engine.Collision.Providers.Bodies;
using Meatcorps.Engine.Collision.Services;
using Meatcorps.Engine.Core.Data;

namespace Meatcorps.Engine.Collision.Extensions;

public static class WorldExtensions
{
    public static Body RegisterRectFBody(this WorldService world, object owner, RectF rect, bool registerCollisionEvents = true)
    {
        var body = new Body(world, owner);
        body.Position = rect.Position;
        body.AddCollider(new RectCollider(body, new RectF(Vector2.Zero, rect.Size))); 
        if (owner is ICollisionEvents events && registerCollisionEvents)
            world.AddCollisionEvents(events);
        
        world.RegisterBody(body);
        return body;
    }
    
    
    public static Body RegisterRectFBodySensor(this WorldService world, object owner, RectF rect, bool registerCollisionEvents = true)
    {
        var body = new Body(world, owner);
        body.Position = rect.Position;
        body.AddCollider(new RectCollider(body, new RectF(Vector2.Zero, rect.Size)).SetSensor(true)); 
        if (owner is ICollisionEvents events && registerCollisionEvents)
            world.AddCollisionEvents(events);
        
        world.RegisterBody(body);
        return body;
    }
    
    public static Body RegisterStaticRectFBody(this WorldService world, object owner, RectF rect)
    {
        var body = RegisterRectFBody(world, owner, rect);
        body.SetType(BodyType.Static);
        world.RegisterBody(body);
        return body;
    }
}
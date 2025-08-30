using Meatcorps.Engine.Collision.Interfaces;
using Meatcorps.Engine.Collision.Utilities;

namespace Meatcorps.Engine.Collision.Extensions;

public static class IColliderExtensions
{
    public static T SetLayer<TEnum, T>(this T collider, TEnum layerEnum) 
        where TEnum : struct, Enum
        where T : ICollider
    
    {
        collider.Layer = LayerBits.Bit(layerEnum);
        return collider;
    }

    public static T SetMask<TEnum, T>(this T collider, params TEnum[] layers) 
        where TEnum : struct, Enum
        where T : ICollider
    {
        collider.CollisionMask = LayerBits.MaskOf(layers);
        return collider;
    }
}
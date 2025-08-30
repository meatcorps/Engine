using System.Numerics;
using Meatcorps.Engine.RayLib.Renderer;
using Meatcorps.Engine.RayLib.Resources;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Extensions;

public static class Texture2DExtensions
{
    public static NineSliceRenderer CreateNineSlice<T>(
        this Texture2DItem<T> item,
        T spriteKey,
        int left = 2, int top = 2, int right = 2, int bottom = 2
    ) where T : Enum
    {
        var rect = item.GetSprite(spriteKey);
        return new NineSliceRenderer(item.Texture, left, top, right, bottom, rect);
    }

    public static void Draw<T>(this Texture2DItem<T> item, T spriteKey, Vector2 position, Color? color = null, float rotation = 0,
        Vector2 origin = default, float scale = 1, bool flippedX = false, bool flippedY = false) where T : Enum
    {
        var rect = item.GetSprite(spriteKey);
        var src = rect;
        if (flippedX)
            src.Width = -src.Width;
        if (flippedY)
            src.Height = -src.Height;
        Raylib.DrawTexturePro(item.Texture, src, new Rectangle(position, rect.Size * scale), origin, rotation, color ?? Color.White);
    }
    
    public static void DrawAnimation<T>(this Texture2DItem<T> item, T animationKey, int animationPosition, Vector2 position, Color? color = null, float rotation = 0,
        Vector2 origin = default, float scale = 1) where T : Enum
    {
        var rect = item.GetAnimation(animationKey, animationPosition);
        Raylib.DrawTexturePro(item.Texture, rect, new Rectangle(position, rect.Size * scale), origin, rotation, color ?? Color.White);
    }
    
    public static void DrawAnimationWithNormal<T>(this Texture2DItem<T> item, T animationKey, float normalPosition, Vector2 position, Color? color = null, float rotation = 0,
        Vector2 origin = default, float scale = 1) where T : Enum
    {
        var animationPosition = (int)(item.GetAnimationCount(animationKey) * normalPosition);
        var rect = item.GetAnimation(animationKey, animationPosition);
        Raylib.DrawTexturePro(item.Texture, rect, new Rectangle(position, rect.Size * scale), origin, rotation, color ?? Color.White);
    }
    public static void DrawAnimationWithNormal<T>(this Texture2DItem<T> item, T animationKey, float normalPosition, Rectangle destRect, Color? color = null, float rotation = 0,
        Vector2 origin = default) where T : Enum
    {
        var animationPosition = (int)(item.GetAnimationCount(animationKey) * normalPosition);
        var rect = item.GetAnimation(animationKey, animationPosition);
        Raylib.DrawTexturePro(item.Texture, rect, destRect, origin, rotation, color ?? Color.White);
    }
}
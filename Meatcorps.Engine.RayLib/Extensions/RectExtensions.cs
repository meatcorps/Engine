using Meatcorps.Engine.Core.Data;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Extensions;

public static class RectExtensions
{
    public static Rectangle ToRectangle(this Rect rect)
    {
        return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
    }
}
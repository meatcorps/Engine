using System.Numerics;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Engine.RayLib.Text;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Extensions;

public static class TextStyleExtensions
{
    /// <summary>
    /// Returns a rectangle containing the bounds of the given text using the style's font, size, spacing, and line height.
    /// </summary>
    public static Rectangle GetTextBounds<T>(this TextManager<T> manager, T type, string text, Vector2 position, TextStyle style) where T : Enum
    {
        var size = TextKit.Measure(style, text);
        return new Rectangle(position.X, position.Y, size.X, size.Y);
    }
}
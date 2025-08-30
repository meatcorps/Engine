using System.Numerics;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Engine.RayLib.Text;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Extensions;

public static class TextManagerExtensions
{
    public static bool HasFont<T>(this TextManager<T> manager, T fontType) where T : Enum
    {
        return manager != null && manager.TryGetFont(fontType, out _);
    }

    public static bool TryGetFont<T>(this TextManager<T> manager, T fontType, out Font font) where T : Enum
    {
        try
        {
            font = manager.GetFont(fontType);
            return true;
        }
        catch
        {
            font = default;
            return false;
        }
    }

    public static Vector2 MeasureText<T>(this TextManager<T> manager, T fontType, string text, float fontSize, float spacing) where T : Enum
    {
        var font = manager.GetFont(fontType);
        return Raylib.MeasureTextEx(font, text, fontSize, spacing);
    }

    public static Vector2 MeasureTextDefault<T>(this TextManager<T> manager, string text, float fontSize, float spacing) where T : Enum
    {
        var font = manager.GetFont();
        return Raylib.MeasureTextEx(font, text, fontSize, spacing);
    }

    public static Vector2 CenteredPosition<T>(this TextManager<T> manager, string text, float fontSize, float spacing, Vector2 areaSize) where T : Enum
    {
        var size = manager.MeasureTextDefault(text, fontSize, spacing);
        return (areaSize - size) / 2f;
    }

    public static TextStyle CreateStyle<T>(this TextManager<T> manager, T type, float size, float spacing = 0f, float lineHeight = 1.2f, Color? color = null) where T : Enum
    {
        return TextStyle.Create(manager.GetFont(type), size, spacing, lineHeight, color);
    }
}
using System.Numerics;
using Meatcorps.Engine.RayLib.Resources;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.RemixIcons;

public static class RemixIconExtensions
{
    public static string ToGlyph(this RemixIcon icon)
    {
        return char.ConvertFromUtf32((int)icon);
    }

    /// <summary>
    /// To use this font. You need to load download it from https://remixicon.com/. Then put the ttf inside your assets' folder. The default is "Assets/Fonts/remixicon.ttf"
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="font"></param>
    /// <param name="location"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static TextManager<T> AddRemixFont<T>(this TextManager<T> manager, T font, string location = "Assets/Fonts/remixicon.ttf") where T: Enum
    {
        var codePoints = new List<int>();
        foreach (var icon in Enum.GetValues<RemixIcon>())
        {
            codePoints.Add((int)icon);
        }
        manager.AddFont(location, font, 32, TextureFilter.Point, codePoints.ToArray());
        return manager;
    }

    public static void DrawRemixIcon<T>(this TextManager<T> manager, T font, RemixIcon icon, Vector2 positon, float size = 32, Color? color = null) where T: Enum
    {
        if (color == null)
            color = Color.White;
            
        var glyph = icon.ToGlyph();
        var font2 = manager.GetFont(font);
        Raylib.DrawTextEx(
            font2,
            glyph,
            positon,
            size,
            1,
            color.Value
        );
    }

    public static void DrawRemixIcon<T>(this TextManager<T> manager, T font, RemixIcon icon, Vector2 positon, float rotation, float size = 32, Color? color = null) where T: Enum
    {
        if (color == null)
            color = Color.White;
            
        var origin = new Vector2(size / 2, size / 2);
        var glyph = icon.ToGlyph(); 
        Raylib.DrawTextPro(
            manager.GetFont(font),
            glyph,
            positon,
            origin,
            rotation,
            size,
            1,
            color.Value
        );
    }
}
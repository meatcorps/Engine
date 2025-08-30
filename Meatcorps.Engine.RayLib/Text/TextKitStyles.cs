using System.Numerics;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Text;

public static class TextKitStyles
{
    public static TextStyle HudDefault(Font font) => new TextStyle
    {
        Font = font,
        Size = 16f,
        Color = Color.White,
        ShadowOffset = new Vector2(2, 2),
        ShadowColor = Color.Black
    };

    public static TextStyle HudAlert(Font font) => new TextStyle
    {
        Font = font,
        Size = 24f,
        Color = Color.Red,
        ShadowOffset = new Vector2(3, 3),
        ShadowColor = Color.Black
    };
    
    /// <summary>
    /// Large centered text, good for countdowns, titles, or big alerts.
    /// </summary>
    public static TextStyle BigCenter(Font font) => new TextStyle
    {
        Font = font,
        Size = 32f,
        Color = Color.White,
        ShadowOffset = new Vector2(4, 4),
        ShadowColor = Color.Black
    };
}
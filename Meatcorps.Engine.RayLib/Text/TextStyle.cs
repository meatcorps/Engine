using Meatcorps.Engine.RayLib.Resources;

namespace Meatcorps.Engine.RayLib.Text;

using System.Numerics;
using Raylib_cs;

public struct TextStyle
{
    public Font Font;
    public float Size;
    public float Spacing;
    public float LineHeight;
    public Color Color;

    // Shadow
    public bool UseShadow;
    public Vector2 ShadowOffset;
    public Color ShadowColor;

    // Outline
    public bool UseOutline;
    public int OutlineSize;     // in pixels
    public Color OutlineColor;
    public bool PixelOutline;   // NEW: true = chunky arcade border, false = smooth

    public static TextStyle Create(Font font, float size, float spacing = 0f, float lineHeight = 1.2f, Color? color = null)
        => new TextStyle
        {
            Font = font,
            Size = size,
            Spacing = spacing,
            LineHeight = lineHeight,
            Color = color ?? Color.White,
            UseShadow = false,
            ShadowOffset = new Vector2(1, 1),
            ShadowColor = new Color(0, 0, 0, 255),
            UseOutline = false,
            OutlineSize = 1,
            OutlineColor = new Color(0, 0, 0, 255),
            PixelOutline = false
        };
}
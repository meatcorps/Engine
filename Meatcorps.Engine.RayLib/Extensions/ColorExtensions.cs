using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Extensions;

public static class ColorExtensions
{
    public static Color Heatmap(float normal)
    {
        return Raylib.ColorFromHSV(240 - (normal * 240), 1, 1);
    }
}
using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Pathfinding.Utilities;
using Meatcorps.Engine.RayLib.Extensions;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Pathfinding.Extensions;

public static class GridDistanceHeatmapVisualizer
{
    public static void DrawHeatMap(this GridDistanceCalculator calculator, Rect topLeftCell, float alpha = 0.5f, bool includeNumbers = false)
    {
        foreach (var position in calculator.Visited)
        {
            var normal = Math.Clamp((float)calculator.Resource.Get(position) / calculator.MaxDistance, 0, 1);
            var color = Raylib.ColorLerp(Color.Blue, Color.Red, normal); //ColorExtensions.Heatmap(normal);
            var worldPosition = position * topLeftCell.Size;
            Raylib.DrawRectangleV((worldPosition + topLeftCell.Position).ToVector2() + new Vector2(2, 2), topLeftCell.Size.ToVector2() - new Vector2(4, 4), Raylib.ColorAlpha(color, alpha));
            if (includeNumbers)
                Raylib.DrawTextEx(Raylib.GetFontDefault(), calculator.Resource.Get(position).ToString(), (worldPosition + topLeftCell.Position).ToVector2() + new Vector2(2, 2), 10, 1, Color.White);
        }
    }
}
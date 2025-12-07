using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Extensions;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Extensions;

public static class RectFExtensions
{
    public static void DrawFilled(this RectF rectF, Color color, float roundness = 0f, int segmentCount = 4)
    {
        if (roundness.EqualsSafe(0))
        {
            Raylib.DrawRectangleRounded(rectF.ToRectangle(), roundness, segmentCount, color);
            return;
        }
        
        Raylib.DrawRectangleRec(rectF.ToRectangle(), color);
    }
    
    public static void DrawLines(this RectF rectF, Color color, float thickness = 1f, float roundness = 0f, int segmentCount = 4)
    {
        if (roundness.EqualsSafe(0))
        {
            Raylib.DrawRectangleRoundedLinesEx(rectF.ToRectangle(), roundness, segmentCount, thickness, color);
            return;
        }
        
        Raylib.DrawRectangleLinesEx(rectF.ToRectangle(), thickness, color);
    }

    public static void DrawFillAndLines(this RectF rectF, Color outerColor, Color innerColor, float thickness = 1f,
        float roundness = 0f, int segmentCount = 4)
    {
        DrawFilled(rectF, outerColor, roundness, segmentCount);
        DrawLines(rectF, innerColor, thickness, roundness, segmentCount);
    }
}
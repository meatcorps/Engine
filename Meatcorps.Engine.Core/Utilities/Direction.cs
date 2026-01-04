using System.Numerics;

namespace Meatcorps.Engine.Core.Utilities;

public static class Direction
{
    public static Vector2 LeftTop = new(-1, -1);
    public static Vector2 RightTop = new(1, -1);
    public static Vector2 RightBottom = new(1, 1);
    public static Vector2 LeftBottom = new(-1, 1);
    public static Vector2 Left = new(-1, 0);
    public static Vector2 Right = new(1, 0);
    public static Vector2 Top = new(0, -1);
    public static Vector2 Bottom = new(0, 1);
}
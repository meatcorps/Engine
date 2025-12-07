using System.Numerics;

namespace Meatcorps.Engine.Core.Utilities;

public static class UVHelper
{
    public static Vector2 Center = new(0.5f, 0.5f);
    public static Vector2 LeftTop = new(0, 0);
    public static Vector2 RightTop = new(1, 0);
    public static Vector2 RightBottom = new(1, 1);
    public static Vector2 LeftBottom = new(0, 1);
    public static Vector2 Left = new(0, 0.5f);
    public static Vector2 Right = new(1, 0.5f);
    public static Vector2 Top = new(0.5f, 0);
    public static Vector2 Bottom = new(0.5f, 1);
}
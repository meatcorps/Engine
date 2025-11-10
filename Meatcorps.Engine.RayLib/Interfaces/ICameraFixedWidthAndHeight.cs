using System.Numerics;

namespace Meatcorps.Engine.RayLib.Interfaces;

public interface ICameraFixedWidthAndHeight
{
    int TargetWidth { get; }
    int TargetHeight { get; }
    Vector2 WorldToScreen(Vector2 worldPos);
    Vector2 ScreenToWorld(Vector2 screenPos);
}
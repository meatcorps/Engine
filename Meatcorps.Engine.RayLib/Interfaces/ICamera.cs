using System.Numerics;
using Meatcorps.Engine.RayLib.Game;

namespace Meatcorps.Engine.RayLib.Interfaces;

public interface ICamera
{
    public Vector2 Position { get; set; }
    public float Zoom { get; set; }
    void StartWorldCamera();
    void EndWorldCamera();
    void StartUICamera();
    void EndUICamera();
    void Update(float deltaTime, IRenderTargetStrategy renderTargetStrategy);
}
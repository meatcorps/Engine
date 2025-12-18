using System.Numerics;

namespace Meatcorps.Engine.RayLib.Interfaces;

public interface ICamera
{
    public Vector2 Position { get; set; }
    public float Zoom { get; set; }
    void StartCamera();
    void EndCamera();
    void Update(float deltaTime, IRenderTargetStrategy renderTargetStrategy);
}
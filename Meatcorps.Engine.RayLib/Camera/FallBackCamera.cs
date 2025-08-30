using System.Numerics;
using Meatcorps.Engine.RayLib.Game;
using Meatcorps.Engine.RayLib.Interfaces;

namespace Meatcorps.Engine.RayLib.Camera;

public class FallBackCamera : ICamera
{
    public Vector2 Position { get; set; }
    public float Zoom { get; set; }
    
    public void SetGameHost(GameHost gameHost)
    {
    }
    
    public void StartWorldCamera()
    {
    }

    public void EndWorldCamera()
    {
    }

    public void StartUICamera()
    {
    }

    public void EndUICamera()
    {
    }

    public void Update(float deltaTime, IRenderTargetStrategy renderTargetStrategy)
    {
    }
}
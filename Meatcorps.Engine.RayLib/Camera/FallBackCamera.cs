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
    
    public void StartCamera()
    {
    }

    public void EndCamera()
    {
    }

    public void Update(float deltaTime, IRenderTargetStrategy renderTargetStrategy)
    {
    }
}
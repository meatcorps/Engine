using System.Numerics;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Game;
using Meatcorps.Engine.RayLib.Interfaces;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Camera;

public class FixedSizeCamera : ICamera, ICameraFixedWidthAndHeight
{
    private GameHost? _gameHost;
    public Camera2D Camera { get; set; } = new Camera2D();
    
    public int TargetWidth { get; }
    public int TargetHeight { get; }
    
    private float _zoom = 0;
    
    public Vector2 Position
    {
        get => Camera.Target;
        set
        {
            var camera = Camera;
            camera.Target = value;
            Camera = camera;
        }
    }

    public float Zoom { get; set; } = 0;

    public FixedSizeCamera(int targetWidth, int targetHeight)
    {
        TargetWidth = targetWidth;
        TargetHeight = targetHeight;
    }

    public void StartCamera()
    {
        Raylib.BeginMode2D(Camera);
    }

    public void EndCamera()
    {
        Raylib.EndMode2D();
    }

    private void UpdateZoom(IRenderTargetStrategy renderTargetStrategy)
    {
        var scaleX = (float)renderTargetStrategy.RenderWidth / TargetWidth;
        var scaleY = (float)renderTargetStrategy.RenderHeight / TargetHeight;

        _zoom = MathF.Min(scaleX, scaleY);
    }
    
    public Vector2 WorldToScreen(Vector2 worldPos) => Raylib.GetWorldToScreen2D(worldPos, Camera);
    public Vector2 ScreenToWorld(Vector2 screenPos) => Raylib.GetScreenToWorld2D(screenPos, Camera);

    public void Update(float deltaTime, IRenderTargetStrategy renderTargetStrategy)
    {
        if (_gameHost == null)
            _gameHost = GlobalObjectManager.ObjectManager.Get<GameHost>();
        
        if (_gameHost == null)
            return;
        
        if (TargetWidth == 0 || TargetHeight == 0 || _gameHost.Width == 0 || _gameHost.Height == 0)
            return;
        
        UpdateZoom(renderTargetStrategy);
        var camera = Camera;
        camera.Zoom = _zoom + Zoom;
        camera.Offset = new Vector2(renderTargetStrategy.RenderWidth, renderTargetStrategy.RenderHeight) / 2;
        Camera = camera;
    }
}
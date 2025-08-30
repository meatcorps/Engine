using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Game;
using Meatcorps.Engine.RayLib.Interfaces;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Renderer;

public class BasicScreenRenderTarget : IRenderTargetStrategy
{
    private GameHost? _gameHost;
    
    public void BeginRender(Color clearColor, ICamera camera)
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(clearColor);
    }

    public void PostProcess(CameraLayer layer)
    {
    }

    public void EndRender()
    {
    }

    public void EndDrawing()
    {
        Raylib.EndDrawing();
    }

    private void LoadGameHost()
    {
        if (_gameHost is not null)
            return;
        
        _gameHost = GlobalObjectManager.ObjectManager.Get<GameHost>();
    }

    public int RenderWidth
    {
        get
        {
            LoadGameHost();
            return _gameHost?.Width ?? 0;
        }
    }

    public int RenderHeight
    {
        get
        {
            LoadGameHost();
            return _gameHost?.Height ?? 0;
        }
    }
}
using Meatcorps.Engine.RayLib.Enums;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Interfaces;

public interface IRenderTargetStrategy
{
    void BeginRender(Color clearColor, ICamera camera);

    void PostProcess(CameraLayer layer);
    
    void EndRender();
    void EndDrawing();

    int RenderWidth { get; }
    int RenderHeight { get; }
}
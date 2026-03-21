using Meatcorps.Engine.Core.Data;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Interfaces;

public interface IRenderTargetStrategy
{
    public string Name { get; set; }
    ICamera? Camera { get; set; }
    List<IPostProcessor> PostProcessors { get; set; }
    RectF Bounds { get; set; }
    bool UsePercentage { get; set; }
    
    int RenderWidth { get; }
    int RenderHeight { get; }
}

public interface IRenderTargetStrategyRenderer: IRenderTargetStrategy
{
    PointInt? ScreenSizeOverride { get; set; }
    
    RectF GetScreenRect();
    void BeginRender(Color clearColor);

    void EndRender(RenderTexture2D? targetTexture = null);
}
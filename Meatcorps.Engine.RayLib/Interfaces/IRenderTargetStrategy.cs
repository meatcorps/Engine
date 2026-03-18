using Meatcorps.Engine.Core.Data;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Interfaces;

public interface IRenderTargetStrategy
{
    ICamera? Camera { get; set; }
    List<IPostProcessor> PostProcessors { get; set; }
    RectF Bounds { get; set; }
    bool UsePercentage { get; set; }
    internal PointInt? ScreenSizeOverride { get; set; }
    int RenderWidth { get; }
    int RenderHeight { get; }
    RectF GetScreenRect();
    void BeginRender(Color clearColor);

    void EndRender(RenderTexture2D? targetTexture = null);
}
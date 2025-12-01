using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.RayLib.Enums;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Interfaces;

public interface IRenderTargetStrategy
{
    ICamera? Camera { get; set; }
    List<IPostProcessor> PostProcessors { get; set; }
    RectF Bounds { get; set; } 
    bool UsePercentage { get; set; }
    void BeginRender(Color clearColor);
    
    void EndRender(RenderTexture2D? targetTexture = null);

    int RenderWidth { get; }
    int RenderHeight { get; }
}
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.RayLib.Interfaces;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Renderer;

public abstract class BaseRenderTarget : IRenderTargetStrategy
{
    public ICamera? Camera { get; set; }
    public List<IPostProcessor> PostProcessors { get; set; } = new();
    public RectF Bounds { get; set; }
    public bool UsePercentage { get; set; }

    public abstract void BeginRender(Color clearColor);
    public abstract void EndRender(RenderTexture2D? targetTexture = null);

    public abstract int RenderWidth { get; }
    public abstract int RenderHeight { get; }

    protected PointInt GetScreenSize()
    {
        var screenWidth = Raylib.GetScreenWidth();
        var screenHeight = Raylib.GetScreenHeight();
        return new PointInt(screenWidth, screenHeight);
    }

    protected RectF GetScreenRect()
    {
        if (!UsePercentage)
            return Bounds;

        var size = GetScreenSize();
        var rect = new RectF(0, 0, size.X, size.Y);

        rect.X *= Bounds.X;
        rect.Y *= Bounds.Y;
        rect.Width *= Bounds.Width;
        rect.Height *= Bounds.Height;

        return rect;
    }

    public BaseRenderTarget SetCamera(ICamera camera)
    {
        Camera = camera;
        return this;
    }

    public BaseRenderTarget SetBounds(RectF bounds, bool usePercentage)
    {
        Bounds = bounds;
        UsePercentage = usePercentage;
        return this;
    }

    public BaseRenderTarget SetFullScreen()
    {
        return SetBounds(new RectF(0, 0, 1, 1), true);
    }

    public BaseRenderTarget AddPostProcessor(IPostProcessor postProcessor)
    {
        PostProcessors.Add(postProcessor);
        return this;
    }
}
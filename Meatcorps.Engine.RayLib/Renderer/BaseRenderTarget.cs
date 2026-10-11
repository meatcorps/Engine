using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.RayLib.Interfaces;
using Raylib_cs;
using Color = Raylib_cs.Color;

namespace Meatcorps.Engine.RayLib.Renderer;

public abstract class BaseRenderTarget : IRenderTargetStrategyRenderer
{
    public string Name { get; set; } = "Default";
    public ICamera? Camera { get; set; }
    public List<IPostProcessor> PostProcessors { get; set; } = new();
    public RectF Bounds { get; set; }
    public bool UsePercentage { get; set; }

    public abstract void BeginRender(Color clearColor);

    public abstract void EndRender(RenderTexture2D? targetTexture = null);

    public abstract int RenderWidth { get; }
    public abstract int RenderHeight { get; }

    public PointInt? ScreenSizeOverride { get; set; }
    
    private readonly List<List<List<IRenderer>>> _gameObjects = new();
    
    private int _sceneLayers = 0;
    private int _gameObjectLayers = 0;

    protected PointInt GetScreenSize()
    {
        var screenWidth = ScreenSizeOverride?.X ?? Raylib.GetScreenWidth();
        var screenHeight = ScreenSizeOverride?.Y ?? Raylib.GetScreenHeight();
        return new PointInt(screenWidth, screenHeight);
    }

    public RectF GetScreenRect()
    {
        if (!UsePercentage)
            return Bounds;

        var size = GetScreenSize();
        var rect = new RectF(size.X, size.Y, size.X, size.Y);

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
    
    public void Setup(int sceneLayers, int gameObjectLayers)
    {
        _sceneLayers = sceneLayers;
        _gameObjectLayers = gameObjectLayers;
        
        for (var i = 0; i < sceneLayers; i++)
        {
            _gameObjects.Add(new List<List<IRenderer>>());

            for (var j = 0; j < gameObjectLayers; j++)
                _gameObjects[i].Add(new List<IRenderer>());
        }
    }

    public void RegisterRenderer(IRenderer renderer)
    {
        if (renderer.SceneLayer >= _sceneLayers)
            throw new Exception(
                $"Scene layer out of bounds, scene layer: {renderer.SceneLayer} max: {_sceneLayers}");
        
        if (renderer.Layer >= _gameObjectLayers)
            throw new Exception(
                $"Game object layer out of bounds, scene layer: {renderer.Layer} max: {_gameObjectLayers}");
        
        _gameObjects[renderer.SceneLayer][renderer.Layer].Add(renderer);
    }

    public void Render()
    {
        foreach (var layer in _gameObjects)
        foreach (var gameObjects in layer)
        {
            foreach (var gameObj in gameObjects)
                gameObj.Draw();
            gameObjects.Clear();
        }
    }
}
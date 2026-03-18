using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Interfaces;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Renderer;

/// <summary>
/// Manages the rendering pipeline and integrates rendering strategies, allowing for customizable scene layers,
/// game object layers, and background color configurations. Provides methods to set render targets, register
/// renderable objects, and orchestrate the rendering process.
/// </summary>
public class RenderService
{
    private readonly int _gameObjectLayers;

    private readonly Dictionary<IRenderTargetStrategyRenderer, List<List<List<IRenderer>>>> _gameObjects = new();
    private readonly IRenderTargetStrategy _presentationRenderTargetStrategy;
    private readonly List<IRenderTargetStrategyRenderer> _renderTargetStrategies;
    private readonly int _sceneLayers;

    private RenderTexture2D? _renderTexture;

    /// <summary>
    /// Initializes a new instance of the <see cref="RenderService"/> class.
    /// </summary>
    /// <param name="objectManager">
    /// The object manager used to resolve registered <see cref="IRenderTargetStrategy"/> instances
    /// and the final render target.
    /// </param>
    /// <param name="sceneLayers">
    /// The number of scene layers supported by the renderer. Defaults to <c>2</c>.
    /// </param>
    /// <param name="gameObjectLayers">
    /// The number of game object layers available within each scene layer. Defaults to <c>16</c>.
    /// </param>
    public RenderService(ObjectManager objectManager, int sceneLayers = 2, int gameObjectLayers = 16)
    {
        _sceneLayers = sceneLayers;
        _gameObjectLayers = gameObjectLayers;

        _renderTargetStrategies = new List<IRenderTargetStrategyRenderer>();

        _presentationRenderTargetStrategy = objectManager.Get<IRenderTargetStrategy>("FINAL") ??
                                    new BasicScreenRenderTarget().SetFullScreen();

        SetRenderTargets(objectManager.GetList<IRenderTargetStrategyRenderer>()!);
    }

    /// <summary>
    /// Gets or sets the background color used when clearing the render texture and final output.
    /// </summary>
    public Color BackgroundColor { get; set; } = Color.Black;

    /// <summary>
    /// Configures the render targets used by the rendering pipeline.
    /// If no collection is provided, the strategies are loaded from the global object manager.
    /// The final render target is always appended last to ensure it is rendered after all intermediate targets.
    /// </summary>
    /// <param name="renderTargetStrategies">
    /// The render target strategies to register, or <c>null</c> to use the globally registered strategies.
    /// </param>
    public void SetRenderTargets(IEnumerable<IRenderTargetStrategyRenderer>? renderTargetStrategies = null)
    {
        renderTargetStrategies ??= GlobalObjectManager.ObjectManager.GetList<IRenderTargetStrategyRenderer>()!;

        _renderTargetStrategies.Clear();
        _renderTargetStrategies.AddRange(renderTargetStrategies);
        //_renderTargetStrategies.AddRange(renderTargetStrategies.Where(x => x != _lastRenderTargetStrategy));
        //_renderTargetStrategies.Add(_lastRenderTargetStrategy);
        _gameObjects.Clear();

        foreach (var renderTargetStrategy in _renderTargetStrategies)
        {
            _gameObjects[renderTargetStrategy] = new List<List<List<IRenderer>>>();
            for (var i = 0; i < _sceneLayers; i++)
            {
                _gameObjects[renderTargetStrategy].Add(new List<List<IRenderer>>());

                for (var j = 0; j < _gameObjectLayers; j++)
                    _gameObjects[renderTargetStrategy][i].Add(new List<IRenderer>());
            }
        }
    }

    /// <summary>
    /// Registers a renderer for drawing during the next render pass.
    /// The renderer is placed into the appropriate render target, scene layer, and object layer bucket.
    /// </summary>
    /// <param name="gameObject">The renderer to register.</param>
    /// <exception cref="Exception">
    /// Thrown when the renderer's scene layer or object layer is outside the configured bounds.
    /// </exception>
    public void RegisterRender(IRenderer gameObject)
    {
        if (gameObject.SceneLayer >= _sceneLayers)
            throw new Exception(
                $"Scene layer out of bounds, scene layer: {gameObject.SceneLayer} max: {_sceneLayers}");
        if (gameObject.Layer >= _gameObjectLayers)
            throw new Exception(
                $"Game object layer out of bounds, scene layer: {gameObject.Layer} max: {_gameObjectLayers}");

        if (gameObject.RenderTarget == null)
            gameObject.RenderTarget = _renderTargetStrategies.First();

        if (!_gameObjects.ContainsKey((IRenderTargetStrategyRenderer)gameObject.RenderTarget))
        {
            Console.WriteLine(
                $"Render target is not registered. Render target: {gameObject.RenderTarget}, GameObject: {gameObject.GetType().FullName}");
            return;
        }

        _gameObjects[(IRenderTargetStrategyRenderer)gameObject.RenderTarget][gameObject.SceneLayer][gameObject.Layer].Add(gameObject);
    }

    /// <summary>
    /// Updates the cameras associated with all registered render target strategies.
    /// </summary>
    /// <param name="deltaTime">The elapsed time since the previous update, in seconds.</param>
    public void Update(float deltaTime)
    {
        foreach (var renderer in _renderTargetStrategies)
            renderer.Camera?.Update(deltaTime, renderer);
    }

    public void SetPresentationViewportPixels(RectF viewport)
    {
        _presentationRenderTargetStrategy.Bounds = viewport;
        _presentationRenderTargetStrategy.UsePercentage = false;
    } 
    
    public void SetPresentationViewportPercent(RectF viewport)
    {
        _presentationRenderTargetStrategy.Bounds = viewport;
        _presentationRenderTargetStrategy.UsePercentage = true;
    } 
    
    public void ResetPresentationViewport()
    {
        _presentationRenderTargetStrategy.Bounds = new RectF(0, 0, 1, 1);
        _presentationRenderTargetStrategy.UsePercentage = true;
    }

    /// <summary>
    /// Executes the rendering pipeline for all configured render targets.
    /// Intermediate targets are rendered first, and the final render target composites the generated texture to the screen.
    /// </summary>
    public void Render()
    {
        SetupRenderTexture();
        var _presentationRenderDone = false;
        foreach (var renderTargetStrategy in _renderTargetStrategies)
        {
            var presentationRenderer = renderTargetStrategy == _presentationRenderTargetStrategy;
            
            if (presentationRenderer)
            {
                renderTargetStrategy.ScreenSizeOverride = new PointInt(_renderTexture.Value.Texture.Width,
                    _renderTexture.Value.Texture.Height);
                renderTargetStrategy.BeginRender(BackgroundColor);
                Raylib.BeginBlendMode(BlendMode.AlphaPremultiply);
                Raylib.DrawTexturePro(
                    _renderTexture!.Value.Texture,
                    new Rectangle(0, 0, _renderTexture.Value.Texture.Width, -_renderTexture.Value.Texture.Height),
                    new Rectangle(0, 0, _renderTexture.Value.Texture.Width, _renderTexture.Value.Texture.Height),
                    Vector2.Zero, 0f, Color.White
                );
                Raylib.EndBlendMode();
                _presentationRenderDone = true;
                renderTargetStrategy.ScreenSizeOverride = null;
                renderTargetStrategy.EndRender();
                continue;
            }

            if (!_presentationRenderDone)
                renderTargetStrategy.ScreenSizeOverride =
                    new PointInt(_renderTexture!.Value.Texture.Width, _renderTexture!.Value.Texture.Height);
            
            renderTargetStrategy.BeginRender(new Color(0, 0, 0, 0));

            foreach (var layer in _gameObjects[renderTargetStrategy])
            foreach (var gameObjects in layer)
            {
                foreach (var gameObj in gameObjects)
                    gameObj.Draw();
                gameObjects.Clear();
            }

            renderTargetStrategy.EndRender(_presentationRenderDone ? null : _renderTexture);
        }
    }

    private void SetupRenderTexture()
    {
        var screenSize = new PointInt(_presentationRenderTargetStrategy.RenderWidth, _presentationRenderTargetStrategy.RenderHeight);

        if (_renderTexture == null || _renderTexture.Value.Texture.Width != screenSize.X ||
            _renderTexture.Value.Texture.Height != screenSize.Y)
        {
            if (_renderTexture is not null)
                Raylib.UnloadRenderTexture(_renderTexture.Value);

            _renderTexture = Raylib.LoadRenderTexture(screenSize.X, screenSize.Y);
            Raylib.SetTextureFilter(_renderTexture.Value.Texture, TextureFilter.Point);
        }

        Raylib.BeginTextureMode(_renderTexture.Value);
        Raylib.ClearBackground(BackgroundColor);
        Raylib.EndTextureMode();
    }
}
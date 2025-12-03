using System.Diagnostics;
using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Camera;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Interfaces;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Renderer;

public class RenderService
{
    private readonly int _sceneLayers;
    private readonly int _gameObjectLayers;
    private readonly List<IRenderTargetStrategy> _renderTargetStrategies;
    public Color BackgroundColor { get; set; } = Color.Black;

    private Dictionary<IRenderTargetStrategy, List<List<List<BaseGameObject>>>> _gameObjects = new();
    private IRenderTargetStrategy _lastRenderTargetStrategy;
    
    private RenderTexture2D? _renderTexture = null;

    public RenderService(ObjectManager objectManager, int sceneLayers = 2, int gameObjectLayers = 16)
    {
        _sceneLayers = sceneLayers;
        _gameObjectLayers = gameObjectLayers;
        
        _renderTargetStrategies = objectManager.GetList<IRenderTargetStrategy>()!;
        
        _lastRenderTargetStrategy = objectManager.Get<IRenderTargetStrategy>("FINAL") ??
                                    new BasicScreenRenderTarget().SetFullScreen();
        
        foreach (var renderTargetStrategy in _renderTargetStrategies)
        {
            _gameObjects[renderTargetStrategy] = new List<List<List<BaseGameObject>>>();
            for (var i = 0; i < sceneLayers; i++)
            {
                _gameObjects[renderTargetStrategy].Add(new List<List<BaseGameObject>>());
                
                for (var j = 0; j < gameObjectLayers; j++)
                    _gameObjects[renderTargetStrategy][i].Add(new List<BaseGameObject>());
            }
        }
    }

    public void RegisterRender(BaseGameObject gameObject)
    {
        if (gameObject.Scene.Layer >= _sceneLayers)
            throw new Exception($"Scene layer out of bounds, scene layer: {gameObject.Scene.Layer} max: {_sceneLayers}");
        if (gameObject.Layer >= _gameObjectLayers)
            throw new Exception($"Game object layer out of bounds, scene layer: {gameObject.Layer} max: {_gameObjectLayers}");
        
        if (gameObject.RenderTarget == null)
            gameObject.RenderTarget = _renderTargetStrategies.First();

        if (!_gameObjects.ContainsKey(gameObject.RenderTarget))
        {
            Console.WriteLine(
                $"Render target is not registered. Render target: {gameObject.RenderTarget}, GameObject: {gameObject.GetType().FullName}");
            return;
        }

        _gameObjects[gameObject.RenderTarget][gameObject.Scene.Layer][gameObject.Layer].Add(gameObject);
    }

    public void Update(float deltaTime)
    {
        foreach (var renderer in _renderTargetStrategies)
            renderer.Camera?.Update(deltaTime, renderer);
    }
    
    public void Render()
    {
        SetupRenderTexture();

        var doneRender = false;
        
        foreach (var renderTargetStrategy in _renderTargetStrategies)
        {
            var lastRenderer = renderTargetStrategy == _lastRenderTargetStrategy;
            
            if (lastRenderer)
            {
                renderTargetStrategy.BeginRender(BackgroundColor);
                
                Raylib.DrawTexturePro(
                    _renderTexture!.Value.Texture,
                    new Rectangle(0, 0, _renderTexture.Value.Texture.Width, -_renderTexture.Value.Texture.Height),
                    new Rectangle(0, 0, _renderTexture.Value.Texture.Width, _renderTexture.Value.Texture.Height),
                    Vector2.Zero, 0f, Color.White
                );
                
                renderTargetStrategy.EndRender();
                break;
            } 
            
            renderTargetStrategy.BeginRender(new Color(0, 0, 0, 0));

            //if (!doneRender)
            //{
            //    doneRender = true;
                
                foreach (var layer in _gameObjects[renderTargetStrategy])
                foreach (var gameObjects in layer)
                {
                    foreach (var gameObj in gameObjects)
                        gameObj.Draw();
                    gameObjects.Clear();
                }
            //}

            renderTargetStrategy.EndRender(_renderTexture);
        }
    }

    private void SetupRenderTexture()
    {
        var screenSize = new PointInt(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());

        if (_renderTexture == null || _renderTexture.Value.Texture.Width != screenSize.X ||
            _renderTexture.Value.Texture.Height != screenSize.Y)
        {
            if (_renderTexture is not null)
                Raylib.UnloadRenderTexture(_renderTexture.Value);
            
            _renderTexture = Raylib.LoadRenderTexture(screenSize.X, screenSize.Y);
            Raylib.SetTextureFilter(_renderTexture.Value.Texture, TextureFilter.Point);
        }
        
        Raylib.ClearBackground(BackgroundColor);
        
        Raylib.BeginTextureMode(_renderTexture.Value);
        Raylib.ClearBackground(BackgroundColor);
        Raylib.EndTextureMode();
    }
}
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
    private readonly List<IRenderTargetStrategy> _renderTargetStrategies = new List<IRenderTargetStrategy>();
    public Color BackgroundColor { get; set; } = Color.Black;

    private List<List<List<BaseGameObject>>> _gameObjects = new();
    private List<List<List<BaseGameObject>>> _uiGameObjects = new();

    private RenderTexture2D? _renderTexture = null;

    public RenderService(ObjectManager objectManager, int sceneLayers = 2, int gameObjectLayers = 16)
    {
        _sceneLayers = sceneLayers;
        _gameObjectLayers = gameObjectLayers;
        _renderTargetStrategies.Add(objectManager.Get<IRenderTargetStrategy>() ?? new BasicScreenRenderTarget().SetFullScreen());
        _renderTargetStrategies.Add(objectManager.Get<IRenderTargetStrategy>("UI") ?? new BasicScreenRenderTarget().SetFullScreen());
        _renderTargetStrategies.Add(objectManager.Get<IRenderTargetStrategy>("FINAL") ?? new BasicScreenRenderTarget().SetFullScreen());
        
        for (var i = 0; i < sceneLayers; i++)
        {
            _gameObjects.Add(new List<List<BaseGameObject>>());
            _uiGameObjects.Add(new List<List<BaseGameObject>>());
            for (var j = 0; j < gameObjectLayers; j++)
            {
                _gameObjects[i].Add(new List<BaseGameObject>());
                _uiGameObjects[i].Add(new List<BaseGameObject>());
            }
        }
    }

    public void RegisterRender(BaseGameObject gameObject)
    {
        if (gameObject.Scene.Layer >= _sceneLayers)
            throw new Exception($"Scene layer out of bounds, scene layer: {gameObject.Scene.Layer} max: {_sceneLayers}");
        if (gameObject.Layer >= _gameObjectLayers)
            throw new Exception($"Game object layer out of bounds, scene layer: {gameObject.Layer} max: {_gameObjectLayers}");
        
        if (gameObject.Camera == CameraLayer.World)
            _gameObjects[gameObject.Scene.Layer][gameObject.Layer].Add(gameObject);
        else
            _uiGameObjects[gameObject.Scene.Layer][gameObject.Layer].Add(gameObject);
    }

    public void Update(float deltaTime)
    {
        foreach (var renderer in _renderTargetStrategies)
            renderer.Camera?.Update(deltaTime, renderer);
    }
    
    public void Render()
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
        
        //Raylib.BeginDrawing();
        
        Raylib.BeginTextureMode(_renderTexture.Value);
        Raylib.ClearBackground(BackgroundColor);
        Raylib.EndTextureMode();
        
        _renderTargetStrategies[0].BeginRender(BackgroundColor);
        foreach (var layer in _gameObjects)
        foreach (var gameObjects in layer)
        {
            foreach (var gameObj in gameObjects)
                gameObj.Draw();
            gameObjects.Clear();
        }

        _renderTargetStrategies[0].EndRender(_renderTexture);

        _renderTargetStrategies[1].BeginRender(BackgroundColor);
        foreach (var layer in _uiGameObjects)
        foreach (var gameObjects in layer)
        {
            foreach (var gameObj in gameObjects)
                gameObj.Draw();
            
            gameObjects.Clear();
        }
        _renderTargetStrategies[1].EndRender(_renderTexture);

        _renderTargetStrategies[2].BeginRender(BackgroundColor);
        
        Raylib.DrawTexturePro(
            _renderTexture.Value.Texture,
            new Rectangle(0, 0, _renderTexture.Value.Texture.Width, -_renderTexture.Value.Texture.Height),
            new Rectangle(0, 0, _renderTexture.Value.Texture.Width, _renderTexture.Value.Texture.Height),
            Vector2.Zero, 0f, Color.White
        );
        _renderTargetStrategies[2].EndRender();
        
        //Raylib.EndDrawing();
    }
}
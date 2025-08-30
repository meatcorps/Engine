using System.Diagnostics;
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
    private readonly ICamera _camera;
    private readonly IRenderTargetStrategy _renderTargetStrategy;
    public Color BackgroundColor { get; set; } = Color.Black;

    private List<List<List<BaseGameObject>>> _gameObjects = new();
    private List<List<List<BaseGameObject>>> _uiGameObjects = new();

    public RenderService(ObjectManager objectManager, int sceneLayers = 2, int gameObjectLayers = 16)
    {
        _sceneLayers = sceneLayers;
        _gameObjectLayers = gameObjectLayers;
        _camera = objectManager.Get<ICamera>() ?? new FallBackCamera();
        _renderTargetStrategy = objectManager.Get<IRenderTargetStrategy>() ?? new BasicScreenRenderTarget();
        
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
        _camera.Update(deltaTime, _renderTargetStrategy);
    }

    public void Render(FrameTimer.ScopedScope renderTimer)
    {
        _renderTargetStrategy.BeginRender(BackgroundColor, _camera);

        _camera.StartWorldCamera();

        foreach (var layer in _gameObjects)
        foreach (var gameObjects in layer)
        {
            foreach (var gameObj in gameObjects)
                gameObj.Draw();
            gameObjects.Clear();
        }

        _camera.EndWorldCamera();
        
        _renderTargetStrategy.PostProcess(CameraLayer.World);

        _camera.StartUICamera();
        
        foreach (var layer in _uiGameObjects)
        foreach (var gameObjects in layer)
        {
            foreach (var gameObj in gameObjects)
                gameObj.Draw();
            
            gameObjects.Clear();
        }

        _camera.EndUICamera();
        
        _renderTargetStrategy.PostProcess(CameraLayer.UI);

        _renderTargetStrategy.EndRender();
        renderTimer.Dispose();
        _renderTargetStrategy.EndDrawing();
    }
}
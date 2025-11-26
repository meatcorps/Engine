using Meatcorps.Engine.Core.Interfaces.Trackers;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Interfaces;

namespace Meatcorps.Engine.RayLib.Game.GameTasks;

public class SceneTask: IGameLoopTask
{
    private Guid _instanceId = Guid.NewGuid();
    public int Priority { get; } = 50;
    public bool Enabled { get; set; } = false;
    public bool IsInitialized { get; private set; }
    
    private BaseScene _scene;
    private GameHost _host;
    private BaseScene? _newSceneToLoad;

    public SceneTask(int priority = 50)
    {
        priority = 50;
    }
    
    public void SwitchScene(BaseScene scene)
    {
        _newSceneToLoad = scene;
        foreach (var service in GlobalObjectManager.ObjectManager.GetList<ISceneSwitchTracker>()!)
            service.OnActiveSceneSwitch(_newSceneToLoad);
    }
    
    public void Initialize(GameHost host)
    {
        IsInitialized = true;
        _host = host;
    }

    public void Task(GameLoopType type, float deltaTime)
    {
        if (type == GameLoopType.BeforeUpdate)
        {
            if (_newSceneToLoad is not null)
            {
                _newSceneToLoad.SetGameHost(_host);
                var currentScene = GlobalObjectManager.ObjectManager.Get<BaseScene>();
                if (currentScene != null)
                {
                    currentScene.Dispose();
                    GlobalObjectManager.ObjectManager.Remove<BaseScene>();
                } else 
                    _scene = currentScene!;
                
                GlobalObjectManager.ObjectManager.Register(_newSceneToLoad);
        
                _newSceneToLoad.Initialize();

                _newSceneToLoad = null;
            }
            return;
        }
        
        var activeScene = GlobalObjectManager.ObjectManager.Get<BaseScene>()!;
        
        if (type == GameLoopType.PreUpdate)
            activeScene.PreUpdate(deltaTime);

        if (type == GameLoopType.Update)
        {
            activeScene.Update(deltaTime);
            activeScene.AlwaysUpdate(deltaTime);
        }

        if (type == GameLoopType.LateUpdate)
        {
            activeScene.LateUpdate(deltaTime);
            _host.RenderService.Update(deltaTime);
        }

        if (type == GameLoopType.AfterUpdate)
        {
            activeScene.RegisterForRender();
        }
        
        if (type == GameLoopType.PreRender)
        {
            _host.RenderService.StartRenderer();
        }

        if (type == GameLoopType.Render)
        {
            activeScene.Draw();
            _host.RenderService.Render();
        }

        if (type == GameLoopType.PostRender)
        {
            _host.RenderService.StopRendering();
        }
    }
}
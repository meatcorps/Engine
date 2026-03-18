using Meatcorps.Engine.Core.Interfaces.Trackers;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Interfaces;
using Raylib_cs;
// ReSharper disable RedundantAssignment

namespace Meatcorps.Engine.RayLib.Game.GameTasks;

public class SceneTask : IGameLoopTask
{
    private GameHost _host = null!;
    private BaseScene? _newSceneToLoad;

    public SceneTask(int priority = 50)
    {
        Priority = priority;
    }

    public int Priority { get; }
    public bool Enabled { get; set; } = false;
    public bool IsInitialized { get; private set; }

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
                }

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
            activeScene.RegisterForRender();

        if (type == GameLoopType.PreRender)
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color(0, 0, 0, 0));
        }

        if (type == GameLoopType.Render)
        {
            activeScene.Draw();
            _host.RenderService.Render();
        }


        if (type == GameLoopType.PostRender) 
            Raylib.EndDrawing();
    }

    public void SwitchScene(BaseScene scene)
    {
        _newSceneToLoad = scene;
        foreach (var service in GlobalObjectManager.ObjectManager.GetList<ISceneSwitchTracker>()!)
            service.OnActiveSceneSwitch(_newSceneToLoad);
    }
}
using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Game;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Meatcorps.Engine.RayLib.Abstractions;

public abstract class BaseScene : IDisposable
{
    private readonly List<BaseGameObject> _gameObjectsToAdd = new();

    private readonly List<BaseGameObject> _gameObjectsToDispose = new();
    private readonly List<BaseScene> _subScenesToAdd = new();
    private readonly List<BaseScene> _subScenesToDispose = new();
    private bool _enabled = true;

    public BaseScene()
    {
        SceneObjectManager.Register(this);
        SceneObjectManager.RegisterSet<BaseScene>();
        SceneObjectManager.RegisterList<BaseGameObject>();
        SceneObjectManager.RegisterList<IBackgroundService>();
    }

    public GameHost GameHost { get; private set; } = null!;
    public int Layer { get; set; } = 0;
    public ObjectManager SceneObjectManager { get; } = new();
    public bool Paused { get; set; }

    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (_enabled == value)
                return;
            _enabled = value;
            if (_enabled)
                OnEnabled();
            else
                OnDisabled();
        }
    }

    public bool Visible { get; set; } = true;
    public float UpdateTimeMultiplier { get; set; } = 1;
    protected bool IsDisposed { get; private set; }

    public void Dispose()
    {
        if (IsDisposed) return;
        OnDispose();
        SceneObjectManager.Dispose();
        IsDisposed = true;
    }

    public void SetGameHost(GameHost gameHost)
    {
        GameHost = gameHost;
    }

    public void AddScene<T>(T scene) where T : BaseScene
    {
        _subScenesToAdd.Add(scene);
    }

    public void RemoveScene<T>(T scene) where T : BaseScene
    {
        _subScenesToDispose.Add(scene);
    }

    public T? GetScene<T>() where T : BaseScene
    {
        return SceneObjectManager.GetSet<BaseScene>()!.FirstOrDefault(x => x is T) as T;
    }

    public T AddGameObject<T>(T gameObject) where T : BaseGameObject
    {
        _gameObjectsToAdd.Add(gameObject);
        return gameObject;
    }

    public T? GetGameObject<T>() where T : BaseGameObject
    {
        return SceneObjectManager.GetList<BaseGameObject>()!.FirstOrDefault(x => x is T) as T;
    }

    public IEnumerable<T> GetGameObjects<T>() where T : BaseGameObject
    {
        return SceneObjectManager.GetList<BaseGameObject>()!.Where(x => x is T).Cast<T>();
    }

    public BaseGameObject? GetGameObjectByName(string name)
    {
        return SceneObjectManager.GetList<BaseGameObject>()!.FirstOrDefault(x => x.Name.Equals(name));
    }

    public IEnumerable<BaseGameObject> GetGameObjectsByName(string name)
    {
        return SceneObjectManager.GetList<BaseGameObject>()!.Where(x => x.Name.Equals(name));
    }

    public void RemoveGameObject<T>(T gameObject) where T : BaseGameObject
    {
        _gameObjectsToDispose.Add(gameObject);
    }

    public void Initialize()
    {
        OnInitialize();
    }

    protected virtual void OnEnabled()
    {
    }

    protected virtual void OnDisabled()
    {
    }

    public void PreUpdate(float deltaTime)
    {
        if (Paused || !Enabled)
            return;

        GameHost.SetMultiplier(UpdateTimeMultiplier);

        foreach (var backgroundService in SceneObjectManager.GetList<IBackgroundService>()!)
            backgroundService.PreUpdate(deltaTime);

        foreach (var scene in _subScenesToDispose)
        {
            scene.Dispose();
            SceneObjectManager.GetSet<BaseScene>()!.Remove(scene);
        }

        _subScenesToDispose.Clear();

        if (_subScenesToAdd.Count > 0)
            foreach (var scene in _subScenesToAdd.ToArray())
            {
                scene.SetGameHost(GameHost);
                scene.Initialize();
                SceneObjectManager.GetSet<BaseScene>()!.Add(scene);
                _subScenesToAdd.Remove(scene);
            }

        foreach (var gameObject in _gameObjectsToDispose)
        {
            gameObject.Dispose();
            SceneObjectManager.GetList<BaseGameObject>()!.Remove(gameObject);
        }

        _gameObjectsToDispose.Clear();

        if (_gameObjectsToAdd.Count > 0)
            foreach (var gameObject in _gameObjectsToAdd.ToArray())
            {
                gameObject.SetScene(this);
                gameObject.Initialize();
                SceneObjectManager.Add(gameObject);
                _gameObjectsToAdd.Remove(gameObject);
            }

        OnPreUpdate(deltaTime);

        foreach (var subScene in SceneObjectManager.GetSet<BaseScene>()!)
            subScene.PreUpdate(deltaTime);
        foreach (var gameObject in SceneObjectManager.GetList<BaseGameObject>()!)
            gameObject.PreUpdate(deltaTime);
    }

    public void Update(float deltaTime)
    {
        if (Paused || !Enabled)
            return;

        foreach (var backgroundService in SceneObjectManager.GetList<IBackgroundService>()!)
            backgroundService.Update(deltaTime);

        foreach (var subScene in SceneObjectManager.GetSet<BaseScene>()!)
            subScene.Update(deltaTime);
        foreach (var gameObject in SceneObjectManager.GetList<BaseGameObject>()!)
            gameObject.Update(deltaTime);

        OnUpdate(deltaTime);
    }

    public void AlwaysUpdate(float deltaTime)
    {
        foreach (var subScene in SceneObjectManager.GetSet<BaseScene>()!)
            subScene.AlwaysUpdate(deltaTime);
        foreach (var gameObject in SceneObjectManager.GetList<BaseGameObject>()!)
            gameObject.AlwaysUpdate(deltaTime);

        OnAlwaysUpdate(deltaTime);
    }

    public void LateUpdate(float deltaTime)
    {
        if (Paused || !Enabled)
            return;

        foreach (var backgroundService in SceneObjectManager.GetList<IBackgroundService>()!)
            backgroundService.LateUpdate(deltaTime);

        foreach (var subScene in SceneObjectManager.GetSet<BaseScene>()!)
            subScene.LateUpdate(deltaTime);
        foreach (var gameObject in SceneObjectManager.GetList<BaseGameObject>()!)
            gameObject.LateUpdate(deltaTime);

        OnLateUpdate(deltaTime);
    }

    public void RegisterForRender()
    {
        if (Visible && Enabled)
        {
            foreach (var subScene in SceneObjectManager.GetSet<BaseScene>()!)
                subScene.RegisterForRender();
            foreach (var gameObject in SceneObjectManager.GetList<BaseGameObject>()!)
                gameObject.RegisterForRender();
        }
    }

    public void Draw()
    {
        if (!Visible || !Enabled)
            return;

        foreach (var subScene in SceneObjectManager.GetSet<BaseScene>()!)
            subScene.Draw();

        OnDraw();
    }

    protected abstract void OnInitialize();

    protected virtual void OnPreUpdate(float deltaTime)
    {
    }

    protected abstract void OnUpdate(float deltaTime);

    protected virtual void OnAlwaysUpdate(float deltaTime)
    {
    }

    protected virtual void OnLateUpdate(float deltaTime)
    {
    }

    protected virtual void OnDraw()
    {
    }

    protected abstract void OnDispose();
}
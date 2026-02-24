using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Game;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Meatcorps.Engine.RayLib.Abstractions;

/// <summary>
/// Abstract base class for all game scenes.
/// Manages game objects, sub-scenes, and background services within a scoped <see cref="ObjectManager"/>.
/// Game objects and sub-scenes are added and removed via deferred queues, which are processed at the
/// start of each <see cref="PreUpdate"/> to avoid mutation during iteration.
/// </summary>
public abstract class BaseScene : IDisposable
{
    private readonly Queue<BaseGameObject> _gameObjectsToAdd = new();

    private readonly Queue<BaseGameObject> _gameObjectsToDispose = new();
    private readonly Queue<BaseScene> _subScenesToAdd = new();
    private readonly Queue<BaseScene> _subScenesToDispose = new();
    private bool _enabled = true;

    public BaseScene()
    {
        SceneObjectManager.Register(this);
        SceneObjectManager.RegisterSet<BaseScene>();
        SceneObjectManager.RegisterList<BaseGameObject>();
        SceneObjectManager.RegisterList<IBackgroundService>();
    }

    /// <summary>The <see cref="GameHost"/> that owns this scene. Assigned before <see cref="Initialize"/> is called.</summary>
    public GameHost GameHost { get; private set; } = null!;

    /// <summary>Draw and update ordering layer. Scenes with lower values are processed first.</summary>
    public int Layer { get; set; } = 0;

    /// <summary>Scoped DI container for this scene's registered services and objects.</summary>
    public ObjectManager SceneObjectManager { get; } = new();

    /// <summary>
    /// When <c>true</c>, <see cref="PreUpdate"/>, <see cref="Update"/>, and <see cref="LateUpdate"/>
    /// are skipped for this scene and all its children.
    /// </summary>
    public bool Paused { get; set; }

    /// <summary>
    /// Controls whether this scene is active. Setting to <c>false</c> skips all updates and rendering
    /// and triggers <see cref="OnDisabled"/>. Setting back to <c>true</c> triggers <see cref="OnEnabled"/>.
    /// </summary>
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

    /// <summary>When <c>false</c>, the scene and all its children are not drawn.</summary>
    public bool Visible { get; set; } = true;

    /// <summary>Scales the delta time passed to this scene's update loop. Default is <c>1.0</c> (real time).</summary>
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

    /// <summary>
    /// Enqueues a sub-scene for addition. It will be initialized and added at the start of the next <see cref="PreUpdate"/>.
    /// </summary>
    public void AddScene<T>(T scene) where T : BaseScene
    {
        _subScenesToAdd.Enqueue(scene);
    }

    /// <summary>
    /// Enqueues a sub-scene for removal and disposal at the start of the next <see cref="PreUpdate"/>.
    /// </summary>
    public void RemoveScene<T>(T scene) where T : BaseScene
    {
        _subScenesToDispose.Enqueue(scene);
    }

    /// <summary>Returns the first registered sub-scene of type <typeparamref name="T"/>, or <c>null</c> if not found.</summary>
    public T? GetScene<T>() where T : BaseScene
    {
        return SceneObjectManager.GetSet<BaseScene>()!.FirstOrDefault(x => x is T) as T;
    }

    /// <summary>
    /// Enqueues a game object for addition. It will be initialized and added at the start of the next <see cref="PreUpdate"/>.
    /// </summary>
    /// <returns>The same game object, for fluent chaining.</returns>
    public T AddGameObject<T>(T gameObject) where T : BaseGameObject
    {
        _gameObjectsToAdd.Enqueue(gameObject);
        return gameObject;
    }

    /// <summary>Returns the first game object of type <typeparamref name="T"/> in this scene, or <c>null</c> if not found.</summary>
    public T? GetGameObject<T>() where T : BaseGameObject
    {
        return SceneObjectManager.GetList<BaseGameObject>()!.FirstOrDefault(x => x is T) as T;
    }

    /// <summary>Returns all game objects of type <typeparamref name="T"/> currently in this scene.</summary>
    public IEnumerable<T> GetGameObjects<T>() where T : BaseGameObject
    {
        return SceneObjectManager.GetList<BaseGameObject>()!.Where(x => x is T).Cast<T>();
    }

    /// <summary>Returns the first game object whose <see cref="BaseGameObject.Name"/> matches, or <c>null</c> if not found.</summary>
    public BaseGameObject? GetGameObjectByName(string name)
    {
        return SceneObjectManager.GetList<BaseGameObject>()!.FirstOrDefault(x => x.Name.Equals(name));
    }

    /// <summary>Returns all game objects whose <see cref="BaseGameObject.Name"/> matches the given name.</summary>
    public IEnumerable<BaseGameObject> GetGameObjectsByName(string name)
    {
        return SceneObjectManager.GetList<BaseGameObject>()!.Where(x => x.Name.Equals(name));
    }

    /// <summary>
    /// Enqueues a game object for removal and disposal at the start of the next <see cref="PreUpdate"/>.
    /// </summary>
    public void RemoveGameObject<T>(T gameObject) where T : BaseGameObject
    {
        _gameObjectsToDispose.Enqueue(gameObject);
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

        while (_subScenesToAdd.TryDequeue(out var scene))
        {
            scene.SetGameHost(GameHost);
            scene.Initialize();
            SceneObjectManager.GetSet<BaseScene>()!.Add(scene);
        }
        
        while (_subScenesToDispose.TryDequeue(out var scene))
        {
            scene.Dispose();
            SceneObjectManager.GetSet<BaseScene>()!.Remove(scene);
        }
        
        while (_gameObjectsToDispose.TryDequeue(out var gameObject))
        {
            gameObject.Dispose();
            SceneObjectManager.GetList<BaseGameObject>()!.Remove(gameObject);
        }

        while (_gameObjectsToAdd.TryDequeue(out var gameObject))
        {
            gameObject.SetScene(this);
            gameObject.Initialize();
            SceneObjectManager.Add(gameObject);
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

    /// <summary>Override to initialize scene state, spawn initial objects, and register dependencies.</summary>
    protected abstract void OnInitialize();

    /// <summary>Override for pre-update logic specific to this scene. Called after deferred adds/removes are processed.</summary>
    protected virtual void OnPreUpdate(float deltaTime)
    {
    }

    /// <summary>Override for the main per-tick update logic of this scene.</summary>
    protected abstract void OnUpdate(float deltaTime);

    /// <summary>Override for logic that must run every tick regardless of <see cref="Paused"/> or <see cref="Enabled"/> state.</summary>
    protected virtual void OnAlwaysUpdate(float deltaTime)
    {
    }

    /// <summary>Override for logic that reacts to state changes made during <see cref="OnUpdate"/>.</summary>
    protected virtual void OnLateUpdate(float deltaTime)
    {
    }

    /// <summary>Override to issue scene-level draw calls. Called after all game objects have drawn.</summary>
    protected virtual void OnDraw()
    {
    }

    /// <summary>Override to release scene-specific resources. Called once during <see cref="Dispose"/>.</summary>
    protected abstract void OnDispose();
}
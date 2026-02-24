using System.Numerics;
using Meatcorps.Engine.Core.Interfaces.Components;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Interfaces;
// ReSharper disable SuspiciousTypeConversion.Global

namespace Meatcorps.Engine.RayLib.Abstractions;

/// <summary>
/// Abstract base class for all game objects managed by a <see cref="BaseScene"/>.
/// Supports the component pattern via <see cref="AddComponent{T}"/>.
/// Components are added and removed via deferred queues processed during <see cref="PreUpdate"/>.
/// </summary>
public abstract class BaseGameObject : IDisposable
{
    private readonly List<IGameComponent> _components = new();
    private readonly Queue<IGameComponent> _toComponentAdd = new();
    private readonly Queue<IGameComponent> _toComponentRemove = new();

    private CameraLayer _cameraLayer = CameraLayer.Other;
    private bool _enabled = true;
    private bool _visible = true;

    protected BaseGameObject()
    {
        Camera = CameraLayer.World;
    }

    /// <summary>World-space position of this object.</summary>
    public Vector2 Position { get; protected set; }

    /// <summary>Identifier for this object. Used by <see cref="BaseScene.GetGameObjectByName"/>.</summary>
    public string Name { get; set; } = "GameObject";

    /// <summary>Draw ordering layer within the scene. Higher values are drawn on top.</summary>
    public int Layer { get; set; }

    /// <summary>
    /// Determines which camera layer this object renders on.
    /// Setting this automatically assigns the appropriate <see cref="RenderTarget"/> from the global registry.
    /// </summary>
    public CameraLayer Camera
    {
        get => _cameraLayer;
        set
        {
            if (_cameraLayer == value)
                return;

            _cameraLayer = value;

            switch (_cameraLayer)
            {
                case CameraLayer.World:
                    RenderTarget = GlobalObjectManager.ObjectManager.Get<IRenderTargetStrategy>()!;
                    break;
                case CameraLayer.UI:
                    RenderTarget = GlobalObjectManager.ObjectManager.Get<IRenderTargetStrategy>("UI")!;
                    break;
            }
        }
    }

    /// <summary>
    /// The render target this object draws into. Automatically assigned based on <see cref="Camera"/>.
    /// Can be overridden manually if custom render target routing is needed.
    /// </summary>
    public IRenderTargetStrategy? RenderTarget { get; set; }

    /// <summary>The scene this object belongs to. Assigned by the scene during addition.</summary>
    public BaseScene Scene { get; private set; } = null!;

    /// <summary>
    /// Controls whether this object receives updates. Setting to <c>false</c> skips all update methods
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

    /// <summary>
    /// Controls whether this object is drawn. Setting to <c>false</c> triggers <see cref="OnHidden"/>.
    /// Setting back to <c>true</c> triggers <see cref="OnVisible"/>.
    /// </summary>
    public bool Visible
    {
        get => _visible;
        set
        {
            if (_visible == value)
                return;
            _visible = value;
            if (_visible)
                OnVisible();
            else
                OnHidden();
        }
    }

    protected bool IsDisposed { get; private set; }

    public void Dispose()
    {
        if (IsDisposed)
            return;


        foreach (var component in _components)
            if (component is IDisposable disposable)
                disposable.Dispose();
        _toComponentAdd.Clear();
        _toComponentRemove.Clear();
        _components.Clear();
        OnDispose();
        IsDisposed = true;
    }

    public void SetScene(BaseScene scene)
    {
        Scene = scene;
    }

    public void Initialize()
    {
        OnInitialize();
    }

    /// <summary>
    /// Enqueues a component for addition. It will be initialized and added during the next <see cref="PreUpdate"/>.
    /// If the component implements <see cref="IRaylibGameComponent"/>, its owner is set immediately.
    /// </summary>
    /// <returns>The component, for fluent chaining.</returns>
    public T AddComponent<T>(T component) where T : IGameComponent
    {
        _toComponentAdd.Enqueue(component);

        if (component is IRaylibGameComponent raylibGameComponent)
            raylibGameComponent.SetOwner(this);

        return component;
    }

    /// <summary>Attempts to retrieve the first component of type <typeparamref name="T"/>.</summary>
    /// <param name="component">The found component, or <c>null</c> if not present.</param>
    /// <returns><c>true</c> if a matching component was found.</returns>
    public bool TryGetComponent<T>(out T? component) where T : IGameComponent
    {
        component = (T?)_components.FirstOrDefault(x => x is T);
        return component != null;
    }

    /// <summary>Returns all components of type <typeparamref name="T"/> attached to this object.</summary>
    public IEnumerable<T> GetComponents<T>() where T : IGameComponent
    {
        return _components.Where(x => x is T).Cast<T>();
    }

    /// <summary>Enqueues a component for removal. It will be removed during the next <see cref="PreUpdate"/>.</summary>
    public void RemoveComponent(IGameComponent component)
    {
        _toComponentRemove.Enqueue(component);
    }

    public void PreUpdate(float deltaTime)
    {
        if (!Enabled)
            return;

        OnPreUpdate(deltaTime);

        while (_toComponentAdd.TryDequeue(out var component))
        {
            _components.Add(component);
            component.Initialize();
        }

        while (_toComponentRemove.TryDequeue(out var component))
            _components.Remove(component);

        foreach (var component in _components)
            component.PreUpdate(deltaTime);
    }

    public void Update(float deltaTime)
    {
        if (!Enabled)
            return;

        OnUpdate(deltaTime);

        foreach (var component in _components)
            component.Update(deltaTime);
    }

    public void AlwaysUpdate(float deltaTime)
    {
        OnAlwaysUpdate(deltaTime);
    }

    public void LateUpdate(float deltaTime)
    {
        if (!Enabled)
            return;
        
        OnLateUpdate(deltaTime);

        foreach (var component in _components)
            component.LateUpdate(deltaTime);
    }

    public void Draw()
    {
        OnDraw();
    }

    /// <summary>Override to initialize this object's state and resolve dependencies from <see cref="Scene"/>.</summary>
    protected abstract void OnInitialize();

    /// <summary>Override for pre-update logic. Called before component updates each tick.</summary>
    protected virtual void OnPreUpdate(float deltaTime)
    {
    }

    /// <summary>Override for the main per-tick update logic of this object.</summary>
    protected abstract void OnUpdate(float deltaTime);

    /// <summary>Override for logic that must run every tick regardless of <see cref="Enabled"/> state.</summary>
    protected virtual void OnAlwaysUpdate(float deltaTime)
    {
    }

    /// <summary>Override for logic that reacts to state changes made during <see cref="OnUpdate"/>.</summary>
    protected virtual void OnLateUpdate(float deltaTime)
    {
    }

    public void RegisterForRender()
    {
        if (Visible && Enabled)
            Scene.GameHost.RenderService.RegisterRender(this);
    }

    /// <summary>Called when <see cref="Enabled"/> transitions from <c>false</c> to <c>true</c>.</summary>
    protected virtual void OnEnabled()
    {
    }

    /// <summary>Called when <see cref="Enabled"/> transitions from <c>true</c> to <c>false</c>.</summary>
    protected virtual void OnDisabled()
    {
    }

    /// <summary>Called when <see cref="Visible"/> transitions from <c>false</c> to <c>true</c>.</summary>
    protected virtual void OnVisible()
    {
    }

    /// <summary>Called when <see cref="Visible"/> transitions from <c>true</c> to <c>false</c>.</summary>
    protected virtual void OnHidden()
    {
    }

    /// <summary>Override to issue custom draw calls. By default draws all attached components.</summary>
    protected virtual void OnDraw()
    {
        foreach (var component in _components)
            component.Draw();
    }

    /// <summary>Override to release object-specific resources. Called once during <see cref="Dispose"/>.</summary>
    protected abstract void OnDispose();
}
using System.Numerics;
using Meatcorps.Engine.Core.Interfaces.Components;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Profiler;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.Renderer;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Meatcorps.Engine.RayLib.Abstractions;

/// <summary>
/// Abstract base class for all game objects managed by a <see cref="BaseScene"/>.
/// Supports the component pattern via <see cref="AddComponent{T}"/>.
/// Components are added and removed via deferred queues processed during <see cref="PreUpdate"/>.
/// </summary>
public abstract class BaseGameObject : IRenderer, IDisposable
{
    private static int _idCounter = 0;
    private readonly List<IGameComponent> _components = new();
    private readonly Queue<IGameComponent> _toComponentAdd = new();
    private readonly Queue<IGameComponent> _toComponentRemove = new();
    private string _id;
    private CameraLayer _cameraLayer = CameraLayer.Other;
    private bool _enabled = true;
    private bool _visible = true;
    private readonly Type _type;

    protected BaseGameObject()
    {
        Camera = CameraLayer.World;
        _type = GetType();
        _id = _idCounter.ToString();
        _idCounter++;
        GAMEOBJECT_INITIALIZE += $"[{_id}]";
        GAMEOBJECT_PREUPDATE += $"[{_id}]";
        GAMEOBJECT_UPDATE += $"[{_id}]";
        GAMEOBJECT_LATEUPDATE += $"[{_id}]";
        GAMEOBJECT_DRAW += $"[{_id}]";
    }

    /// <summary>World-space position of this object.</summary>
    public Vector2 Position { get; protected set; }

    /// <summary>Identifier for this object. Used by <see cref="BaseScene.GetGameObjectByName"/>.</summary>
    public string Name { get; set; } = "GameObject";

    /// <summary>Draw ordering layer within the scene. Higher values are drawn on top.</summary>
    public int Layer { get; set; }

    public int SceneLayer => Scene.Layer;

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
    
    private string GAMEOBJECT_INITIALIZE = "GameObject.Initialize";
    public void Initialize()
    {
        using (Profiler.Instance.StartProfile(_type, GAMEOBJECT_INITIALIZE))
        {
            OnInitialize();
        }
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
        foreach (var t in _components)
        {
            if (t is not T match) 
                continue;
            
            component = match;
            return true;
        }
        component = default;
        return false;
    }

    /// <summary>
    /// Executes the specified action on each component of the specified type within the object.
    /// </summary>
    /// <typeparam name="T">The type of components to retrieve.</typeparam>
    /// <typeparam name="TAction">The type of the action to be executed on the components.</typeparam>
    /// <param name="action">The action instance with the logic to be executed for each matching component.</param>
    public void GetComponents<T, TAction>(ref TAction action)
        where T : IGameComponent
        where TAction : struct, IComponentAction<T>
    {
        foreach (var t in _components)
            if (t is T match)
                action.Execute(match);
    }

    /// <summary>Enqueues a component for removal. It will be removed during the next <see cref="PreUpdate"/>.</summary>
    public void RemoveComponent(IGameComponent component)
    {
        _toComponentRemove.Enqueue(component);
    }

    
    private string GAMEOBJECT_PREUPDATE = "GameObject.PreUpdate";
    public void PreUpdate(float deltaTime)
    {
        if (!Enabled)
            return;

        using (Profiler.Instance.StartProfile(_type, GAMEOBJECT_PREUPDATE))
        {
            OnPreUpdate(deltaTime);
        }

        while (_toComponentAdd.TryDequeue(out var component))
        {
            _components.Add(component);
            using (Profiler.Instance.StartProfile(_type, GAMEOBJECT_INITIALIZE, component.GetType()))
            {
                component.Initialize();
            }

            foreach (var other in _components)
            {
                if (other == component)
                    continue;
                
                if (other is IGameComponentAddedRemoved addedRemoved)
                    addedRemoved.OnAdded(component);
            }
        }

        while (_toComponentRemove.TryDequeue(out var component))
        {
            _components.Remove(component);
            
            foreach (var other in _components)
            {
                if (other is IGameComponentAddedRemoved addedRemoved)
                    addedRemoved.OnRemoved(component);
            }
        }

        foreach (var component in _components)
        {
            using (Profiler.Instance.StartProfile(_type, GAMEOBJECT_INITIALIZE, component.GetType()))
            {
                component.PreUpdate(deltaTime);
            }
        }
    }

    private string GAMEOBJECT_UPDATE = "GameObject.Update";
    public void Update(float deltaTime)
    {
        if (!Enabled)
            return;
        
        using (Profiler.Instance.StartProfile(_type, GAMEOBJECT_UPDATE))
        {
            OnUpdate(deltaTime);
        }

        foreach (var component in _components)
        {
            using (Profiler.Instance.StartProfile(_type, GAMEOBJECT_UPDATE, component.GetType()))
            {
                component.Update(deltaTime);
            }
        }
    }

    private string GAMEOBJECT_ALWAYSUPDATE = "GameObject.AlwaysUpdate";
    public void AlwaysUpdate(float deltaTime)
    {
        using (Profiler.Instance.StartProfile(_type, GAMEOBJECT_ALWAYSUPDATE))
        {
            OnAlwaysUpdate(deltaTime);
        }
    }

    private string GAMEOBJECT_LATEUPDATE = "GameObject.LateUpdate";
    public void LateUpdate(float deltaTime)
    {
        if (!Enabled)
            return;

        using (Profiler.Instance.StartProfile(_type, GAMEOBJECT_LATEUPDATE))
        {
            OnLateUpdate(deltaTime);
        }

        foreach (var component in _components)
        {
            using (Profiler.Instance.StartProfile(_type, GAMEOBJECT_LATEUPDATE, component.GetType()))
            {
                component.LateUpdate(deltaTime);
            }
        }
    }

    private string GAMEOBJECT_DRAW = "GameObject.Draw";
    public void Draw()
    {
        using (Profiler.Instance.StartProfile(_type, GAMEOBJECT_DRAW))
        {
            OnDraw();
        }
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
using System.Numerics;
using Meatcorps.Engine.Core.Interfaces.Components;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Interfaces;

namespace Meatcorps.Engine.RayLib.Abstractions;

public abstract class BaseGameObject: IDisposable
{
    private bool _enabled = true;
    private bool _visible = true;
    public Vector2 Position { get; protected set; }
    public string Name { get; set; } = "GameObject";
    public int Layer { get; set; } = 0;
    
    private CameraLayer _cameraLayer = CameraLayer.Other;

    public CameraLayer Camera
    {
        get => _cameraLayer; 
        set {
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

    public IRenderTargetStrategy? RenderTarget { get; set; }
    
    public BaseScene Scene { get; private set; }
    
    private readonly List<IGameComponent> _components = new();
    private readonly Queue<IGameComponent> _toComponentAdd = new();
    private readonly Queue<IGameComponent> _toComponentRemove = new();

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

    public bool Visible
    {
        get => _visible;
        set
        {
            if (_visible == value)
                return;
            _visible = value;
            if (_enabled) 
                OnVisible();
            else
                OnHidden();
        }
    }
    protected bool IsDisposed { get; private set; }

    public BaseGameObject()
    {
        Camera = CameraLayer.World;
    }
    
    public void SetScene(BaseScene scene)
    {
        Scene = scene;
    }

    public void Initialize()
    {
        OnInitialize();
    }
    
    public T AddComponent<T>(T component) where T : IGameComponent
    {
        _toComponentAdd.Enqueue(component);
        
        if (component is IRaylibGameComponent raylibGameComponent)
            raylibGameComponent.SetOwner(this);

        return component;
    }
    
    public bool TryGetComponent<T>(out T? component) where T : IGameComponent
    {
        component = (T?)_components.FirstOrDefault(x => x is T);
        return component != null;
    }

    public IEnumerable<T> GetComponents<T>() where T : IGameComponent
    {
        return _components.Where(x => x is T).Cast<T>();
    }
    
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
        if (Enabled) 
            OnLateUpdate(deltaTime);
        
        foreach (var component in _components)
            component.LateUpdate(deltaTime);
    }

    public void Draw()
    {
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

    public void RegisterForRender()
    {
        if (Visible && Enabled) 
            Scene.GameHost.RenderService.RegisterRender(this);
    }
    
    protected virtual void OnEnabled()
    {
        
    }
    
    protected virtual void OnDisabled()
    {
        
    }
    
    protected virtual void OnVisible()
    {
        
    }
    
    protected virtual void OnHidden()
    {
        
    }

    protected virtual void OnDraw()
    {
        foreach (var component in _components)
            component.Draw();
    }
    
    protected abstract void OnDispose();

    public void Dispose()
    {
        if (IsDisposed) 
            return;


        foreach (var component in _components)
        {
            if (component is IDisposable disposable)
                disposable.Dispose();
        } 
        _toComponentAdd.Clear();
        _toComponentRemove.Clear();
        _components.Clear();
        OnDispose();
        IsDisposed = true;
    }
}
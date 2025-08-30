using System.Numerics;
using Meatcorps.Engine.RayLib.Enums;

namespace Meatcorps.Engine.RayLib.Abstractions;

public abstract class BaseGameObject: IDisposable
{
    private bool _enabled = true;
    private bool _visible = true;
    public Vector2 Position { get; protected set; }
    public string Name { get; set; } = "GameObject";
    public int Layer { get; set; } = 0;
    public CameraLayer Camera { get; set; } = CameraLayer.World;
    public BaseScene Scene { get; private set; }
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

    public void SetScene(BaseScene scene)
    {
        Scene = scene;
    }

    public void Initialize()
    {
        OnInitialize();
    }

    public void PreUpdate(float deltaTime)
    {
        if (!Enabled) 
            return;
        
        OnPreUpdate(deltaTime);
    }

    public void Update(float deltaTime)
    {
        if (!Enabled) 
            return;
        
        OnUpdate(deltaTime);
    }

    public void AlwaysUpdate(float deltaTime)
    {
        OnAlwaysUpdate(deltaTime);
    }

    public void LateUpdate(float deltaTime)
    {
        if (Enabled) 
            OnLateUpdate(deltaTime);
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
        
    }
    
    protected abstract void OnDispose();

    public void Dispose()
    {
        if (IsDisposed) return;
        OnDispose();
        IsDisposed = true;
    }
}
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Abstractions;

namespace Meatcorps.Engine.RayLib.ImGuiTools;

public abstract class BaseImGuiTool: IDisposable
{
    private bool _disposed;
    public bool Enabled { get; set; } = true;
    public BaseScene? Scene { get; set; }
    
    private Action _unRegisterAction = () => { };
    
    public void SetUnregisterAction(Action action) => _unRegisterAction = action;
    
    public BaseImGuiTool()
    {
        GlobalObjectManager.ObjectManager.Get<ImGuiManager>()!.Register(this);
    }

    public virtual void Initialize()
    {
        
    }
    

    public virtual void Update(float deltaTime)
    {
        if (Enabled)
            DoUpdate(deltaTime);
    }

    protected virtual void DoUpdate(float deltaTime)
    {
        
    }

    public void Draw(float deltaTime)
    {
        if (Enabled)
            DoDraw(deltaTime);
    }

    public void NonImGuiDraw()
    {
        if (Enabled)
            OnNonImGuiDraw();
    }
    
    protected virtual void DoDraw(float deltaTime)
    {
          
    } 
    
    protected virtual void OnNonImGuiDraw()
    {
          
    } 

    protected virtual void OnDispose()
    {
    }

    public void Dispose()
    {
        if (_disposed) 
            return;
        
        OnDispose();
        GlobalObjectManager.ObjectManager.Get<ImGuiManager>()!.Unregister(this);
        _unRegisterAction();
        _disposed = true;
    }
}

public static class BaseSceneExtensions
{
    public static T AddImGuiUniqueTool<T>(this BaseScene scene, T instance) where T : BaseImGuiTool
    {
        instance.Scene = scene;
        scene.SceneObjectManager.Register<T>(instance);
        instance.SetUnregisterAction(() => scene.SceneObjectManager.Remove<T>());
        return instance;
    }
    
    public static T AddImGuiTool<T>(this BaseScene scene, T instance) where T : BaseImGuiTool
    {
        instance.Scene = scene;
        scene.SceneObjectManager.Add<IDisposable>(instance);
        instance.SetUnregisterAction(() => scene.SceneObjectManager.Remove<IDisposable>(instance));
        return instance;
    }
}
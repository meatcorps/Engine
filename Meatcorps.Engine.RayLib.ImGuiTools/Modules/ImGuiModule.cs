using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.ImGuiTools.Controllers;
using Meatcorps.Engine.RayLib.ImGuiTools.Interfaces;
using Meatcorps.Engine.RayLib.Interfaces;

namespace Meatcorps.Engine.RayLib.ImGuiTools.Modules;

public class ImGuiModule
{
    private static bool _setupCalled;
    public static ImGuiModule Setup(string targetRenderTarget = "UI", 
        bool darkMode = true, 
        int sceneLayer = 1, 
        int layer = 2)
    {
        if (!_setupCalled)
        {
            GlobalObjectManager.ObjectManager.RegisterList<IGameLoopTask>();
            GlobalObjectManager.ObjectManager.RegisterList<IResourceLoadOnInit>();
            GlobalObjectManager.ObjectManager.RegisterList<IImGuiInitializeModule>();
            GlobalObjectManager.ObjectManager.RegisterOnce(new ImGuiManager(targetRenderTarget, darkMode, sceneLayer,
                layer));
            GlobalObjectManager.ObjectManager.Add<IBackgroundService>(GlobalObjectManager.ObjectManager.Get<ImGuiManager>()!);
            GlobalObjectManager.ObjectManager.Add<IGameLoopTask>(new ImGuiRenderTask());
            _setupCalled = true;
        }

        return new ImGuiModule();
    }

    private ImGuiModule()
    {
        // Do nothing
    }
    
    public ImGuiModule RegisterFont<T>(T font, int size = 16) where T : Enum
    {
        var instance = GlobalObjectManager.ObjectManager.Get<ImGuiTextManager<T>>();
        if (instance == null)
        {
            instance = new ImGuiTextManager<T>();
            GlobalObjectManager.ObjectManager.RegisterOnce(instance);
            GlobalObjectManager.ObjectManager.Add<IResourceLoadOnInit>(instance);
        }
        
        RegisterInitModule(new ImGuiGenericInitialize(io =>
        {
            GlobalObjectManager.ObjectManager.Get<ImGuiTextManager<T>>()!.RequireFont(io, font, size);
        }));
        
        return this;
    }

    public ImGuiModule RegisterInitModule(IImGuiInitializeModule module)
    {
        GlobalObjectManager.ObjectManager.GetList<IImGuiInitializeModule>()!.Add(module);
        return this;
    }

    public ImGuiModule RegisterUniqueGlobalTool(BaseImGuiTool tool)
    {
        GlobalObjectManager.ObjectManager.Register(tool);
        return this;
    }
    
    public ImGuiModule RegisterGlobalTool(BaseImGuiTool tool)
    {
        GlobalObjectManager.ObjectManager.Add<IDisposable>(tool);
        return this;
    }
}
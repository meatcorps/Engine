using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.Modules;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Storage.Services;
using Meatcorps.Engine.Logging.Module;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Audio;
using Meatcorps.Engine.RayLib.Camera;
using Meatcorps.Engine.RayLib.Game;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.Renderer;
using Microsoft.Extensions.Logging;

namespace Meatcorps.Engine.RayLib.Modules;

public class RayLibModule
{
    private int _initialWidth = 1280;
    private int _initialHeight = 720;
    private string _title = "Meatcorps Engine";
    private int _fps = 60;
    private ICamera? _camera;
    private IRenderTargetStrategy? _renderTargetStrategy;
    private IUniversalConfig _config;
    
    public static RayLibModule Setup()
    {
        GlobalObjectManager.ObjectManager.RegisterList<ILoadAfterRayLibInit>();
        GlobalObjectManager.ObjectManager.RegisterList<IPostProcessor>();
        return new RayLibModule();
    }

    public RayLibModule()
    {
        _config = GlobalObjectManager.ObjectManager.Get<IUniversalConfig>() ?? new FallbackConfig();
    }

    public RayLibModule SetInitialSize(int width, int height)
    {
        _initialWidth = _config.GetOrDefault("Graphics", "WindowWidth", width);
        _initialHeight = _config.GetOrDefault("Graphics", "WindowHeight", height);
        return this;
    }
    
    public RayLibModule SetTitle(string title)
    {
        _title = title;
        return this;
    }

    public RayLibModule SetCustomCamera(ICamera camera)
    {
        _camera = camera;
        return this;
    }

    public RayLibModule SetFixedSizeCamera(int targetWidth, int targetHeight, bool pixelPerfect = true)
    {
        #if DEBUG
        if (!_config.GetOrDefault("Debug", "SetFixedSizeCamera", true))
            return this;
        
        targetWidth = _config.GetOrDefault("Debug", "SetFixedSizeCamera_TargetWidth", targetWidth);
        targetHeight = _config.GetOrDefault("Debug", "SetFixedSizeCamera_TargetHeight", targetHeight);
        pixelPerfect = _config.GetOrDefault("Debug", "SetFixedSizeCamera_PixelPerfect", pixelPerfect);
        
        #endif
        if (pixelPerfect)
            _renderTargetStrategy = new PixelPerfectRenderTarget(targetWidth, targetHeight);
        
        _camera = new FixedSizeCamera(targetWidth, targetHeight);
        return this;
    }

    public RayLibModule SetResource<T>(T instance) where T : class, ILoadAfterRayLibInit
    {
#if DEBUG
        if (!_config.GetOrDefault("Debug", "SetResource_" + instance.GetType().Name, true))
            return this;
#endif
        GlobalObjectManager.ObjectManager.Add<ILoadAfterRayLibInit>(instance);
        GlobalObjectManager.ObjectManager.Register(instance);
        return this;
    }

    public RayLibModule SetProcessing<T>(T postProcessor) where T : class, IPostProcessor
    {
#if DEBUG
        if (!_config.GetOrDefault("Debug", "SetProcessing_" + postProcessor.GetType().Name, true))
            return this;
#endif
        GlobalObjectManager.ObjectManager.Add<IPostProcessor>(postProcessor);
        GlobalObjectManager.ObjectManager.Register<T>(postProcessor);
        return this;
    }
    
    public RayLibModule Load<T>(T scene) where T : BaseScene
    {
        if (_renderTargetStrategy is not null)
            GlobalObjectManager.ObjectManager.Register<IRenderTargetStrategy>(_renderTargetStrategy);
        else 
            GlobalObjectManager.ObjectManager.Register<IRenderTargetStrategy>(new BasicScreenRenderTarget());
        
        var gameHost = new GameHost(_initialWidth, _initialHeight, _title, _fps, _camera);
        gameHost.SwitchScene(scene);
        
        return this;
    }
    
    public GameHost Run()
    {
        var host = GlobalObjectManager.ObjectManager.Get<GameHost>()!;
        host.Run();
        return host;
    }
}
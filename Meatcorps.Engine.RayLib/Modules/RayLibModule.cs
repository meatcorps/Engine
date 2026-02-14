using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.Interfaces.Resource;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Settings;
using Meatcorps.Engine.Core.Storage.Services;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Camera;
using Meatcorps.Engine.RayLib.Game;
using Meatcorps.Engine.RayLib.Game.GameTasks;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.PostProcessing.Abstractions;
using Meatcorps.Engine.RayLib.Renderer;
using Meatcorps.Engine.RayLib.Resources;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Modules;

public class RayLibModule
{
    private readonly IUniversalConfig _config;
    private ICamera? _camera;
    private KeyboardKey _exitKey = KeyboardKey.Escape;
    private int _fps = 60;
    private int _initialHeight = 720;
    private int _initialWidth = 1280;
    private string _title = "Meatcorps Engine";

    public RayLibModule()
    {
        _config = GlobalObjectManager.ObjectManager.Get<IUniversalConfig>() ?? new FallbackConfig();
    }

    public static RayLibModule Setup()
    {
        GlobalObjectManager.ObjectManager.RegisterList<IResourceLoadOnInit>();
        GlobalObjectManager.ObjectManager.RegisterList<IPostProcessor>();
        GlobalObjectManager.ObjectManager.RegisterList<IGameLoopTask>();
        GlobalObjectManager.ObjectManager.RegisterList<IRenderTargetStrategy>();
        var raylibResource = GlobalObjectManager.ObjectManager.Get<IRaylibResource>();

        if (raylibResource is null || MeatcorpsEngineLibSettings.IsDebug)
            raylibResource = new FileResourceLoader();

        GlobalObjectManager.ObjectManager.Register<IRaylibResource>(raylibResource);
        GlobalObjectManager.ObjectManager.Register<IResource>(raylibResource);
        GlobalObjectManager.ObjectManager.Register<ResourceManager>(new ResourceManager());
        return new RayLibModule();
    }

    public RayLibModule SetInitialSize(int width, int height)
    {
        _initialWidth = _config.GetOrDefault("Graphics", "WindowWidth", width, false);
        _initialHeight = _config.GetOrDefault("Graphics", "WindowHeight", height, false);
        return this;
    }

    public RayLibModule AutoSize()
    {
        
        _initialWidth = _config.GetOrDefault("Graphics", "WindowWidth", 0, false);
        _initialHeight = _config.GetOrDefault("Graphics", "WindowHeight", 0, false);
        if (_initialWidth == 0 || _initialHeight == 0)
        {
            _initialWidth = Raylib.GetScreenWidth();
            _initialHeight = Raylib.GetScreenHeight();
        }
        
        return this;
    }

    public RayLibModule StartInFullscreen()
    {
        _config.GetOrDefault("Graphics", "FullScreen", true, false);
        return this;
    }

    public RayLibModule SetTitle(string title)
    {
        _title = title;
        return this;
    }

    public RayLibModule SetFps(int fps)
    {
        _fps = fps;
        return this;
    }

    public RayLibModule SetCustomCamera(ICamera camera)
    {
        _camera = camera;
        return this;
    }

    public RayLibModule SetFixedSizeCamera(int targetWidth, int targetHeight, bool pixelPerfect = true)
    {
        if (MeatcorpsEngineLibSettings.IsDebug)
        {
            if (!_config.GetOrDefault("Debug", "SetFixedSizeCamera", true))
                return this;

            targetWidth = _config.GetOrDefault("Debug", "SetFixedSizeCamera_TargetWidth", targetWidth);
            targetHeight = _config.GetOrDefault("Debug", "SetFixedSizeCamera_TargetHeight", targetHeight);
        }

        _camera = new FixedSizeCamera(targetWidth, targetHeight);

        var renderTargetStrategy = new PixelPerfectRenderTarget(targetWidth, targetHeight).SetFullScreen();
        renderTargetStrategy.Bounds = new RectF(0, 0, 1, 1);
        renderTargetStrategy.UsePercentage = true;
        renderTargetStrategy.Camera = _camera;
        RegisterRenderTargetStrategy(renderTargetStrategy);
        RegisterRenderTargetStrategy(
            new PixelPerfectRenderTarget(renderTargetStrategy.RenderWidth, renderTargetStrategy.RenderHeight)
                .SetFullScreen(), "UI");

        return this;
    }

    public RayLibModule SetResource<T>(T instance, string tag = "default") where T : class, IResourceLoadOnInit
    {
        if (MeatcorpsEngineLibSettings.IsDebug)
        {
            if (!_config.GetOrDefault("Debug", "SetResource_" + instance.GetType().Name, true))
                return this;
        }

        GlobalObjectManager.ObjectManager.Add<IResourceLoadOnInit>(instance);
        GlobalObjectManager.ObjectManager.Register(instance, tag);
        return this;
    }

    public RayLibModule SetProcessing<T>(T postProcessor) where T : class, IPostProcessor
    {
        if (MeatcorpsEngineLibSettings.IsDebug)
        {
            if (!_config.GetOrDefault("Debug", "SetProcessing_" + postProcessor.GetType().Name, true))
                return this;
        }

        GlobalObjectManager.ObjectManager.Add<IPostProcessor>(postProcessor);
        GlobalObjectManager.ObjectManager.Add<IResourceLoadOnInit>(postProcessor);
        
        if (postProcessor is IConfigChangeTracker tracker)
            GlobalObjectManager.ObjectManager.Add<IConfigChangeTracker>(tracker);
        
        GlobalObjectManager.ObjectManager.Register<T>(postProcessor);
        return this;
    }

    public RayLibModule SetExitKey(KeyboardKey key = KeyboardKey.Null)
    {
        _exitKey = key;
        return this;
    }

    public RayLibModule Load<T>(T scene) where T : BaseScene
    {
        GlobalObjectManager.ObjectManager.Add<IGameLoopTask>(new AudioTask());
        GlobalObjectManager.ObjectManager.Add<IGameLoopTask>(new BackgroundServicesTask());
        GlobalObjectManager.ObjectManager.Add<IGameLoopTask>(new LoadAfterRayLibInitTask());
        GlobalObjectManager.ObjectManager.Add<IGameLoopTask>(new MouseTask());
        GlobalObjectManager.ObjectManager.Add<IGameLoopTask>(new SceneTask());

        if (_camera is null)
            _camera = new FallBackCamera();
        
        if (!GlobalObjectManager.ObjectManager.GetList<IRenderTargetStrategy>()!.Any())
        {
            RegisterRenderTargetStrategy(new BasicScreenRenderTarget().SetFullScreen());
            RegisterRenderTargetStrategy(new BasicScreenRenderTarget().SetFullScreen(), "UI");
        }

        FinalRenderer();

        var gameHost = new GameHost(_initialWidth, _initialHeight, _title, _fps, _camera);
        gameHost.SetExistKey(_exitKey);
        gameHost.SwitchScene(scene);

        return this;
    }

    private void FinalRenderer()
    {
        var finalRenderer = new BasicScreenRenderTarget().SetFullScreen();
        var mainRenderer = GlobalObjectManager.ObjectManager.Get<IRenderTargetStrategy>()!;
        foreach (var postProcessor in GlobalObjectManager.ObjectManager.GetList<IPostProcessor>()!)
            if (postProcessor is BaseFinalPostProcessor)
                finalRenderer.AddPostProcessor(postProcessor);
            else
                mainRenderer.PostProcessors.Add(postProcessor);

        RegisterRenderTargetStrategy(finalRenderer, "FINAL");
    }

    public RayLibModule RegisterRenderTargetStrategy(IRenderTargetStrategy renderTargetStrategy, string tag = "default")
    {
        GlobalObjectManager.ObjectManager.Register<IRenderTargetStrategy>(renderTargetStrategy, tag);
        GlobalObjectManager.ObjectManager.Add(renderTargetStrategy);
        return this;
    }

    public GameHost Run()
    {
        var host = GlobalObjectManager.ObjectManager.Get<GameHost>()!;
        host.Run();
        return host;
    }
}
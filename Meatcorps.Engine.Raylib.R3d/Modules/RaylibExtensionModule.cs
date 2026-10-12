using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Settings;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.Modules;
using Meatcorps.Engine.RayLib.R3d.Camera;
using Meatcorps.Engine.RayLib.R3d.Data;
using Meatcorps.Engine.RayLib.R3d.GameTasks;
using Meatcorps.Engine.RayLib.R3d.Interfaces;
using Meatcorps.Engine.RayLib.R3d.Renderer;
using Meatcorps.Engine.RayLib.Renderer;

namespace Meatcorps.Engine.RayLib.R3d.Modules;

public static class RaylibExtensionModule
{
    private static Camera3D? _camera;
    private static bool _initialized;
    private static bool _customSettings;

    public static RayLibModule Enable3D(this RayLibModule module, R3dSettings? settings = null)
    {
        GlobalObjectManager.ObjectManager.RegisterList<IGameLoopTask>();
        GlobalObjectManager.ObjectManager.Register(settings ?? new R3dSettings());
        GlobalObjectManager.ObjectManager.Add<IGameLoopTask>(new R3dGameLoop());
        GlobalObjectManager.ObjectManager.RegisterList<IR3dDispose>();
        _customSettings = settings != null;
        _initialized = true;
        return module;
    }

    public static RayLibModule Set3DCamera(this RayLibModule module, Camera3D camera)
    {
        if (!_initialized)
            throw new InvalidOperationException("3D module not initialized");
        _camera = camera;
        return module;
    }
    
    public static RayLibModule Set3DFixedCamera(this RayLibModule module, int targetWidth, int targetHeight)
    {
        if (!_initialized)
            throw new InvalidOperationException("3D module not initialized");
        
        var config = GlobalObjectManager.ObjectManager.Get<IUniversalConfig>()!;

        if (MeatcorpsEngineLibSettings.IsDebug)
        {
            if (!config.GetOrDefault("Debug", "SetFixedSizeCamera", true))
                return module;

            targetWidth = config.GetOrDefault("Debug", "SetFixedSizeCamera_TargetWidth", targetWidth);
            targetHeight = config.GetOrDefault("Debug", "SetFixedSizeCamera_TargetHeight", targetHeight);
        }

        var renderTargetStrategy = new R3dRenderTarget(targetWidth, targetHeight).SetFullScreen();
        renderTargetStrategy.Bounds = new RectF(0, 0, 1, 1);
        renderTargetStrategy.UsePercentage = true;
        renderTargetStrategy.Camera = _camera ?? new Camera3D(false);
        module.RegisterRenderTargetStrategy(renderTargetStrategy);
        module.RegisterRenderTargetStrategy(
            new PixelPerfectRenderTarget(renderTargetStrategy.RenderWidth, renderTargetStrategy.RenderHeight)
                .SetFullScreen(), "UI");

        if (!_customSettings)
        {
            GlobalObjectManager.ObjectManager.Get<R3dSettings>()!.RenderBufferSizeMode.Value =
                RenderBufferSizeModeEnum.RenderStrategy;
        }
        
        return module;
    }
}
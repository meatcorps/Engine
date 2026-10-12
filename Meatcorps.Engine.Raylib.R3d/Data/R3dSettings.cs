using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Utilities;
using R3D_cs;
using Raylib_cs;
using Environment = R3D_cs.Environment;

namespace Meatcorps.Engine.RayLib.R3d.Data;

public sealed class R3dSettings
{
    public DirtyValue<RenderBufferSizeModeEnum> RenderBufferSizeMode { get; } = new (RenderBufferSizeModeEnum.RenderStrategy, (_, __) => {});
    
    public DirtyValue<PointInt> RenderBufferSize { get; } = new (PointInt.Zero, (size, b) =>
    {
        if (b)
            R3D.Init(size.X, size.Y);
        else
            R3D.SetResolution(size.X, size.Y);
    });
    
    public DirtyValue<AntiAliasingMode> AntiAliasingMode { get; } = new (R3D_cs.AntiAliasingMode.Fxaa, (mode, _) => R3D.SetAntiAliasingMode(mode));
    
    public DirtyValue<AntiAliasingPreset> AntiAliasingPreset { get; } = new (R3D_cs.AntiAliasingPreset.Medium, (mode, _) => R3D.SetAntiAliasingPreset(mode));
    
    public DirtyValue<AspectMode> AspectMode { get; } = new (R3D_cs.AspectMode.Keep, (mode, _) => R3D.SetAspectMode(mode));
    
    public DirtyValue<DownscaleMode> DownscaleMode { get; } = new (R3D_cs.DownscaleMode.Linear, (mode, _) => R3D.SetDownscaleMode(mode));
    
    public DirtyValue<UpscaleMode> UpscaleMode { get; } = new (R3D_cs.UpscaleMode.Linear, (mode, _) => R3D.SetUpscaleMode(mode));
    
    public DirtyValue<TextureFilter> DefaultTextureFilter { get; } = new (TextureFilter.Trilinear, (mode, _) => R3D.SetTextureFilter(mode));
    
    public DirtyValue<TextureWrap> DefaultTextureWrap { get; } = new (TextureWrap.Repeat, (mode, _) => R3D.SetTextureWrap(mode));
    
    private bool first = true;
    
    public void EnvironmentChange(R3D.EnvironmentUpdater mode)
    {
        R3D.SetEnvironmentEx(mode);
    }

    public R3dSettings(PointInt? renderBufferSize = null)
    {
        if (renderBufferSize.HasValue)
        {
            RenderBufferSize.Value = renderBufferSize.Value;
            RenderBufferSizeMode.Value = RenderBufferSizeModeEnum.Custom;
        }
    }
    
    public void InitializeAll()
    {
        RenderBufferSize.ForceUpdate();
        AntiAliasingMode.ForceUpdate();
        AntiAliasingPreset.ForceUpdate();
        AspectMode.ForceUpdate();
        DownscaleMode.ForceUpdate();
        UpscaleMode.ForceUpdate();
        DefaultTextureFilter.ForceUpdate();
        DefaultTextureWrap.ForceUpdate();
        ApplyAll();
    }
    
    public void ApplyAll()
    {
        RenderBufferSize.Apply();
        AntiAliasingMode.Apply();
        AntiAliasingPreset.Apply();
        AspectMode.Apply();
        DownscaleMode.Apply();
        UpscaleMode.Apply();
        DefaultTextureFilter.Apply();
        DefaultTextureWrap.Apply();

        if (first)
        {
            EnvironmentChange((ref Environment env) =>
            {
                env.Background.Color = Color.Black;
                env.Ambient.Color = new Color(0.1f, 0.1f, 0.1f);
                env.Ambient.Energy = 1.0f;
            });
        }
    }
}

public enum RenderBufferSizeModeEnum
{
    WindowSize,
    RenderStrategy,
    Custom
}
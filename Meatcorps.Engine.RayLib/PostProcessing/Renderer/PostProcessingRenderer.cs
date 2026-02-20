using Meatcorps.Engine.RayLib.Interfaces;
using Raylib_cs;
// ReSharper disable PossibleMultipleEnumeration

namespace Meatcorps.Engine.RayLib.PostProcessing.Renderer;

public sealed class PostProcessingRenderer : IDisposable
{
    private RenderTexture2D? _renderTarget1;
    private RenderTexture2D? _renderTarget2;
    private bool _swapped;

    private RenderTexture2D FromTexture => _swapped ? _renderTarget2!.Value : _renderTarget1!.Value;

    private RenderTexture2D ToTexture => _swapped ? _renderTarget1!.Value : _renderTarget2!.Value;

    public void Dispose()
    {
        if (_renderTarget1 is not null)
            Raylib.UnloadRenderTexture(_renderTarget1.Value);
        if (_renderTarget2 is not null)
            Raylib.UnloadRenderTexture(_renderTarget2.Value);
    }


    public void Start(IEnumerable<IPostProcessor> postProcessors, float deltaTime)
    {
        foreach (var postProcessor in postProcessors)
        {
            if (!postProcessor.Enabled)
                continue;
            postProcessor.BeginFrame(deltaTime);
        }
    }

    public RenderTexture2D Render(IEnumerable<IPostProcessor> postProcessors, RenderTexture2D sourceTexture)
    {
        var totalEnabled = 0;
        
        foreach (var postProcessor in postProcessors)
            if (postProcessor.Enabled) 
                totalEnabled++;

        if (totalEnabled == 0)
            return sourceTexture;

        _swapped = false;

        _renderTarget1 = CreateRenderTexture(_renderTarget1, sourceTexture.Texture);
        _renderTarget2 = CreateRenderTexture(_renderTarget2, sourceTexture.Texture);

        var first = true;

        foreach (var postProcessor in postProcessors)
        {
            if (!postProcessor.Enabled)
                continue;

            if (postProcessor is INeedsSceneTexture needsSceneTexture)
                needsSceneTexture.SetSceneTexture(sourceTexture.Texture);

            postProcessor.Apply(first ? sourceTexture.Texture : FromTexture.Texture, ToTexture);

            _swapped = !_swapped;
            first = false;
        }

        return FromTexture;
    }

    public void End(IEnumerable<IPostProcessor> postProcessors)
    {
        foreach (var postProcessor in postProcessors)
        {
            if (!postProcessor.Enabled)
                continue;
            postProcessor.EndFrame();
        }
    }

    private RenderTexture2D CreateRenderTexture(RenderTexture2D? target, Texture2D sourceTexture)
    {
        var targetChanged = false;

        if (target is null)
        {
            targetChanged = true;
            target = Raylib.LoadRenderTexture(sourceTexture.Width, sourceTexture.Height);
        }

        if (target.Value.Texture.Width != sourceTexture.Width || target.Value.Texture.Height != sourceTexture.Height)
        {
            targetChanged = true;
            Raylib.UnloadRenderTexture(target.Value);
            target = Raylib.LoadRenderTexture(sourceTexture.Width, sourceTexture.Height);
        }

        if (targetChanged) Raylib.SetTextureFilter(target.Value.Texture, TextureFilter.Point);

        return target.Value;
    }
}
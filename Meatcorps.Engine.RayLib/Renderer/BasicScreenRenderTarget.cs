using System.Numerics;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Engine.RayLib.PostProcessing.Renderer;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Renderer;

public class BasicScreenRenderTarget : BaseRenderTarget, IDisposable
{
    private readonly PostProcessingRenderer _postProcessingRenderer = new();
    private readonly bool _useRenderTexture = true;
    private RenderTexture2D? _renderTexture;

    public BasicScreenRenderTarget(bool useRenderTexture = true)
    {
        _useRenderTexture = useRenderTexture;
    }
    
    public override int RenderWidth
    {
        get
        {
            var screenSize = GetScreenRect();
            return (int)screenSize.Width;
        }
    }

    public override int RenderHeight
    {
        get
        {
            var screenSize = GetScreenRect();
            return (int)screenSize.Height;
        }
    }

    public void Dispose()
    {
        if (_renderTexture is not null)
            Raylib.UnloadRenderTexture(_renderTexture.Value);

        _postProcessingRenderer.Dispose();
    }

    public override void BeginRender(Color clearColor)
    {
        if (_useRenderTexture)
        {
            var screenSize = GetScreenSize();

            if (_renderTexture is null || _renderTexture.Value.Texture.Width != screenSize.X ||
                _renderTexture.Value.Texture.Height != screenSize.Y)
            {
                if (_renderTexture is not null)
                    Raylib.UnloadRenderTexture(_renderTexture.Value);
                _renderTexture = Raylib.LoadRenderTexture(screenSize.X, screenSize.Y);
            }
        }

        Camera?.StartCamera();

        if (_useRenderTexture)
            Raylib.BeginTextureMode(_renderTexture!.Value);

        Raylib.ClearBackground(new Color(0, 0, 0, 0));
    }

    public override void EndRender(RenderTexture2D? targetTexture = null)
    {
        if (_useRenderTexture)
            Raylib.EndTextureMode();

        Camera?.EndCamera();

        if (_useRenderTexture)
        {
            var deltaTime = Raylib.GetFrameTime();
            _postProcessingRenderer.Start(PostProcessors, deltaTime);
            var currentRenderer = _postProcessingRenderer.Render(PostProcessors, _renderTexture!.Value);

            var destinationRect = GetScreenRect();

            if (targetTexture is not null)
            {
                Raylib.BeginTextureMode(targetTexture.Value);
                Raylib.BeginBlendMode(BlendMode.AlphaPremultiply);
            }

            Raylib.DrawTexturePro(
                currentRenderer.Texture,
                new Rectangle(0, 0, destinationRect.Width, -destinationRect.Height),
                destinationRect.ToRectangle(),
                Vector2.Zero, 0f, Color.White
            );

            if (targetTexture is not null)
            {
                Raylib.EndBlendMode();
                Raylib.EndTextureMode();
            }
        }

        if (_useRenderTexture)
            _postProcessingRenderer.End(PostProcessors);
    }
}
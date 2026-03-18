using System.Numerics;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Engine.RayLib.PostProcessing.Renderer;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Renderer;

public sealed class PixelPerfectRenderTarget : BaseRenderTarget, IDisposable
{
    public bool UsePixelOffset { get; set; } = true;
    private readonly PostProcessingRenderer _postProcessingRenderer = new();
    private Color _clearColor;
    private RenderTexture2D? _currentRenderer;
    private bool _isDisposed;
    private Vector2 _offset;
    private RenderTexture2D? _renderTexture1;
    private RenderTexture2D? _renderTextureFinal;
    private float _screenScale;

    public PixelPerfectRenderTarget(int targetWidth, int targetHeight)
    {
        RenderWidth = targetWidth;
        RenderHeight = targetHeight;
    }

    public override int RenderWidth { get; }

    public override int RenderHeight { get; }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        if (_renderTexture1 is not null)
            Raylib.UnloadRenderTexture(_renderTexture1!.Value);
        if (_renderTextureFinal is not null)
            Raylib.UnloadRenderTexture(_renderTextureFinal!.Value);

        _renderTexture1 = null;
        _renderTextureFinal = null;

        _postProcessingRenderer.Dispose();
    }

    public override void BeginRender(Color clearColor)
    {
        _clearColor = clearColor;
        if (_renderTexture1 is null)
        {
            _renderTexture1 = Raylib.LoadRenderTexture(RenderWidth + 1, RenderHeight + 1); // +1 for overlap
            Raylib.SetTextureFilter(_renderTexture1.Value.Texture, TextureFilter.Point);
        }

        var destinationRect = GetScreenRect();
        if (_renderTextureFinal is null || _renderTextureFinal.Value.Texture.Width != (int)destinationRect.Width ||
            _renderTextureFinal.Value.Texture.Height != (int)destinationRect.Height)
        {
            if (_renderTextureFinal is not null)
                Raylib.UnloadRenderTexture(_renderTextureFinal.Value);

            _renderTextureFinal = Raylib.LoadRenderTexture((int)destinationRect.Width, (int)destinationRect.Height);
            Raylib.SetTextureFilter(_renderTextureFinal.Value.Texture, TextureFilter.Point);
        }

        Raylib.BeginTextureMode(_renderTexture1.Value);
        Raylib.ClearBackground(clearColor);

        Camera?.StartCamera();
    }

    public override void EndRender(RenderTexture2D? targetTexture = null)
    {
        Camera?.EndCamera();
        Raylib.EndTextureMode();

        var deltaTime = Raylib.GetFrameTime();
        _postProcessingRenderer.Start(PostProcessors, deltaTime);
        _currentRenderer = _postProcessingRenderer.Render(PostProcessors, _renderTexture1!.Value);

        var screenWidth = _renderTextureFinal!.Value.Texture.Width;
        var screenHeight = _renderTextureFinal!.Value.Texture.Height;

        _screenScale = MathF.Min(
            screenWidth / (float)RenderWidth,
            screenHeight / (float)RenderHeight
        );

        var renderWidth = RenderWidth * _screenScale;
        var renderHeight = RenderHeight * _screenScale;

        var cameraPosition = Camera?.Position ?? Vector2.Zero;

        if (UsePixelOffset)
        {
            var cameraSubpixelX = cameraPosition.X - MathF.Floor(cameraPosition.X);
            var cameraSubpixelY = cameraPosition.Y - MathF.Floor(cameraPosition.Y);

            var subpixelOffsetX = cameraSubpixelX * _screenScale;
            var subpixelOffsetY = cameraSubpixelY * _screenScale;

            _offset = new Vector2(
                (screenWidth - renderWidth) / 2f - subpixelOffsetX,
                (screenHeight - renderHeight) / 2f - subpixelOffsetY
            );
        }
        else
        {
            _offset = new Vector2(
                (screenWidth - renderWidth) / 2f,
                (screenHeight - renderHeight) / 2f
            );
        }

        Raylib.BeginTextureMode(_renderTextureFinal.Value);
        Raylib.BeginBlendMode(BlendMode.AlphaPremultiply);
        Raylib.ClearBackground(_clearColor);
        Raylib.DrawTexturePro(
            _currentRenderer!.Value.Texture,
            new Rectangle(0, 0, RenderWidth, -RenderHeight),
            new Rectangle(_offset.X, _offset.Y, renderWidth, renderHeight),
            Vector2.Zero, 0f, Color.White
        );
        Raylib.EndBlendMode();
        Raylib.EndTextureMode();
        
        var destinationRect = GetScreenRect();

        if (targetTexture is not null)
            Raylib.BeginTextureMode(targetTexture.Value);


        Raylib.BeginBlendMode(BlendMode.AlphaPremultiply);
        Raylib.DrawTexturePro(
            _renderTextureFinal.Value.Texture,
            new Rectangle(0, 0, screenWidth, -screenHeight),
            destinationRect.ToRectangle(),
            Vector2.Zero, 0f, Color.White
        );
        Raylib.EndBlendMode();

        if (targetTexture is not null)
            Raylib.EndTextureMode();

        _postProcessingRenderer.End(PostProcessors);
    }
}
using System.Numerics;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Engine.RayLib.PostProcessing.Renderer;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Renderer;

public sealed class PixelPerfectRenderTarget : BaseRenderTarget, IDisposable
{
    private RenderTexture2D? _renderTexture1;
    private RenderTexture2D? _currentRenderer;
    private RenderTexture2D? _renderTextureFinal;
    
    private int _targetWidth, _targetHeight;
    private float _screenScale;
    private Vector2 _offset;
    private bool _isDisposed;

    public override int RenderWidth => _targetWidth;
    public override int RenderHeight => _targetHeight;

    private readonly PostProcessingRenderer _postProcessingRenderer = new();

    public PixelPerfectRenderTarget(int targetWidth, int targetHeight)
    {
        _targetWidth = targetWidth;
        _targetHeight = targetHeight;
    }

    public override void BeginRender(Color clearColor)
    {
        if (_renderTexture1 is null)
        {
            _renderTexture1 = Raylib.LoadRenderTexture(_targetWidth + 1, _targetHeight + 1); // +1 for overlap
            Raylib.SetTextureFilter(_renderTexture1.Value.Texture, TextureFilter.Point);
        }

        var destinationRect = GetScreenRect();
        if (_renderTextureFinal is null || _renderTextureFinal.Value.Texture.Width != destinationRect.Width ||
            _renderTextureFinal.Value.Texture.Height != destinationRect.Height)
        {
            if (_renderTextureFinal is not null)
                Raylib.UnloadRenderTexture(_renderTextureFinal.Value);

            _renderTextureFinal = Raylib.LoadRenderTexture((int) destinationRect.Width, (int) destinationRect.Height);
            Raylib.SetTextureFilter(_renderTextureFinal.Value.Texture, TextureFilter.Point);
        }

        Raylib.BeginTextureMode(_renderTexture1.Value);
        Raylib.ClearBackground(new Color(0,0,0,0));
        
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
            screenWidth / (float)_targetWidth,
            screenHeight / (float)_targetHeight
        );

        var renderWidth = _targetWidth * _screenScale;
        var renderHeight = _targetHeight * _screenScale;

        var cameraPosition = Camera?.Position ?? Vector2.Zero;
        
        var cameraSubpixelX = cameraPosition.X - MathF.Floor(cameraPosition.X);
        var cameraSubpixelY = cameraPosition.Y - MathF.Floor(cameraPosition.Y);
        
        var subpixelOffsetX = cameraSubpixelX * _screenScale;
        var subpixelOffsetY = cameraSubpixelY * _screenScale;

        _offset = new Vector2(
            (screenWidth - renderWidth) / 2f - subpixelOffsetX,
            (screenHeight - renderHeight) / 2f - subpixelOffsetY
        );

        Raylib.BeginTextureMode(_renderTextureFinal.Value);
        Raylib.ClearBackground(new Color(0,0,0,0));
        Raylib.DrawTexturePro(
            _currentRenderer!.Value.Texture,
            new Rectangle(0, 0, _targetWidth, -_targetHeight),
            new Rectangle(_offset.X, _offset.Y, renderWidth, renderHeight),
            Vector2.Zero, 0f, Color.White
        );
        Raylib.EndTextureMode();
        
        var destinationRect = GetScreenRect();

        if (targetTexture is not null)
            Raylib.BeginTextureMode(targetTexture.Value);

        Raylib.DrawTexturePro(
            _renderTextureFinal.Value.Texture,
            new Rectangle(0, 0, screenWidth, -screenHeight),
            destinationRect.ToRectangle(),
            Vector2.Zero, 0f, Color.White
        );
        
        if (targetTexture is not null)
            Raylib.EndTextureMode();
        
        _postProcessingRenderer.End(PostProcessors!);
    }

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
}
using System.Numerics;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.PostProcessing.Abstractions;
using Meatcorps.Engine.RayLib.PostProcessing.Renderer;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Renderer;

public sealed class PixelPerfectRenderTarget : IRenderTargetStrategy, IDisposable
{
    private RenderTexture2D? _renderTexture1;
    private RenderTexture2D? _currentRenderer;

    private RenderTexture2D? _renderTextureFinal;
    
    private int _targetWidth, _targetHeight;
    private float _screenScale;
    private Vector2 _offset;
    private bool _isDisposed;

    public int RenderWidth => _targetWidth;
    public int RenderHeight => _targetHeight;

    private List<IPostProcessor>? _postProcessors;
    private List<IPostProcessor>? _finalPostProcessors;

    private readonly PostProcessingRenderer _postProcessingRenderer = new();
    private readonly PostProcessingRenderer _finalPostProcessingRenderer = new();

    public PixelPerfectRenderTarget(int targetWidth, int targetHeight)
    {
        _targetWidth = targetWidth;
        _targetHeight = targetHeight;
    }

    public void BeginRender(Color clearColor, ICamera camera)
    {
        if (_postProcessors is null)
        {
            var postProcessors = GlobalObjectManager.ObjectManager.GetList<IPostProcessor>()!;

            _postProcessors = new List<IPostProcessor>(postProcessors.Where(x => x is not BaseFinalPostProcessor));
            _finalPostProcessors = new List<IPostProcessor>(postProcessors.Where(x => x is BaseFinalPostProcessor));
        }

        if (_renderTexture1 is null)
        {
            _renderTexture1 = Raylib.LoadRenderTexture(_targetWidth + 1, _targetHeight + 1); // +1 for overlap
            Raylib.SetTextureFilter(_renderTexture1.Value.Texture, TextureFilter.Point);
        }

        var screenWidth = Raylib.GetScreenWidth();
        var screenHeight = Raylib.GetScreenHeight();
        if (_renderTextureFinal is null || _renderTextureFinal.Value.Texture.Width != screenWidth ||
            _renderTextureFinal.Value.Texture.Height != screenHeight)
        {
            if (_renderTextureFinal is not null)
                Raylib.UnloadRenderTexture(_renderTextureFinal.Value);
        
            _renderTextureFinal = Raylib.LoadRenderTexture(screenWidth, screenHeight);
            Raylib.SetTextureFilter(_renderTextureFinal.Value.Texture, TextureFilter.Point);
        }

        Raylib.BeginTextureMode(_renderTexture1.Value);
        Raylib.ClearBackground(clearColor);
    }

    public void PostProcess(CameraLayer layer)
    {
        Raylib.EndTextureMode();

        if (layer == CameraLayer.World)
        {
            var deltaTime = Raylib.GetFrameTime();
            _postProcessingRenderer.Start(_postProcessors!, deltaTime);
        }
        
        _currentRenderer = _postProcessingRenderer.Render(_postProcessors!, _renderTexture1!.Value);

        if (layer == CameraLayer.World)
            Raylib.BeginTextureMode(_currentRenderer.Value);

        _currentRenderer = _renderTexture1;
    }

    public void EndRender()
    {
        var screenWidth = _renderTextureFinal!.Value.Texture.Width;
        var screenHeight = _renderTextureFinal!.Value.Texture.Height;
        
        var _preScreenScale = MathF.Min(
            screenWidth / (float)_targetWidth,
            screenHeight / (float)_targetHeight
        );
        _screenScale = MathF.Floor(_preScreenScale);

        var renderWidth = _targetWidth * _screenScale;
        var renderHeight = _targetHeight * _screenScale;

        var camera = GlobalObjectManager.ObjectManager.Get<ICamera>()!;
        var cameraSubpixelX = camera.Position.X - MathF.Floor(camera.Position.X);
        var cameraSubpixelY = camera.Position.Y - MathF.Floor(camera.Position.Y);

        var subpixelOffsetX = cameraSubpixelX * _screenScale;
        var subpixelOffsetY = cameraSubpixelY * _screenScale;

        _offset = new Vector2(
            (screenWidth - renderWidth) / 2f - subpixelOffsetX,
            (screenHeight - renderHeight) / 2f - subpixelOffsetY
        );

        Raylib.BeginTextureMode(_renderTextureFinal.Value);
        Raylib.ClearBackground(Color.Black);
        Raylib.DrawTexturePro(
            _currentRenderer!.Value.Texture,
            new Rectangle(0, 0, _targetWidth, -_targetHeight),
            new Rectangle(_offset.X, _offset.Y, renderWidth, renderHeight),
            Vector2.Zero, 0f, Color.White
        );
        Raylib.EndTextureMode();

        _finalPostProcessingRenderer.Start(_finalPostProcessors!, Raylib.GetFrameTime());
        _currentRenderer = _finalPostProcessingRenderer.Render(_finalPostProcessors!, _renderTextureFinal.Value);
        
        Raylib.BeginDrawing();
        Raylib.DrawTexturePro(
            _currentRenderer.Value.Texture,
            new Rectangle(0, 0, screenWidth, -screenHeight),
            new Rectangle(0, 0, screenWidth, screenHeight),
            Vector2.Zero, 0f, Color.White
        );
    }
    
    public void EndDrawing()
    {
        Raylib.EndDrawing();
        
        _postProcessingRenderer.End(_postProcessors!);
        _finalPostProcessingRenderer.End(_finalPostProcessors!);
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
        _finalPostProcessingRenderer.Dispose();
    }
}
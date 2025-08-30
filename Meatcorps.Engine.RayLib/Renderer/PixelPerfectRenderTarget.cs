using System.Numerics;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.PostProcessing.Abstractions;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Renderer;

public sealed class PixelPerfectRenderTarget : IRenderTargetStrategy, IDisposable
{
    private RenderTexture2D? _renderTexture1;
    private RenderTexture2D? _renderTexture2;
    private RenderTexture2D? _renderTextureFinal;
    private RenderTexture2D? _renderTextureTemp;
    private RenderTexture2D? _renderTextureSceneCopy;
    private int _targetWidth, _targetHeight;
    private float _screenScale;
    private Vector2 _offset;
    private bool _isDisposed;

    public int RenderWidth => _targetWidth;
    public int RenderHeight => _targetHeight;

    private List<IPostProcessor>? _postProcessors;
    private List<IPostProcessor>? _finalPostProcessors;

    private bool _swapTarget;
    
    private bool _postProcessingWithSceneTexture;

    public PixelPerfectRenderTarget(int targetWidth, int targetHeight)
    {
        _targetWidth = targetWidth;
        _targetHeight = targetHeight;
    }

    public void BeginRender(Color clearColor, ICamera camera)
    {
        if (_postProcessors is null)
        {
            var postProcessors = GlobalObjectManager.ObjectManager.GetList<IPostProcessor>();

            foreach (var postProcessor in postProcessors!)
            {
                postProcessor.Load();
                if (postProcessor is INeedsSceneTexture)
                    _postProcessingWithSceneTexture = true;
            }

            _postProcessors = new List<IPostProcessor>(postProcessors.Where(x => x is not BaseFinalPostProcessor));
            _finalPostProcessors = new List<IPostProcessor>(postProcessors.Where(x => x is BaseFinalPostProcessor));
            
            
        }

        if (_renderTexture1 is null)
        {
            _renderTexture1 = Raylib.LoadRenderTexture(_targetWidth + 1, _targetHeight + 1); // +1 for overlap
            Raylib.SetTextureFilter(_renderTexture1.Value.Texture, TextureFilter.Point);
            _renderTexture2 = Raylib.LoadRenderTexture(_targetWidth + 1, _targetHeight + 1); // +1 for overlap
            Raylib.SetTextureFilter(_renderTexture2.Value.Texture, TextureFilter.Point);
        }

        var screenWidth = Raylib.GetScreenWidth();
        var screenHeight = Raylib.GetScreenHeight();
        if (_renderTextureFinal is null || _renderTextureFinal.Value.Texture.Width != screenWidth ||
            _renderTextureFinal.Value.Texture.Height != screenHeight)
        {
            if (_renderTextureFinal is not null)
                Raylib.UnloadRenderTexture(_renderTextureFinal.Value);

            if (_renderTextureTemp is not null)
                Raylib.UnloadRenderTexture(_renderTextureTemp.Value);
            
            if (_postProcessingWithSceneTexture)
                if (_renderTextureSceneCopy is not null)
                    Raylib.UnloadRenderTexture(_renderTextureSceneCopy.Value);

            _renderTextureFinal = Raylib.LoadRenderTexture(screenWidth, screenHeight);
            Raylib.SetTextureFilter(_renderTextureFinal.Value.Texture, TextureFilter.Point);
            _renderTextureTemp = Raylib.LoadRenderTexture(screenWidth, screenHeight);
            Raylib.SetTextureFilter(_renderTextureTemp.Value.Texture, TextureFilter.Point);

            if (_postProcessingWithSceneTexture)
            {
                _renderTextureSceneCopy = Raylib.LoadRenderTexture(screenWidth, screenHeight);
                Raylib.SetTextureFilter(_renderTextureSceneCopy.Value.Texture, TextureFilter.Point);
            }
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

            foreach (var postProcessor in _postProcessors!)
            {
                if (!postProcessor.Enabled)
                    continue;
                postProcessor.BeginFrame(deltaTime);
            }
        }

        var currentTexture = _renderTexture2!.Value;
        var nextTexture = _renderTexture1!.Value;
        var swap = true;

        foreach (var postProcessor in _postProcessors!)
        {
            if (ShouldNotApplyPostProcessor(postProcessor, layer))
                continue;

            if (swap)
            {
                currentTexture = _renderTexture1.Value;
                nextTexture = _renderTexture2.Value;
            }
            else
            {
                currentTexture = _renderTexture2.Value;
                nextTexture = _renderTexture1.Value;
            }

            postProcessor.Apply(currentTexture!.Texture, nextTexture);

            swap = !swap;
        }

        _renderTexture1 = nextTexture;
        _renderTexture2 = currentTexture;

        if (layer == CameraLayer.World)
            Raylib.BeginTextureMode(_renderTexture1.Value);
    }

    public void EndRender()
    {
        var screenWidth = _renderTextureFinal!.Value.Texture.Width;
        var screenHeight = _renderTextureFinal!.Value.Texture.Height;

        _screenScale = MathF.Floor(MathF.Min(
            screenWidth / (float)_targetWidth,
            screenHeight / (float)_targetHeight
        ));

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
            _renderTexture1!.Value.Texture,
            new Rectangle(0, 0, _targetWidth, -_targetHeight),
            new Rectangle(_offset.X, _offset.Y, renderWidth, renderHeight),
            Vector2.Zero, 0f, Color.White
        );
        Raylib.EndTextureMode();

        if (_postProcessingWithSceneTexture)
        {
            Raylib.BeginTextureMode(_renderTextureSceneCopy!.Value);
            Raylib.DrawTexturePro(
                _renderTextureFinal!.Value.Texture,
                new Rectangle(0, 0, screenWidth, -screenHeight),
                new Rectangle(0, 0, screenWidth, screenHeight),
                Vector2.Zero, 0f, Color.White
            );
            Raylib.EndTextureMode();
        }

// Post-process full-screen effects
        var finalCurrent = _renderTextureTemp!.Value;
        var finalNext = _renderTextureFinal!.Value;
        bool finalSwap = true;

        foreach (var finalProcessor in _finalPostProcessors!)
        {
            if (!finalProcessor.Enabled) continue;
            
            if (finalSwap)
            {
                finalCurrent = _renderTextureFinal.Value;
                finalNext = _renderTextureTemp.Value;
            }
            else
            {
                finalCurrent = _renderTextureTemp.Value;
                finalNext = _renderTextureFinal.Value;
            }

            if (_postProcessingWithSceneTexture)
            {
                if (finalProcessor is INeedsSceneTexture needsScene)
                    needsScene.SetSceneTexture(_renderTextureSceneCopy!.Value.Texture);
            }

            finalProcessor.Apply(finalCurrent.Texture, finalNext);
            finalSwap = !finalSwap;
        }
        
        _renderTextureTemp = finalCurrent; 
        _renderTextureFinal = finalNext; 

// Final push to screen
        Raylib.BeginDrawing();
        Raylib.DrawTexturePro(
            _renderTextureFinal.Value.Texture,
            new Rectangle(0, 0, screenWidth, -screenHeight),
            new Rectangle(0, 0, screenWidth, screenHeight),
            Vector2.Zero, 0f, Color.White
        );
    }
    
    
    public void EndDrawing()
    {
        Raylib.EndDrawing();

        foreach (var postProcessor in _postProcessors!)
        {
            if (!postProcessor.Enabled)
                continue;

            postProcessor.EndFrame();
        }
    }

    private bool ShouldNotApplyPostProcessor(IPostProcessor pp, CameraLayer layer)
    {
        if (!pp.Enabled)
            return true;
        if (layer == CameraLayer.World && !pp.IncludeUI)
            return false;
        if (layer == CameraLayer.UI && pp.IncludeUI)
            return false;
        return true;
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;
        _isDisposed = true;
        
        if (_renderTexture1 is not null)
            Raylib.UnloadRenderTexture(_renderTexture1!.Value);
        if (_renderTexture2 is not null)
            Raylib.UnloadRenderTexture(_renderTexture2!.Value);
        if (_renderTextureFinal is not null)
            Raylib.UnloadRenderTexture(_renderTextureFinal!.Value);
        if (_renderTextureTemp is not null)
            Raylib.UnloadRenderTexture(_renderTextureTemp!.Value);
        if (_renderTextureSceneCopy is not null)
            Raylib.UnloadRenderTexture(_renderTextureSceneCopy!.Value);
        
        _renderTexture1 = null;
        _renderTexture2 = null;
        _renderTextureFinal = null;
        _renderTextureTemp = null;
        _renderTextureSceneCopy = null;
    }
}
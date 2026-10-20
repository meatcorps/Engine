using System.Numerics;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Engine.RayLib.PostProcessing.Renderer;
using Meatcorps.Engine.RayLib.Renderer;
using Meatcorps.Engine.RayLib.R3d.Interfaces;
using R3D_cs;
using Raylib_cs;
using BlendMode = Raylib_cs.BlendMode;
using R3dCamera3D = Meatcorps.Engine.RayLib.R3d.Camera.Camera3D;

namespace Meatcorps.Engine.RayLib.R3d.Renderer;

public sealed class R3dRenderTarget : BaseRenderTarget, IDisposable
{
    private readonly PostProcessingRenderer _postProcessingRenderer = new();
    private readonly List<IRenderer> _renderers = new();

    private View _view;

    private Color _clearColor;

    private RenderTexture2D? _renderTexture;
    private RenderTexture2D? _renderTextureFinal;

    private bool _isDisposed;

    public ICamera3D Camera3D
    {
        get => Camera as ICamera3D
               ?? throw new InvalidOperationException(
                   "R3dRenderTarget requires a camera implementing ICamera3D.");

        set => Camera = value
            ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// R3D viewport inside the render target.
    ///
    /// Width/height <= 0 means the complete render target is used.
    /// </summary>
    public Rectangle Viewport
    {
        get => _view.Viewport;
        set => _view.Viewport = value;
    }

    public R3dRenderTarget(
        int targetWidth,
        int targetHeight,
        ICamera3D? camera = null)
    {
        RenderWidth = targetWidth;
        RenderHeight = targetHeight;

        Camera3D = camera ?? R3dCamera3D.DefaultPerspective();

        _view = new View
        {
            Viewport = new Rectangle(0, 0, 0, 0)
        };
    }

    public override int RenderWidth { get; }

    public override int RenderHeight { get; }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        if (_renderTexture is not null)
            Raylib.UnloadRenderTexture(_renderTexture.Value);

        if (_renderTextureFinal is not null)
            Raylib.UnloadRenderTexture(_renderTextureFinal.Value);

        _renderTexture = null;
        _renderTextureFinal = null;

        // Do not leave the view referring to a released render target.
        _view.Target = default;

        _postProcessingRenderer.Dispose();
    }

    public override void BeginRender(Color clearColor)
    {
        _clearColor = clearColor;

        EnsureRenderTextures();

        /*
         * R3D owns the actual render pass.
         *
         * Do NOT leave BeginTextureMode active around BeginPro/End.
         * The RenderTexture is supplied through View.Target instead.
         *
         * We only clear it beforehand. This is also useful when the
         * R3D viewport/aspect mode does not cover the complete target.
         */
        Raylib.BeginTextureMode(_renderTexture!.Value);
        Raylib.ClearBackground(clearColor);
        Raylib.EndTextureMode();

        /*
         * Refresh these every frame.
         *
         * The camera may have been updated or replaced since the
         * previous frame, and the RenderTexture may have been recreated.
         */
        _view.Camera = Camera3D.Native;
        _view.Target = _renderTexture.Value;

        R3D.BeginPro(_view);
    }

    public override void EndRender(RenderTexture2D? targetTexture = null)
    {
        /*
         * This is where R3D actually performs the rendering of all
         * queued meshes/lights/probes/etc. and writes into View.Target.
         */
        R3D.End();

        /*
         * From this point onward we're back in the normal Meatcorps
         * 2D/composition pipeline.
         */
        var deltaTime = Raylib.GetFrameTime();

        _postProcessingRenderer.Start(PostProcessors, deltaTime);

        var currentRenderer = _postProcessingRenderer.Render(
            PostProcessors,
            _renderTexture!.Value);

        var screenWidth = _renderTextureFinal!.Value.Texture.Width;
        var screenHeight = _renderTextureFinal.Value.Texture.Height;

        var screenScale = MathF.Min(
            screenWidth / (float)RenderWidth,
            screenHeight / (float)RenderHeight);

        var renderWidth = RenderWidth * screenScale;
        var renderHeight = RenderHeight * screenScale;

        /*
         * No 2D camera sub-pixel offset here.
         *
         * Camera3D.Position represents actual world-space camera
         * movement and must never shift the completed framebuffer.
         */
        var offset = new Vector2(
            (screenWidth - renderWidth) / 2f,
            (screenHeight - renderHeight) / 2f);

        /*
         * Scale the fixed render target into the target's presentation
         * area while preserving the render-target aspect ratio.
         */
        Raylib.BeginTextureMode(_renderTextureFinal.Value);

        Raylib.BeginBlendMode(BlendMode.AlphaPremultiply);

        Raylib.ClearBackground(_clearColor);

        Raylib.DrawTexturePro(
            currentRenderer.Texture,
            new Rectangle(
                0,
                0,
                currentRenderer.Texture.Width,
                -currentRenderer.Texture.Height),
            new Rectangle(
                offset.X,
                offset.Y,
                renderWidth,
                renderHeight),
            Vector2.Zero,
            0f,
            Color.White);

        Raylib.EndBlendMode();

        Raylib.EndTextureMode();

        /*
         * Normal RenderService composition.
         *
         * targetTexture != null:
         *     compose into the shared Meatcorps render texture.
         *
         * targetTexture == null:
         *     compose into the currently active framebuffer.
         */
        var destinationRect = GetScreenRect();

        if (targetTexture is not null)
            Raylib.BeginTextureMode(targetTexture.Value);

        Raylib.BeginBlendMode(BlendMode.AlphaPremultiply);

        Raylib.DrawTexturePro(
            _renderTextureFinal.Value.Texture,
            new Rectangle(
                0,
                0,
                screenWidth,
                -screenHeight),
            destinationRect.ToRectangle(),
            Vector2.Zero,
            0f,
            Color.White);

        Raylib.EndBlendMode();

        if (targetTexture is not null)
        {
            Raylib.EndTextureMode();
            Result = targetTexture.Value.Texture;
        }
        else
            Result = _renderTextureFinal.Value.Texture;

        _postProcessingRenderer.End(PostProcessors);
    }

    public override void RegisterRenderer(IRenderer renderer)
    {
        /*
         * SceneLayer ordering is a 2D concern.
         *
         * Layer also must not be interpreted as one of the normal
         * Meatcorps ordered 2D buckets here. R3D renderers can use it
         * for their native R3D layer-mask semantics instead.
         */
        _renderers.Add(renderer);
    }

    public override void Render()
    {
        /*
         * Draw() queues R3D draw calls.
         *
         * The actual GPU rendering happens later in R3D.End().
         */
        foreach (var renderer in _renderers)
            renderer.Draw();

        _renderers.Clear();
    }

    public override void Setup(int sceneLayers, int gameObjectLayers)
    {
        /*
         * Intentionally do not call base.Setup().
         *
         * BaseRenderTarget's buckets implement ordered 2D
         * SceneLayer -> Layer rendering, which R3D does not use.
         */
        _renderers.Clear();
    }

    private void EnsureRenderTextures()
    {
        /*
         * Unlike PixelPerfectRenderTarget there is deliberately no +1.
         *
         * R3D derives the camera aspect ratio from View.Target, so the
         * render texture must represent the real requested target size.
         */
        if (_renderTexture is null ||
            _renderTexture.Value.Texture.Width != RenderWidth ||
            _renderTexture.Value.Texture.Height != RenderHeight)
        {
            if (_renderTexture is not null)
                Raylib.UnloadRenderTexture(_renderTexture.Value);

            _renderTexture = Raylib.LoadRenderTexture(
                RenderWidth,
                RenderHeight);

            Raylib.SetTextureFilter(
                _renderTexture.Value.Texture,
                TextureFilter.Point);
        }

        var destinationRect = GetScreenRect();

        var destinationWidth = (int)destinationRect.Width;
        var destinationHeight = (int)destinationRect.Height;

        if (_renderTextureFinal is null ||
            _renderTextureFinal.Value.Texture.Width != destinationWidth ||
            _renderTextureFinal.Value.Texture.Height != destinationHeight)
        {
            if (_renderTextureFinal is not null)
                Raylib.UnloadRenderTexture(_renderTextureFinal.Value);

            _renderTextureFinal = Raylib.LoadRenderTexture(
                destinationWidth,
                destinationHeight);

            Raylib.SetTextureFilter(
                _renderTextureFinal.Value.Texture,
                TextureFilter.Point);
        }
    }
}
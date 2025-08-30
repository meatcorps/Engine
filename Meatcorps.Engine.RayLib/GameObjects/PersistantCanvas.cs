using Meatcorps.Engine.RayLib.Abstractions;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.GameObjects;

public class PersistentCanvas : BaseGameObject
{
    private RenderTexture2D _renderTexture;
    private readonly List<Action> _drawQueue = new();
    private bool _dirty = false;
    private readonly int _width;
    private readonly int _height;
    private readonly bool _clearOnStart;

    public Texture2D Texture => _renderTexture.Texture;

    public PersistentCanvas(int width, int height, bool clearOnStart = true)
    {
        _width = width;
        _height = height;
        _clearOnStart = clearOnStart;
    }

    protected override void OnInitialize()
    {
        _renderTexture = Raylib.LoadRenderTexture(_width, _height);

        if (_clearOnStart)
        {
            Raylib.BeginTextureMode(_renderTexture);
            Raylib.ClearBackground(new Color(0,0,0,0)); // transparent
            Raylib.EndTextureMode();
        }
    }

    /// <summary>
    /// Queue a drawing action that will be stamped onto the texture.
    /// </summary>
    public void AddDrawAction(Action draw)
    {
        _drawQueue.Add(draw);
        _dirty = true;
    }

    /// <summary>
    /// Optional fade-out or aging effect (use sparingly)
    /// </summary>
    public void FadeOut(float alpha = 0.01f)
    {
        AddDrawAction(() =>
        {
            Raylib.DrawRectangle(0, 0, _width, _height, Raylib.ColorAlpha(Color.Black, alpha));
        });
    }

    protected override void OnUpdate(float deltaTime)
    {
    }

    protected override void OnLateUpdate(float delta)
    {
        if (!_dirty) return;

        Raylib.BeginTextureMode(_renderTexture);
        foreach (var draw in _drawQueue)
            draw();
        Raylib.EndTextureMode();

        _drawQueue.Clear();
        _dirty = false;
    }

    protected override void OnDispose()
    {
        Raylib.UnloadRenderTexture(_renderTexture);
    }
}
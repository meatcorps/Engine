using Meatcorps.Engine.RayLib.Interfaces;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Resources;

public class OneTexture: ILoadAfterRayLibInit, IDisposable
{
    private readonly string _path;
    private readonly Action<Texture2D> _onLoaded = (_) => { };
    public Texture2D Texture { get; private set; }
    private bool _isDisposed;

    public OneTexture(string path)
    {
        _path = path;
    }
    
    public OneTexture(string path, Action<Texture2D> onLoaded)
    {
        _path = path;
        _onLoaded = onLoaded;
    }

    public void Load()
    {
        Texture = Raylib.LoadTexture(_path);
        _onLoaded(Texture);
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;
        Raylib.UnloadTexture(Texture);
        Texture = default;
        _isDisposed = true;
    }
}
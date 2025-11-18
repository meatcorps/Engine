using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Interfaces;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Resources;

public class OneTexture: ILoadAfterRayLibInit, IDisposable
{
    private readonly string _path;
    private readonly TextureFilter _filter;
    private readonly TextureWrap _wrap;
    private readonly Action<Texture2D> _onLoaded = (_) => { };
    public Texture2D Texture { get; private set; }
    private bool _isDisposed;

    public OneTexture(string path, TextureFilter filter = TextureFilter.Point, TextureWrap wrap = TextureWrap.Repeat)
    {
        _path = path;
        _filter = filter;
        _wrap = wrap;
    }
    
    public OneTexture(string path, Action<Texture2D> onLoaded)
    {
        _path = path;
        _onLoaded = onLoaded;
    }

    public void Load()
    {
        Texture = GlobalObjectManager.ObjectManager.Get<IRaylibResource>()!.LoadTexture(_path);
        Raylib.SetTextureFilter(Texture, _filter);
        Raylib.SetTextureWrap(Texture, _wrap);
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
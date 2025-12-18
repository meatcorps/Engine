using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Interfaces;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Resources;

public class OneTexture : IResourceLoadOnInit, IDisposable
{
    private readonly TextureFilter _filter;
    private readonly Action<Texture2D> _onLoaded = _ => { };
    private readonly string _path;
    private readonly TextureWrap _wrap;
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

    public Texture2D Texture { get; private set; }

    public void Dispose()
    {
        if (_isDisposed)
            return;
        Raylib.UnloadTexture(Texture);
        Texture = default;
        _isDisposed = true;
    }

    public int TotalResources => 1;
    public int ResourcesLoaded { get; private set; }

    public async Task Load()
    {
        Texture = await GlobalObjectManager.ObjectManager.Get<IRaylibResource>()!.LoadTexture(_path);
        await GlobalObjectManager.ObjectManager.Get<ResourceManager>()!.AddTaskToMainThread(() =>
        {
            Raylib.SetTextureFilter(Texture, _filter);
            Raylib.SetTextureWrap(Texture, _wrap);
        });
        ResourcesLoaded = 1;
        _onLoaded(Texture);
    }
}
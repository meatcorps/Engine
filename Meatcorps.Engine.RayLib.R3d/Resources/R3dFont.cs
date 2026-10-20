using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.R3d.Data;
using Meatcorps.Engine.RayLib.R3d.Interfaces;
using Meatcorps.Engine.RayLib.Resources;
using R3D_cs;

namespace Meatcorps.Engine.RayLib.R3d.Resources;

public sealed class R3dFont : IResourceLoadOnInit, IR3dDispose
{
    private readonly string _path;
    private TextFont _data;
    private R3dSettings? _settings = null;

    public ref TextFont DataRef => ref _data;
    public int TotalResources => 1;
    public int ResourcesLoaded { get; private set; }

    private R3dFont(string path)
    {
        _path = path;
        GlobalObjectManager.ObjectManager.RegisterList<IResourceLoadOnInit>();
        GlobalObjectManager.ObjectManager.Add<IResourceLoadOnInit>(this);
        GlobalObjectManager.ObjectManager.Register(this, path.Replace("\\", "/"));
        GlobalObjectManager.ObjectManager.RegisterList<IR3dDispose>();
        GlobalObjectManager.ObjectManager.Add<IR3dDispose>(this);
    }
    
    public static R3dFont Register(string path)
    {
        return new R3dFont(path);
    }
    
    public async Task Load()
    {
        var resource = GlobalObjectManager.ObjectManager.Get<IRaylibResource>()!;
        if (resource.Exists(_path))
        {
            await GlobalObjectManager.ObjectManager.Get<ResourceManager>()!.AddTaskToMainThread(() =>
            {
                var data = resource.LoadBytes(_path);
                _data = R3D.LoadTextFontFromMemory(data.AsSpan());
            });
        } else 
            throw new Exception($"3D font resource not found: " + _path);

        ResourcesLoaded = 1;
    }

    public void DisposeR3d()
    {
        R3D.UnloadTextFont(_data);
    }
}
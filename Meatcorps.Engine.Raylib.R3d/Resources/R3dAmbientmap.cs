using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.R3d.Data;
using Meatcorps.Engine.RayLib.R3d.Interfaces;
using Meatcorps.Engine.RayLib.Resources;
using R3D_cs;
using Raylib_cs;
using CubemapLayout = R3D_cs.CubemapLayout;
using Environment = R3D_cs.Environment;

namespace Meatcorps.Engine.RayLib.R3d.Resources;

public sealed class R3dAmbientmap : IResourceLoadOnInit, IR3dDispose
{
    private readonly string _path;
    private readonly CubemapLayout _layout;
    private readonly AmbientFlags _flags;
    private AmbientMap _data;
    private R3dSettings? _settings = null;

    public ref AmbientMap DataRef => ref _data;
    public int TotalResources => 1;
    public int ResourcesLoaded { get; private set; }

    private R3dAmbientmap(string path, CubemapLayout layout, AmbientFlags flags)
    {
        _path = path;
        _layout = layout;
        _flags = flags;
        GlobalObjectManager.ObjectManager.RegisterList<IResourceLoadOnInit>();
        GlobalObjectManager.ObjectManager.Add<IResourceLoadOnInit>(this);
        GlobalObjectManager.ObjectManager.Register(this, path.Replace("\\", "/"));
        GlobalObjectManager.ObjectManager.RegisterList<IR3dDispose>();
        GlobalObjectManager.ObjectManager.Add<IR3dDispose>(this);
    }
    
    public static R3dAmbientmap Register(string path, CubemapLayout layout = CubemapLayout.AutoDetect, AmbientFlags flags = AmbientFlags.Illumination)
    {
        return new R3dAmbientmap(path, layout, flags);
    }

    public void SetAsAmbient(float ambientEnergy = 0.3f)
    {
        _settings ??= GlobalObjectManager.ObjectManager.Get<R3dSettings>()!;
        _settings.EnvironmentChange((ref Environment env) =>
        {
            env.Ambient.Energy = ambientEnergy;
            env.Ambient.Map = _data;
        });
    }
    
    public async Task Load()
    {
        var resource = GlobalObjectManager.ObjectManager.Get<IRaylibResource>()!;
        if (resource.Exists(_path))
        {
            var image = await resource.LoadImage(_path);
            await GlobalObjectManager.ObjectManager.Get<ResourceManager>()!.AddTaskToMainThread(() =>
            {
                _data = R3D.LoadAmbientMapFromImage(image, _layout, _flags);
                Raylib.UnloadImage(image);
            });
        } else 
            throw new Exception($"Ambientmap resource not found: " + _path);

        ResourcesLoaded = 1;
    }

    public void DisposeR3d()
    {
        R3D.UnloadAmbientMap(_data);
    }
}
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

public sealed class R3dCubemap : IResourceLoadOnInit, IR3dDispose
{
    private readonly string _path;
    private readonly CubemapLayout _layout;
    private Cubemap _data;
    private R3dSettings? _settings = null;

    public ref Cubemap DataRef => ref _data;
    public int TotalResources => 1;
    public int ResourcesLoaded { get; private set; }

    public AmbientMap? AmbientMap = null;
    
    private R3dCubemap(string path, CubemapLayout layout)
    {
        _path = path;
        _layout = layout;
        GlobalObjectManager.ObjectManager.RegisterList<IResourceLoadOnInit>();
        GlobalObjectManager.ObjectManager.Add<IResourceLoadOnInit>(this);
        GlobalObjectManager.ObjectManager.Register(this, path.Replace("\\", "/"));
        
        GlobalObjectManager.ObjectManager.RegisterList<IR3dDispose>();
        GlobalObjectManager.ObjectManager.Add<IR3dDispose>(this);
    }
    
    public static R3dCubemap Register(string path, CubemapLayout layout = CubemapLayout.AutoDetect)
    {
        return new R3dCubemap(path, layout);
    }

    public void SetAsSkybox(float blur = 0.3f, float backgroundEnergy = 0.6f, float ambientEnergy = 0.3f)
    {
        _settings ??= GlobalObjectManager.ObjectManager.Get<R3dSettings>()!;
        AmbientMap ??= R3D.GenAmbientMap(_data, AmbientFlags.Illumination);
        _settings.EnvironmentChange((ref Environment env) =>
        {
            env.Background.Sky = _data;
            env.Background.SkyBlur = blur;
            env.Background.Energy = backgroundEnergy;
            env.Ambient.Energy = ambientEnergy;
            env.Ambient.Map = AmbientMap.Value;
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
                _data = R3D.LoadCubemapFromImage(image, _layout);
                Raylib.UnloadImage(image);
            });
        } else 
            throw new Exception($"Cubemap resource not found: " + _path);

        ResourcesLoaded = 1;
    }

    public void DisposeR3d()
    {
        R3D.UnloadCubemap(_data);
        if (AmbientMap != null)
            R3D.UnloadAmbientMap(AmbientMap.Value);
    }
}
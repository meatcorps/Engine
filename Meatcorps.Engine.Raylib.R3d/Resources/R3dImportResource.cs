using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.R3d.Interfaces;
using R3D_cs;

namespace Meatcorps.Engine.RayLib.R3d.Resources;

public sealed class R3dImportResource : IResourceLoadOnInit, IR3dDispose
{
    private readonly string _path;
    private ImportFlags _flags = ImportFlags.RetainMeshData | ImportFlags.RetainMeshNames;
    private Importer _importer;

    public int TotalResources => 1;
    public int ResourcesLoaded { get; private set; }

    private R3dImportResource(string path)
    {
        _path = path;
        GlobalObjectManager.ObjectManager.RegisterList<IResourceLoadOnInit>();
        GlobalObjectManager.ObjectManager.Add<IResourceLoadOnInit>(this);
        GlobalObjectManager.ObjectManager.Register(this, path.Replace("\\", "/"));
        GlobalObjectManager.ObjectManager.RegisterList<IR3dDispose>();
        GlobalObjectManager.ObjectManager.Add<IR3dDispose>(this);
    }

    public static R3dImportResource Register(string path)
    {
        return new R3dImportResource(path);
    }
    
    public R3dImportResource WithCustomFlags()
    {
        _flags = 0;
        return this;
    }
    
    public R3dImportResource WithFlag(ImportFlags flag)
    {
        _flags |= flag;
        return this;
    }
    
    public Model CreateModelInstance()
    {
        return R3D.LoadModelFromImporter(_importer);
    }
    
    public Skeleton CreateSkeletonInstance()
    {
        return R3D.LoadSkeletonFromImporter(_importer);
    }
    
    public AnimationLib CreateAnimationLibInstance()
    {
        return R3D.LoadAnimationLibFromImporter(_importer);
    }
    
    public Material[] CreateMaterialArrayInstance()
    {
        var total = 0;
        unsafe
        {
            var materials = R3D.LoadMaterialsFromImporter(_importer, ref total);
            var materialsArray = new Material[total];
            for (var i = 0; i < total; i++)
            {
                materialsArray[i] = materials[i];
            }

            return materialsArray;
        }
    }
    
    public async Task Load()
    {
        _importer = R3D.LoadImporter(_path, _flags);
        ResourcesLoaded = 1;
    }
    
    public void DisposeR3d()
    {
        R3D.UnloadImporter(_importer);
    }
}
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Interfaces;

namespace Meatcorps.Engine.RayLib.R3d.Resources;

public static class R3dRegistrationHelper
{
    public static void AutoLoadAssets(string baseFolder = "Assets", string cubemapFolder = "cubemaps", string ambientmapFolder = "ambientmaps")
    {
        var resource = GlobalObjectManager.ObjectManager.Get<IRaylibResource>();
        if (resource == null)
            throw new Exception("IRaylibResource not found");

        var files = resource.GetFiles(baseFolder, "*", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            IResourceLoadOnInit? type = null;
            if (file.EndsWith(".hdr", StringComparison.OrdinalIgnoreCase))
            {
                
                if (file.Contains(cubemapFolder, StringComparison.OrdinalIgnoreCase))
                    type = R3dCubemap.Register(file);
                else if (file.Contains(ambientmapFolder, StringComparison.OrdinalIgnoreCase))
                    type = R3dAmbientmap.Register(file);
                else
                    type = R3dCubemap.Register(file);
            }
            
            if (file.EndsWith(".glb", StringComparison.OrdinalIgnoreCase))
                type = R3dImportResource.Register(file);
            
            if (type != null)
                Console.WriteLine($"AUTO REGISTERED: ({type.GetType().Name}) {file}");
        }
    }
}
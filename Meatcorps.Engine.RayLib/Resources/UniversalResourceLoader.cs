using Meatcorps.Engine.RayLib.Interfaces;

namespace Meatcorps.Engine.RayLib.Resources;

public class UniversalResourceLoader: IResourceLoadOnInit
{
    private readonly Action _onLoaded;
    public int TotalResources => 0;
    public int ResourcesLoaded => 0;

    public UniversalResourceLoader(Action onLoaded)
    {
        _onLoaded = onLoaded;
    }
    
    public Task Load()
    {
        _onLoaded();
        return Task.CompletedTask;
    }
}
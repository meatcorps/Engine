namespace Meatcorps.Engine.RayLib.Interfaces;

public interface IResourceLoadOnInit
{
    int TotalResources { get; }
    int ResourcesLoaded { get; }
    Task Load();
}
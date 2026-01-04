namespace Meatcorps.Engine.Core.Interfaces.Config;

public interface IConfigChangeTracker
{
    public void ConfigChanged(string group, string key, object value);
}
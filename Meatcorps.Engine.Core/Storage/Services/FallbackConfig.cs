using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.Interfaces.Config;

namespace Meatcorps.Engine.Core.Storage.Services;

public class FallbackConfig : IUniversalConfig
{
    public string GetOrDefault(string group, string key, string defaultValue)
    {
        return defaultValue;
    }

    public void Set(string group, string key, string value)
    {
    }

    public int GetOrDefault(string group, string key, int defaultValue)
    {
        return defaultValue;
    }

    public void Set(string group, string key, int value)
    {
    }

    public float GetOrDefault(string group, string key, float defaultValue)
    {
        return defaultValue;
    }

    public void Set(string group, string key, float value)
    {
    }

    public bool GetOrDefault(string group, string key, bool defaultValue)
    {
        return defaultValue;
    }

    public void Set(string group, string key, bool value)
    {
    }

    public IEnumerable<string> GetGroups()
    {
        // Yes this will allocate GC. But you should not call this in a fallback state
        return new List<string>(); 
    }

    public IEnumerable<(string key, string value, ConfigValueType type)> GetKeys(string group)
    {
        // Yes this will allocate GC. But you should not call this in a fallback state
        return new List<(string key, string value, ConfigValueType type)>();
    }

    public void Save()
    {
    }
}
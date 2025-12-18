using Meatcorps.Engine.Core.Enums;

namespace Meatcorps.Engine.Core.Interfaces.Config;

public interface IUniversalConfig
{
    public string GetOrDefault(string group, string key, string defaultValue, bool expose = true);
    public void Set(string group, string key, string value);
    public int GetOrDefault(string group, string key, int defaultValue, bool expose = true);
    public void Set(string group, string key, int value);
    public float GetOrDefault(string group, string key, float defaultValue, bool expose = true);
    public void Set(string group, string key, float value);
    public bool GetOrDefault(string group, string key, bool defaultValue, bool expose = true);
    public void Set(string group, string key, bool value);
    public IEnumerable<string> GetGroups();
    public IEnumerable<(string key, string value, ConfigValueType type)> GetKeys(string group);
    public void Save();
}
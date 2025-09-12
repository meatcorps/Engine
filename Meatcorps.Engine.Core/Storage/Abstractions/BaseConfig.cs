using System.Globalization;
using System.Text.Json;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.ObjectManager;

namespace Meatcorps.Engine.Core.Storage.Abstractions;

[Serializable]
public abstract class BaseConfig<T>: IUniversalConfig, IDisposable where T : BaseConfig<T>, new()
{
    protected Dictionary<string, Dictionary<string, string>> SystemSettings = new();
    
    [NonSerialized]
    private Dictionary<string, ConfigValueType> _valueType = new();

    private bool _dirty;
    protected BaseConfig()
    {
        var fileInfo = new FileInfo("Config.json");
        Console.WriteLine($"Trying to load: " + fileInfo.FullName);
        if (File.Exists("Config.json"))
            SystemSettings = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(File.ReadAllText("Config.json")) ?? new();
        
        GlobalObjectManager.ObjectManager.Register<IUniversalConfig>(this);
        GlobalObjectManager.ObjectManager.Register<T>(Instance);
        DoRegisterDefaultValues();
    }

    protected abstract void DoRegisterDefaultValues();
    
    protected abstract T Instance { get; }

    public string GetOrDefault(string group, string key, string defaultValue)
    {
        if (!SystemSettings.ContainsKey(group))
            SystemSettings.Add(group, new Dictionary<string, string>());

        if (SystemSettings[group].TryAdd(key, defaultValue)) 
            Set(group, key, defaultValue);

        _valueType.TryAdd(group + ":" + key, ConfigValueType.IsString);

        return SystemSettings[group][key];
    }

    public void Set(string group, string key, string value)
    {
        if (!SystemSettings.ContainsKey(group))
            throw new InvalidOperationException("Group does not exist");
        if (!SystemSettings[group].ContainsKey(key))
            throw new InvalidOperationException("Key does not exist");
        SystemSettings[group][key] = value;
        _dirty = true;
    }

    public int GetOrDefault(string group, string key, int defaultValue)
    {
        _valueType.TryAdd(group + ":" + key, ConfigValueType.IsInt);
        if (int.TryParse(GetOrDefault(group, key, defaultValue.ToString(CultureInfo.InvariantCulture)), CultureInfo.InvariantCulture, out var result)) 
            return result;
        return defaultValue;
    }

    public void Set(string group, string key, int value)
    {
        Set(group, key, value.ToString(CultureInfo.InvariantCulture));
    }

    public float GetOrDefault(string group, string key, float defaultValue)
    {
        _valueType.TryAdd(group + ":" + key, ConfigValueType.IsFloat);
        if (float.TryParse(GetOrDefault(group, key, defaultValue.ToString(CultureInfo.InvariantCulture)), CultureInfo.InvariantCulture, out var result)) 
            return result;
        return defaultValue;
    }

    public void Set(string group, string key, float value)
    {
        Set(group, key, value.ToString(CultureInfo.InvariantCulture));
    }

    public bool GetOrDefault(string group, string key, bool defaultValue)
    {
        _valueType.TryAdd(group + ":" + key, ConfigValueType.IsBool);
        if (bool.TryParse(GetOrDefault(group, key, defaultValue.ToString()), out var result)) 
            return result;
        return defaultValue;
    }

    public void Set(string group, string key, bool value)
    {
        Set(group, key, value.ToString());
    }

    public IEnumerable<string> GetGroups()
    {
        foreach (var key in SystemSettings.Keys)
            yield return key;
    }

    public IEnumerable<(string key, string value, ConfigValueType type)> GetKeys(string group)
    {
        if (!SystemSettings.ContainsKey(group))
            throw new InvalidOperationException("Group does not exist");
        foreach (var key in SystemSettings[group].Keys)
        {
            var type = ConfigValueType.IsString;
            if (_valueType.ContainsKey(group + ":" + key))
                type = _valueType[group + ":" + key];
            yield return (key, SystemSettings[group][key], type);
        }
    }

    public void Save()
    {
        if (!_dirty)
            return;
        
        var json = JsonSerializer.Serialize(SystemSettings, new JsonSerializerOptions
        {
               WriteIndented = true,
        });
        File.WriteAllText("Config.json", json);
    }

    public void Dispose()
    {
        Save();
    }
}
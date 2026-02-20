using System.Globalization;
using System.Text.Json;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.ObjectManager;
// ReSharper disable VirtualMemberCallInConstructor

namespace Meatcorps.Engine.Core.Storage.Abstractions;

[Serializable]
public abstract class BaseConfig<T>: IUniversalConfig, IDisposable where T : BaseConfig<T>, new()
{
    protected Dictionary<string, Dictionary<string, string>> SystemSettings = new();
    
    [NonSerialized]
    private Dictionary<string, ConfigValueType> _valueType = new();
    private Dictionary<string, bool> _expose = new();
    
    private bool _dirty;
    private bool _running = true;
    protected BaseConfig()
    {
        var fileInfo = new FileInfo("Config.json");
        Console.WriteLine($"Trying to load: " + fileInfo.FullName);
        if (File.Exists("Config.json"))
            SystemSettings = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(File.ReadAllText("Config.json")) ?? new();
        
        GlobalObjectManager.ObjectManager.Register<IUniversalConfig>(this);
        GlobalObjectManager.ObjectManager.RegisterList<IConfigChangeTracker>();
        GlobalObjectManager.ObjectManager.Register(Instance);
        DoRegisterDefaultValues();

        Task.Run(async () =>
        {
            while (_running)
            {
                Save();
                await Task.Delay(2000);
            }
        });
    }

    protected abstract void DoRegisterDefaultValues();
    
    protected abstract T Instance { get; }

    public string GetOrDefault(string group, string key, string defaultValue, bool expose = true)
    {
        if (!SystemSettings.ContainsKey(group))
            SystemSettings.Add(group, new Dictionary<string, string>());

        if (SystemSettings[group].TryAdd(key, defaultValue))
            _dirty = true;

        _valueType.TryAdd(group + ":" + key, ConfigValueType.IsString);
        _expose.TryAdd(group + ":" + key, expose);

        if (expose)
            _expose.TryAdd(group, expose);
        
        return SystemSettings[group][key];
    }

    public void Set(string group, string key, string value)
    {
        if (!SystemSettings.TryGetValue(group, out var setting))
            throw new InvalidOperationException("Group does not exist " + group);
        if (!setting.ContainsKey(key))
            throw new InvalidOperationException("Key does not exist " + group + ":" + key);
        
        if (value == SystemSettings[group][key]) 
            return;
        
        SystemSettings[group][key] = value;
        
        foreach (var tracker in GlobalObjectManager.ObjectManager.GetList<IConfigChangeTracker>()!)
            tracker.ConfigChanged(group, key, value);
        
        _dirty = true;
    }

    public int GetOrDefault(string group, string key, int defaultValue, bool expose = true)
    {
        _valueType.TryAdd(group + ":" + key, ConfigValueType.IsInt);
        if (int.TryParse(GetOrDefault(group, key, defaultValue.ToString(CultureInfo.InvariantCulture), expose), CultureInfo.InvariantCulture, out var result)) 
            return result;
        return defaultValue;
    }

    public void Set(string group, string key, int value)
    {
        Set(group, key, value.ToString(CultureInfo.InvariantCulture));
    }

    public float GetOrDefault(string group, string key, float defaultValue, bool expose = true)
    {
        _valueType.TryAdd(group + ":" + key, ConfigValueType.IsFloat);
        if (float.TryParse(GetOrDefault(group, key, defaultValue.ToString(CultureInfo.InvariantCulture), expose), CultureInfo.InvariantCulture, out var result)) 
            return result;
        return defaultValue;
    }

    public void Set(string group, string key, float value)
    {
        Set(group, key, value.ToString(CultureInfo.InvariantCulture));
    }

    public bool GetOrDefault(string group, string key, bool defaultValue, bool expose = true)
    {
        _valueType.TryAdd(group + ":" + key, ConfigValueType.IsBool);
        if (bool.TryParse(GetOrDefault(group, key, defaultValue.ToString(), expose), out var result)) 
            return result;
        return defaultValue;
    }

    public void Set(string group, string key, bool value)
    {
        Set(group, key, value.ToString());
    }

    public IEnumerable<string> GetGroups()
    {
        foreach (var key in SystemSettings.Keys.ToArray())
        {
            if (!_expose.TryGetValue(key, out var value) || !value) 
                continue;
            yield return key;
        }
    }

    public IEnumerable<(string key, string value, ConfigValueType type)> GetKeys(string group)
    {
        if (!SystemSettings.ContainsKey(group))
            throw new InvalidOperationException("Group does not exist");
        foreach (var key in SystemSettings[group].Keys.ToArray())
        {
            if (!_expose.TryGetValue(group + ":" + key, out var value) || !value) 
                continue;
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
        _running = false;
        Save();
    }
}
using System.Text.Json;
using Meatcorps.Engine.Core.Interfaces.Storage;

namespace Meatcorps.Engine.Core.Storage.Services;

public class PersistentDatabase : Dictionary<string, object>, IKeyValueDatabase<string>
{
    public PersistentDatabase(IKeyValueLoader<string> loader, IKeyValueSaver<string> saver)
    {
        loader.GetData(this);
        saver.SetTarget(this);
    }

    public T GetOrDefault<T>(string key, T defaultValue)
    {
        if (TryGetValue(key, out var jsonDataRaw))
        {
            var jsonString = (string) jsonDataRaw;
            return defaultValue is string ? defaultValue : JsonSerializer.Deserialize<T>(jsonString) ?? defaultValue;
        }
        Set<T>(key, defaultValue);
        return defaultValue;
    }
    
    public void Set<T>(string key, T value)
    {
        var jsonString = JsonSerializer.Serialize(value);
        Remove(key);
        Add(key, jsonString);
        Dirty = true;
    }
    
    public bool Dirty { get; set; }
}
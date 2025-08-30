using System.ComponentModel;
using Meatcorps.Engine.Core.Interfaces.Storage;
using Meatcorps.Engine.Core.Storage.Enum;

namespace Meatcorps.Engine.Core.Storage.Services;

public class DataCollectionService : IKeyValueDatabaseCollection<StorageType, string>, IDisposable
{
    private readonly CancellationTokenRegistration _cancellationTokenRegistration = new();
    private readonly Dictionary<StorageType, IKeyValueDatabase<string>?> _collection = new();

    public DataCollectionService(ObjectManager.ObjectManager objectManager)
    {
        var persistentDatabase = objectManager.Get<PersistentDatabase>();
        if (persistentDatabase is not null)
            _collection.Add(StorageType.Persistent, persistentDatabase);
        
        var memoryDatabaseService = objectManager.Get<MemoryDatabaseService>();
        if (memoryDatabaseService is not null)
            _collection.Add(StorageType.InMemory, memoryDatabaseService);
    }
    
    public IKeyValueDatabaseCollection<StorageType, string> SetKeyValueDatabase(StorageType group,
        IKeyValueDatabase<string>? collection)
    {
        _collection.Add(group, collection);
        return this;
    }

    public void SetItem<T>(StorageType group, string key, T? data)
    {
        var collection = Collection(group);
        if (collection == null || data == null) return;
        if (collection.ContainsKey(key))
        {
            collection[key] = data;
            return;
        }

        collection.Add(key, data);
    }

    public T GetItem<T>(StorageType group, string key)
    {
        var collection = Collection(group);
        return (T) collection?[key]!;
    }

    public T? GetItemOrSetDefault<T>(StorageType group, string key, T? defaultData)
    {
        var collection = Collection(group);
        if (collection == null) return defaultData;
        if (collection.ContainsKey(key))
        {
            var value = collection[key];
            if (value is string && !(typeof(T) == typeof(string)))
            {
                var stringValue = value.ToString();
                if (stringValue != null)
                    return (T) TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(stringValue)!;
            }

            return (T) collection[key];
        }

        if (defaultData == null) 
            return default;
        
        collection.Add(key, defaultData);
        return defaultData;

    }

    public void RemoveItem(StorageType group, string key)
    {
        var collection = Collection(group);
        collection?.Remove(key);
    }

    public void Reset(StorageType group)
    {
        var collection = Collection(group);
        collection?.Clear();
    }

    public IKeyValueDatabase<string>? Collection(StorageType group)
    {
        return _collection.ContainsKey(group) ? _collection[group] : null;
    }
    
    public void Dispose()
    {
        _cancellationTokenRegistration.Dispose();
    }
}
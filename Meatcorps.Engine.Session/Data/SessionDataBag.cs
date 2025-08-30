using Meatcorps.Engine.Session.ValueTypes;

namespace Meatcorps.Engine.Session.Data;

public class SessionDataBag<TType> where TType : Enum
{
    private Dictionary<TType, ISessionDataItem> _items { get; } = new ();
    private Dictionary<IValueType, ISessionDataItem> _genericItems { get; } = new ();
    private Dictionary<Type, ISessionDataTypeSerializer> _serializers { get; } = new ();

    public SessionDataBag()
    {
        // default serializers
        _serializers.Add(typeof(int), new SessionDataTypeSerializerInt());
        _serializers.Add(typeof(float), new SessionDataTypeSerializerFloat());
        _serializers.Add(typeof(string), new SessionDataTypeSerializerString());
    }
    
    public SessionDataBag<TType> RegisterSerializer(ISessionDataTypeSerializer serializer)
    {
        if (_serializers.ContainsKey(serializer.Type))
            throw new Exception("Serializer already registered");
        
        _serializers.Add(serializer.Type, serializer);
        return this;
    }

    public SessionDataBag<TType> RegisterItem(ISessionDataItem<TType> item)
    {
        if (!_serializers.ContainsKey(item.Type))
            throw new Exception($"Serializer for this type not registered {item.Type.Name} - use RegisterSerializer");

        if (_items.ContainsKey(item.Key))
            throw new Exception($"Item already registered: {item.Key}");
        _items.Add(item.Key, item);
        return this;
    }
    
    internal SessionDataBag<TType> RegisterItem<T>(SessionDataItemUniversal<T> item)
    {
        if (!_serializers.ContainsKey(item.Type))
            throw new Exception($"Serializer for this type not registered {item.Type.Name} - use RegisterSerializer");
        _genericItems.Add(item.ValueType, item);
        return this;
    }
    
    public T Get<T>(TType key) 
    {
        if (_items[key] is not ISessionDataValue<T> sessionDataValue)
            throw new Exception("Invalid type");
        
        return sessionDataValue.Value;
    }
    
    internal T Get<T>(IValueType type) 
    {
        if (_genericItems[type] is not ISessionDataValue<T> sessionDataValue)
            throw new Exception("Invalid type");
        
        return sessionDataValue.Value;
    }

    public void Set<T>(TType key, T value)
    {
        if (_items[key] is not ISessionDataValue<T> sessionDataValue)
            throw new Exception("Invalid type");
           
        sessionDataValue.Value = value;
    } 
    
    public bool TryGet<T>(TType key, out T value)
    {
        if (_items.TryGetValue(key, out var item) && item is ISessionDataValue<T> v)
        {
            value = v.Value;
            return true;
        }
        value = default!;
        return false;
    }

    public T GetOrDefault<T>(TType key, T fallback)
    {
        return TryGet<T>(key, out var v) ? v : fallback;
    }

    public bool TrySet<T>(TType key, T value)
    {
        if (_items.TryGetValue(key, out var item) && item is ISessionDataValue<T> v)
        {
            v.Value = value;
            return true;
        }
        return false;
    }
    
    public void Reset()
    {
        foreach (var item in _items.Values)
        {
            item.Reset();
        }
    }

    public IReadOnlyDictionary<string, string> Serialize()
    {
        var result = new Dictionary<string, string>();
        foreach (var item in _items.Values)
        {
            if (!_serializers.TryGetValue(item.Type, out var serializer))
                throw new Exception($"Type not registered {item.Type.Name}");

            result[GetKeyName(item)] = serializer.Serialize(item);
        }
        foreach (var item in _genericItems.Values)
        {
            if (!_serializers.TryGetValue(item.Type, out var serializer))
                throw new Exception($"Type not registered {item.Type.Name}");

            result[GetKeyName(item)] = serializer.Serialize(item);
        }
        return result;
    }
    
    public void Deserialize(IReadOnlyDictionary<string, string> data)
    {
        foreach (var item in _items.Values)
        {
            var key = GetKeyName(item);
            if (!data.TryGetValue(key, out var raw))
                continue;
            if (!_serializers.TryGetValue(item.Type, out var serializer))
                throw new Exception($"Type not registered {item.Type.Name}");
            serializer.Deserialize(raw, item);
        }
        foreach (var item in _genericItems.Values)
        {
            var key = GetKeyName(item);
            if (!data.TryGetValue(key, out var raw))
                continue;
            if (!_serializers.TryGetValue(item.Type, out var serializer))
                throw new Exception($"Type not registered {item.Type.Name}");
            serializer.Deserialize(raw, item);
        }
    }

    private string GetKeyName(ISessionDataItem item)
    {
        return $"{typeof(TType).FullName}:{item.Name}";
    }
}
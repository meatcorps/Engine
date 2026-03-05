using System.Globalization;
using Meatcorps.Engine.Session.ValueTypes;
using Newtonsoft.Json;

namespace Meatcorps.Engine.Session.Data;

public class SessionDataItem<TType, TValue>: ISessionDataItem<TType>, ISessionDataValue<TValue> where TType : Enum
{
    public Type Type => typeof(TValue);
    public TType Key { get; }
    public string Name { get; }
    public TValue Value { get; set; }
    private TValue DefaultValue { get; }

    public SessionDataItem(TType key, string? name, TValue defaultValue)
    {
        Key = key;
        Name = string.IsNullOrWhiteSpace(name) ? key.ToString() : name;
        DefaultValue = defaultValue;
        Value = defaultValue;
    }
    
    public void Reset()
    {
        Value = DefaultValue;
    }
}

public class SessionDataItemUniversal<TValue>: ISessionDataValue<TValue>
{
    public Type Type => typeof(TValue);
    public string Name { get; }
    public TValue Value { get; set; }
    public IValueType ValueType { get; }
    private TValue DefaultValue { get; }

    public SessionDataItemUniversal(IValueType name, TValue defaultValue)
    {
        ValueType = name;
        Name = name.ToString();
        DefaultValue = defaultValue;
        Value = defaultValue;
    }
    
    public virtual void Reset()
    {
        Value = DefaultValue;
    }
}

public class SessionDataItemComplex<TEnum, TValue> : SessionDataItem<TEnum, TValue>, ISessionDataTypeSerializer where TEnum : Enum where TValue : class
{
    private readonly JsonSerializerSettings _serializerSettings;

    public SessionDataItemComplex(TEnum type, string name, TValue defaultValue, JsonSerializerSettings serializerSettings) : base(type, name, defaultValue)
    {
        _serializerSettings = serializerSettings;
    }

    public string Serialize(ISessionDataItem _)
    {
        return JsonConvert.SerializeObject(Value, _serializerSettings);
    }

    public void Deserialize(string value, ISessionDataItem _)
    {
        var objectValue = JsonConvert.DeserializeObject<TValue>(value, _serializerSettings);
        if (objectValue == null)
            throw new Exception("Failed to deserialize value");
        
        Value = objectValue;
    }
}

public class SessionDataItemUniversalDate : SessionDataItemUniversal<string>
{
    public SessionDataItemUniversalDate(IValueType name) : base(name, DateTime.Now.ToString(CultureInfo.InvariantCulture))
    {
    }

    public override void Reset()
    {
        Value = DateTime.Now.ToString(CultureInfo.InvariantCulture);   
    }
}

public interface ISessionDataItem<TEnum>: ISessionDataItem where TEnum : Enum
{
    public TEnum Key { get; }
}

public interface ISessionDataValue<TValue>: ISessionDataItem
{
    public TValue Value { get; set; }
}

public interface ISessionDataItem
{
    public Type Type { get; }
    public string Name { get; }
    public void Reset();
}
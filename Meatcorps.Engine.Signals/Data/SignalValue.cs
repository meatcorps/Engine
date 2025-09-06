using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Signals.Interfaces;

namespace Meatcorps.Engine.Signals.Data;

public interface ISignalValueTracker
{
    string Topic { get; }
    string GroupName { get; }
    void UpdateValueFromTracker(object value);
}

public  class SignalValue<TValueType, TGroup> : IEqualityComparer<SignalValue<TValueType, TGroup>>, IDisposable, ISignalValueTracker where TGroup : Enum
{
    private TValueType _value;
    private ObjectManager _objectManager;
    public TGroup Group { get; }
    public string GroupName => Group.ToString();
    public string Topic { get; init; }
    public event ValueChangedEventHandler ValueChanged = _ => { };
    public delegate void ValueChangedEventHandler(TValueType value);
    
    public TValueType Value
    {
        get => _value;
        set
        {
            if (!_value?.Equals(value) ?? false)
                return;
            
            _value = value;
            ValueChanged.Invoke(_value);
            SentChangeToTrackers();
        }
    }
    
    public SignalValue(TGroup group, string topic, TValueType initialValue, ObjectManager? manager = null)
    {
        Group = group;
        Topic = topic;
        _value = initialValue;
        _objectManager = manager  ?? GlobalObjectManager.ObjectManager;
        
        foreach (var valueEvent in AllValueEventTrackers)
        {
            if (valueEvent.GetGroup().Equals(Group))
                valueEvent.Register(this);
        }
    }

    public void UpdateValueFromTracker(object value)
    {
        if (value is not TValueType valueType)
            return;
        
        if (EqualityComparer<TValueType>.Default.Equals(_value, valueType))
            return;
            
        _value = valueType;
        ValueChanged.Invoke(_value);
    }

    private void SentChangeToTrackers()
    {
        foreach (var valueEvent in AllValueEventTrackers)
        {
            if (valueEvent.GetGroup().Equals(Group))
                valueEvent.OnValueChanged(this);
        }
    }

    private IEnumerable<ISignalValueEvent<TGroup>> AllValueEventTrackers
        => _objectManager.GetSet<ISignalValueEvent<TGroup>>() ??
           throw new NullReferenceException("Signal Value Event systems not correctly setup");

    public bool Equals(SignalValue<TValueType, TGroup>? x, SignalValue<TValueType, TGroup>? y)
        => !(x is null || y is null) && EqualityComparer<TGroup>.Default.Equals(x.Group, y.Group) && x.Topic == y.Topic;
    
    public int GetHashCode(SignalValue<TValueType, TGroup> obj) => HashCode.Combine(obj.Group, obj.Topic);

    
    public void Dispose()
    {
        foreach (var valueEvent in AllValueEventTrackers)
        {
            if (valueEvent.GetGroup().Equals(Group))
                valueEvent.Unregister(this);
        }
    }
}
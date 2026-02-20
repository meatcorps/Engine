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
    private TValueType? _value;
    private readonly ObjectManager _objectManager;
    public TGroup Group { get; }
    public string GroupName => Group.ToString();
    public string Topic { get; init; }
    public event ValueChangedEventHandler ValueChanged = _ => { };
    public event ValueChangedEventHandler IncomingValue = _ => { };
    public delegate void ValueChangedEventHandler(TValueType value);
    
    public TValueType Value
    {
        get => (_value ?? default)!;
        set
        {
            if (_value?.Equals(value) ?? false)
                return;
            
            _value = value;
            Push();
        }
    }
    
    public SignalValue(TGroup group, string topic, TValueType? initialValue = default, ObjectManager? manager = null)
    {
        Group = group;
        Topic = topic;
        _objectManager = manager  ?? GlobalObjectManager.ObjectManager;
        var valueSet = false;
        
        foreach (var valueEvent in AllValueEventTrackers)
        {
            if (valueEvent.GetGroup().Equals(Group))
            {
                if (valueEvent.Register(this, initialValue, out _))
                    valueSet = true;
            }
        }
        
        if (!valueSet)
            _value = initialValue ?? throw new NullReferenceException("Initial value cannot be null when the main value is not found by the tracker");
    }

    public void Push()
    {
        ValueChanged.Invoke(Value);
        SentChangeToTrackers();
    }

    public void UpdateValueFromTracker(object value)
    {
        if (value is not TValueType valueType)
            return;
        
        IncomingValue.Invoke(valueType);
        
        if (_value?.Equals(value) ?? false)
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
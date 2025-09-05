using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Signals.Interfaces;

namespace Meatcorps.Engine.Signals.Data;

public  class SignalValue<TValueType, TGroup> : IEqualityComparer<SignalValue<TValueType, TGroup>>, IDisposable where TGroup : Enum
{
    private TValueType _value;
    private ObjectManager _objectManager;
    public TGroup Group { get; }
    public string Topic { get; init; }
    
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
    
    public ValueChangedEventHandler ValueChanged { get; set; }
    
    public delegate void ValueChangedEventHandler(TValueType value);
    
    public SignalValue(TGroup group, string name, TValueType initialValue, ObjectManager? manager = null)
    {
        _objectManager = manager  ?? GlobalObjectManager.ObjectManager;
        
        foreach (var valueEvent in AllValueEventTrackers)
        {
            if (valueEvent.GetGroup().Equals(Group))
                valueEvent.Register(this);
        }
    }

    public void UpdateValueFromTracker(TValueType value)
    {
        if (!_value?.Equals(value) ?? false)
            return;
            
        _value = value;
        ValueChanged.Invoke(_value);
        
        _value = value;
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
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
        if (x.GetType() != y.GetType()) return false;
        return EqualityComparer<TGroup>.Default.Equals(x.Group, y.Group) && x.Topic == y.Topic;
    }

    public int GetHashCode(SignalValue<TValueType, TGroup> obj)
    {
        return HashCode.Combine(obj.Group, obj.Topic);
    }
    
    public void Dispose()
    {
        foreach (var valueEvent in AllValueEventTrackers)
        {
            if (valueEvent.GetGroup().Equals(Group))
                valueEvent.Unregister(this);
        }
    }
}
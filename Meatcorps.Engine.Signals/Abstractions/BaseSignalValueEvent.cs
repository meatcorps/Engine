using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Signals.Data;
using Meatcorps.Engine.Signals.Interfaces;

namespace Meatcorps.Engine.Signals.Abstractions;

public abstract class BaseSignalValueEvent<TGroup>: IBackgroundService, ISignalValueEvent<TGroup>, IDisposable where TGroup : Enum
{
    private readonly Dictionary<Type, HashSet<object>> _values = new ();
    private readonly Dictionary<string, object> _subjects = new ();
    private readonly CancellationDisposable  _cancellationDisposable = new();
    protected CancellationToken AliveToken => _cancellationDisposable.Token;
    private bool _disposed = false;
    private readonly object _gate = new();
    
    public void PreUpdate(float deltaTime)
    {
    }

    public void Update(float deltaTime)
    {
    }

    public void LateUpdate(float deltaTime)
    {
    }

    public abstract TGroup GetGroup();

    protected ISubject<TValueType> GetSubject<TValueType>(string topic)
    {
        lock (_gate)
        {
            if (_subjects.ContainsKey(topic))
                return (Subject<TValueType>)_subjects[topic];

            var subject = new Subject<TValueType>();

            subject
                .DistinctUntilChanged()
                .Subscribe(x => SetValue(topic, x), AliveToken);

            _subjects.Add(topic, subject);

            return subject;
        }
    }
    
    public bool Register<TValueType>(SignalValue<TValueType, TGroup> value, in TValueType? initialValue, out TValueType? currentValue)
    {
        lock (_gate)
        {
            if (initialValue is not null)
                value.UpdateValueFromTracker(initialValue);
            
            var havingAValue = TryGetValue<TValueType>(value.Topic, out var existingValue);
            
            if (havingAValue)
                value.UpdateValueFromTracker(existingValue);

            IsValueTypeOk(value);
            
            _values[value.Value!.GetType()].Add(value);

            if (havingAValue)
                currentValue = value.Value;
            else
                currentValue = default;
            
            return havingAValue;
        }
    }

    public void Unregister<TValueType>(SignalValue<TValueType, TGroup> value)
    {
        lock (_gate)
        {
            IsValueTypeOk(value);

            _values[value.Value!.GetType()].Remove(value);
        }
    }

    public void OnValueChanged<TValueType>(SignalValue<TValueType, TGroup> value)
    {
        if (_subjects.TryGetValue(value.Topic, out var subject) && subject is Subject<TValueType> subjectType)
        {
            if (!subjectType.IsDisposed)
                subjectType.OnNext(value.Value);
        }

        lock (_gate)
        {
            foreach (var item in _values[value.Value!.GetType()])
            {
                if (item is not SignalValue<TValueType, TGroup> other)
                    continue;

                if (other.Group.Equals(GetGroup()) && other.Topic.Equals(value.Topic))
                    other.UpdateValueFromTracker(value.Value);
            }
        }
    }

    protected void SetValue<TValueType>(string topic, TValueType value)
    {
        if (value is null)
            return;
        lock (_gate)
        {
            foreach (var item in _values[value!.GetType()])
            {
                if (item is not ISignalValueTracker other)
                    continue;
                if (other.GroupName.Equals(GetGroup().ToString()) && other.Topic.Equals(topic))
                    other.UpdateValueFromTracker(value);
            }
        }
    }

    protected bool TryGetValue<TValueType>(string topic, out TValueType? value)
    {
        lock (_gate)
        {
            if (!_values.ContainsKey(typeof(TValueType)))
            {
                value = default;
                return false;
            }

            foreach (var item in _values[typeof(TValueType)])
            {
                if (item is not SignalValue<TValueType, TGroup> other)
                    continue;
                if (other.Group.Equals(GetGroup()) && other.Topic.Equals(topic))
                {
                    value = other.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }
    }

    private void IsValueTypeOk<TValueType>(SignalValue<TValueType, TGroup> value)
    {
        if (value.Value == null)
            throw new NullReferenceException($"{nameof(value.Value)} cannot be null");
        var type = value.Value.GetType();

        if (!_values.ContainsKey(type))
            _values[type] = new HashSet<object>();
    }

    protected virtual void OnDispose(bool disposing)
    {
        
    }
    
    public void Dispose()
    {
        if (_disposed)
            return;
        
        OnDispose(!_disposed);

        foreach (var subject in _subjects.Values)
        {
            if (subject is IDisposable disposable)
                disposable.Dispose();
        }
        if (!_cancellationDisposable.IsDisposed)
            _cancellationDisposable.Dispose();
        
        _disposed = true;
    }
}
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Signals.Data;
using Meatcorps.Engine.Signals.Interfaces;

namespace Meatcorps.Engine.Signals.Abstractions;

/// <summary>
/// Abstract base class for signal tracker backends.
/// Handles registration, deduplication, and in-process broadcasting of <see cref="SignalValue{TValueType,TGroup}"/> changes
/// using Reactive Extensions (<c>DistinctUntilChanged</c> subjects per topic).
/// <para>
/// Extend this class to implement custom transport backends (e.g. MQTT, WebSocket, Redis).
/// The only required override is <see cref="GetGroup"/>. Use <see cref="GetSubject{TValueType}"/>
/// and <see cref="SetValue{TValueType}"/> to integrate incoming messages from your transport layer.
/// </para>
/// </summary>
public abstract class BaseSignalValueEvent<TGroup>: IBackgroundService, ISignalValueEvent<TGroup>, IDisposable where TGroup : Enum
{
    private readonly Dictionary<Type, HashSet<object>> _values = new ();
    private readonly Dictionary<string, object> _subjects = new ();
    private readonly CancellationDisposable  _cancellationDisposable = new();
    /// <summary>
    /// A <see cref="CancellationToken"/> that is cancelled when this tracker is disposed.
    /// Use this to tie the lifetime of transport subscriptions to the tracker's lifetime.
    /// </summary>
    protected CancellationToken AliveToken => _cancellationDisposable.Token;
    private bool _disposed;
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

    /// <summary>Returns the group enum value this tracker handles. Must match the group used when creating signal values.</summary>
    public abstract TGroup GetGroup();

    /// <summary>
    /// Returns the Rx <see cref="ISubject{T}"/> for the given topic, creating it if it does not exist.
    /// The subject applies <c>DistinctUntilChanged</c> and broadcasts new values to all registered signals on that topic.
    /// <para>
    /// Call <c>subject.OnNext(value)</c> from your transport layer when an incoming message arrives
    /// to push it into the local signal graph.
    /// </para>
    /// </summary>
    /// <param name="topic">The signal topic string.</param>
    protected ISubject<TValueType> GetSubject<TValueType>(string topic)
    {
        lock (_gate)
        {
            if (_subjects.TryGetValue(topic, out var subject1))
                return (Subject<TValueType>)subject1;

            var subject = new Subject<TValueType>();

            subject
                .DistinctUntilChanged()
                .Subscribe(x => SetValue(topic, x), AliveToken);

            _subjects.Add(topic, subject);

            return subject;
        }
    }

    /// <summary>
    /// Registers a signal value and optionally updates it with the specified initial value.
    /// Returns whether a value already exists for the given topic, and outputs the current value if available.
    /// </summary>
    /// <typeparam name="TValueType">The type of the signal value being registered.</typeparam>
    /// <param name="value">The signal value object to register.</param>
    /// <param name="initialValue">An optional initial value to set.</param>
    /// <param name="currentValue">The current value of the signal, if one exists.</param>
    /// <returns>True if a value already exists for the signal's topic, otherwise false.</returns>
    public bool Register<TValueType>(SignalValue<TValueType, TGroup> value, in TValueType? initialValue,
        out TValueType? currentValue)
    {
        lock (_gate)
        {
            if (initialValue is not null)
                value.UpdateValueFromTracker(initialValue);
            
            var havingAValue = TryGetValue<TValueType>(value.Topic, out var existingValue);
            
            if (havingAValue && existingValue is not null)
                value.UpdateValueFromTracker(existingValue);

            IsValueTypeOk(value);
            
            _values[value.Value!.GetType()].Add(value);

            currentValue = havingAValue ? value.Value : default;
            
            return havingAValue;
        }
    }

    /// <summary>
    /// Unregisters a previously registered signal value from the event tracker. Ensures the signal value is no longer tracked or managed.
    /// </summary>
    /// <typeparam name="TValueType">The type of value associated with the signal.</typeparam>
    /// <param name="value">The signal value instance to be unregistered.</param>
    public void Unregister<TValueType>(SignalValue<TValueType, TGroup> value)
    {
        lock (_gate)
        {
            IsValueTypeOk(value);

            _values[value.Value!.GetType()].Remove(value);
        }
    }

    /// <summary>
    /// Notifies about a change in the value of the provided signal and propagates the updated value to relevant subscribers.
    /// </summary>
    /// <param name="value">The signal value instance whose change needs to be notified.</param>
    /// <typeparam name="TValueType">The type of the value associated with the signal.</typeparam>
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

    /// <summary>
    /// Pushes a value directly to all registered signals matching the given topic.
    /// Use this as an alternative to <see cref="GetSubject{TValueType}"/> when you want to
    /// deliver an incoming transport message without going through the Rx subject pipeline.
    /// </summary>
    /// <param name="topic">The signal topic to target.</param>
    /// <param name="value">The value to deliver.</param>
    protected void SetValue<TValueType>(string topic, TValueType value)
    {
        if (value is null)
            return;
        lock (_gate)
        {
            foreach (var item in _values[value.GetType()])
            {
                if (item is not ISignalValueTracker other)
                    continue;
                if (other.GroupName.Equals(GetGroup().ToString()) && other.Topic.Equals(topic))
                    other.UpdateValueFromTracker(value);
            }
        }
    }

    /// <summary>
    /// Attempts to retrieve the current value held by any registered signal for the given topic.
    /// Useful during registration to seed a newly registered signal
    /// with the last known value.
    /// </summary>
    /// <param name="topic">The signal topic to look up.</param>
    /// <param name="value">The current value if found, otherwise default.</param>
    /// <returns><c>true</c> if a value was found for the topic.</returns>
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

    /// <summary>
    /// Override to release custom resources held by your tracker implementation.
    /// Called once during <see cref="Dispose"/> before Rx subjects and the cancellation token are cleaned up.
    /// </summary>
    /// <param name="disposing"><c>true</c> if called from <see cref="Dispose"/>.</param>
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
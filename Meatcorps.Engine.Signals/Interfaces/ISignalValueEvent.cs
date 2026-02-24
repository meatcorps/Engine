using Meatcorps.Engine.Signals.Data;

namespace Meatcorps.Engine.Signals.Interfaces;

/// <summary>
/// Represents a signal tracker for a specific <typeparamref name="TGroup"/>.
/// Implementations handle routing value changes between all registered <see cref="SignalValue{TValueType,TGroup}"/>
/// instances that share the same group and topic.
/// </summary>
public interface ISignalValueEvent<TGroup> where TGroup : Enum
{
    /// <summary>Returns the group enum value this tracker is responsible for.</summary>
    TGroup GetGroup();

    /// <summary>
    /// Registers a <see cref="SignalValue{TValueType,TGroup}"/> with this tracker.
    /// If a value already exists for the topic, it is pushed to the signal immediately.
    /// </summary>
    /// <param name="value">The signal value to register.</param>
    /// <param name="initialValue">Optional initial value to seed the signal with.</param>
    /// <param name="currentValue">The current value for the topic if one already exists, otherwise default.</param>
    /// <returns><c>true</c> if an existing value was found for the topic and applied; <c>false</c> if this is the first registration.</returns>
    bool Register<TValueType>(SignalValue<TValueType, TGroup> value, in TValueType? initialValue, out TValueType? currentValue);

    /// <summary>Removes a <see cref="SignalValue{TValueType,TGroup}"/> from this tracker. It will no longer receive updates.</summary>
    void Unregister<TValueType>(SignalValue<TValueType, TGroup> value);

    /// <summary>
    /// Called by a <see cref="SignalValue{TValueType,TGroup}"/> when its value changes.
    /// Broadcasts the new value to all other registered signals with the same group and topic.
    /// </summary>
    void OnValueChanged<TValueType>(SignalValue<TValueType, TGroup> value);
}
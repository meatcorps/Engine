using Meatcorps.Engine.Signals.Data;

namespace Meatcorps.Engine.Signals.Interfaces;

public interface ISignalValueEvent<TGroup> where TGroup : Enum
{
    TGroup GetGroup();
    bool Register<TValueType>(SignalValue<TValueType, TGroup> value, in TValueType? initialValue, out TValueType? currentValue);
    void Unregister<TValueType>(SignalValue<TValueType, TGroup> value);
    void OnValueChanged<TValueType>(SignalValue<TValueType, TGroup> value);
}
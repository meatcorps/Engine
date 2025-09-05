using Meatcorps.Engine.Signals.Data;

namespace Meatcorps.Engine.Signals.Interfaces;

public interface ISignalValueEvent<TGroup> where TGroup : Enum
{
    TGroup GetGroup();
    void Register<TValueType>(SignalValue<TValueType, TGroup> value);
    void Unregister<TValueType>(SignalValue<TValueType, TGroup> value);
    void OnValueChanged<TValueType>(SignalValue<TValueType, TGroup> value);
}
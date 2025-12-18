using Meatcorps.Engine.Core.Input;

namespace Meatcorps.Engine.Core.Interfaces.Input;

public interface IGenericInputMapSaver<T> where T : Enum
{
    public GenericInput LoadFromConfig(int profile, T input, GenericInput map);
    public void SaveToConfig(int profile, T input, GenericInput map);

    public GenericInput? DefaultMap(int profile, T input);
}
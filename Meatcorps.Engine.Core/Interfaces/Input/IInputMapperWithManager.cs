namespace Meatcorps.Engine.Core.Interfaces.Input;

public interface IInputMapperWithManager<in T, out TManager> : IInputMapper<T> where T : Enum
{
    TManager Manager { get; }
}
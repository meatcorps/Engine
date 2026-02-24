namespace Meatcorps.Engine.Core.Interfaces.Input;

/// <summary>
/// Extends <see cref="IInputMapper{T}"/> with access to an underlying hardware manager of type <typeparamref name="TManager"/>.
/// Use this when device-specific features (e.g. button lights, force feedback) are needed beyond standard input state.
/// </summary>
public interface IInputMapperWithManager<in T, out TManager> : IInputMapper<T> where T : Enum
{
    /// <summary>The underlying hardware manager for this mapper.</summary>
    TManager Manager { get; }
}
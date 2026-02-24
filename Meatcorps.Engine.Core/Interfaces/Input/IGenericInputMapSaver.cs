using Meatcorps.Engine.Core.Input;

namespace Meatcorps.Engine.Core.Interfaces.Input;

/// <summary>
/// Provides persistence for <see cref="GenericInput"/> bindings, loading from config on startup
/// and saving when the player remaps a control.
/// Implement this to support per-profile remapping that survives restarts.
/// </summary>
public interface IGenericInputMapSaver<T> where T : Enum
{
    /// <summary>
    /// Loads the saved binding for <paramref name="input"/> on <paramref name="profile"/> from config.
    /// Returns <paramref name="map"/> unchanged if no saved binding exists.
    /// </summary>
    public GenericInput LoadFromConfig(int profile, T input, GenericInput map);

    /// <summary>Persists the current binding for <paramref name="input"/> on <paramref name="profile"/> to config.</summary>
    public void SaveToConfig(int profile, T input, GenericInput map);

    /// <summary>
    /// Returns the factory-default <see cref="GenericInput"/> for <paramref name="input"/> on <paramref name="profile"/>,
    /// or <c>null</c> if no default is defined. Used when resetting a binding.
    /// </summary>
    public GenericInput? DefaultMap(int profile, T input);
}
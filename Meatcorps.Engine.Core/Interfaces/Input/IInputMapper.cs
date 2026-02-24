using System.Numerics;
using Meatcorps.Engine.Core.Enums;

namespace Meatcorps.Engine.Core.Interfaces.Input;

/// <summary>
/// Player-indexed access to input state for a given input enum <typeparamref name="T"/>.
/// <para>
/// Separates <b>profiles</b> (physical device configurations) from <b>players</b> (logical game slots, 1-based).
/// A profile is registered once with its bindings and then assigned to a player index at runtime,
/// enabling hot-swap and auto-assignment workflows.
/// </para>
/// </summary>
public interface IInputMapper<in T> where T : Enum
{
    /// <summary>Returns the <see cref="IInput"/> state for <paramref name="input"/> on the given player slot.</summary>
    IInput GetState(int player, T input);

    /// <summary>Returns the combined directional axis for the given player and axis index (default 1).</summary>
    Vector2 GetAxis(int player, int axis = 1);

    /// <summary>Triggers rumble on the given player's device. No-op if the device does not support it.</summary>
    void Rumble(int player, float left, float right, float duration);

    /// <summary>Assigns a profile to a player slot. If the profile is already assigned elsewhere it is unassigned first.</summary>
    void AssignProfile(int profileId, int player);

    /// <summary>Removes any profile assignment from the given player slot.</summary>
    void UnassignProfile(int player);

    /// <summary>Returns <c>true</c> if a profile is currently assigned to the given player slot.</summary>
    bool IsAssigned(int player);

    /// <summary>Returns <c>true</c> if the given player slot has an active, connected device.</summary>
    bool IsConnected(int player);

    /// <summary>
    /// Scans all profiles for any pressed input and returns the profile and player that triggered it.
    /// Returns <c>false</c> if nothing is pressed. Used for auto-assignment workflows.
    /// </summary>
    bool AnyInputPressed(out int profileId, out int playerId);

    /// <summary>Returns the <see cref="PlayerInputType"/> (keyboard, gamepad, etc.) for the given player.</summary>
    PlayerInputType InputType(int player);

    /// <summary>Returns all registered profile IDs available in this mapper.</summary>
    IReadOnlyList<int> GetAvailableProfiles();
}
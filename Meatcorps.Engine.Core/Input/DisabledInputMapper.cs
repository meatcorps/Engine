using System.Numerics;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.Interfaces.Input;

namespace Meatcorps.Engine.Core.Input;

public class DisabledInputMapper<T>: IInputMapper<T> where T : Enum
{
    private readonly GenericInput _defaultInput = new GenericInput(() => 0, "UNKNOWN");
    private readonly List<int> _availableProfiles = new List<int>();
    
    public IInput GetState(int player, T input)
    {
        return _defaultInput;
    }

    public Vector2 GetAxis(int player, int axis = 1)
    {
        return Vector2.Zero;
    }

    public void Rumble(int player, float left, float right, float duration)
    {
        // Nop
    }

    public void AssignProfile(int profileId, int player)
    {
        // Nop
    }

    public void UnassignProfile(int player)
    {
        // Nop
    }

    public bool IsAssigned(int player)
    {
        return true;
    }

    public bool IsConnected(int player)
    {
        return true;
    }

    public bool AnyInputPressed(out int profileId, out int playerId)
    {
        profileId = 0;
        playerId = 1;
        return false;
    }

    public PlayerInputType InputType(int player)
    {
        return PlayerInputType.KeyboardMouse;
    }

    public IReadOnlyList<int> GetAvailableProfiles()
    {
        return _availableProfiles;
    }
}
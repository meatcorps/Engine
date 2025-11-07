using System.Numerics;
using Meatcorps.Engine.Core.Enums;

namespace Meatcorps.Engine.Core.Interfaces.Input;

public interface IInputMapper<in T> where T : Enum
{
    IInput GetState(int player, T input);
    Vector2 GetAxis(int player, int axis = 1);
    void Rumble(int player, float left, float right, float duration);
    void AssignProfile(int profileId, int player);
    void UnassignProfile(int player);
    bool IsAssigned(int player);
    bool IsConnected(int player);
    bool AnyInputPressed(out int profileId, out int playerId);
    PlayerInputType InputType(int player);
    IReadOnlyList<int> GetAvailableProfiles();
}
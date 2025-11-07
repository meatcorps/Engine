using System.Numerics;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.Interfaces.Input;

namespace Meatcorps.Engine.Hardware.ArduinoController.ArduinoController;

public class ArduinoInputMapper<T> : IInputMapperWithManager<T, ArduinoControllerManager> where T : Enum
{
    public ArduinoControllerManager Manager { get; }
    private Dictionary<T, ControllerInputEnum> Map { get; } = new();
    
    private List<int> _availableProfiles = new([0, 1]);
    
    public ArduinoInputMapper(ArduinoControllerManager manager)
    {
        Manager = manager;
    }

    public IInput GetState(int player, T input)
    {
        return Manager.GetState(player, Map[input]);
    }

    public Vector2 GetAxis(int player, int axis = 1)
    {
        if (axis != 1)
            throw new NotImplementedException();
        
        return Manager.GetAxis(player);
    }

    public void Rumble(int player, float left, float right, float duration)
    {
        // No rumble support
    }

    public void AssignProfile(int profileId, int player)
    {
        // Not supported
    }

    public void UnassignProfile(int player)
    {
        // Not supported
    }

    public bool IsAssigned(int player)
    {
        return player > 0 & player < 3;
    }

    public bool IsConnected(int _)
    {
        return Manager.IsConnected;
    }

    public bool AnyInputPressed(out int profileId, out int playerId)
    {
        foreach (var controllerInput in Map.Values)
        {
            for (var i = 1; i <= 2; i++)
            {
                if (Manager.GetState(i, controllerInput).IsPressed)
                {
                    profileId = i - 1;
                    playerId = i;
                    return true;
                }
            }
        }

        playerId = -1;
        profileId = -1;
        return false;
    }

    public PlayerInputType InputType(int _) => PlayerInputType.Arcade;
    
    public IReadOnlyList<int> GetAvailableProfiles()
    {
        return _availableProfiles;
    }

    public ControllerInputEnum MapInput(T input)
    {
        return Map[input];
    }
    
    public void AddMapping(T input, ControllerInputEnum controllerInput)
    {
        Map.Add(input, controllerInput);
    }
}
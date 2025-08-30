using System.Numerics;
using Meatcorps.Engine.Core.Interfaces.Input;

namespace Meatcorps.Engine.Hardware.ArduinoController.ArduinoController;

public class ArduinoInputMapper<T> : IInputMapperWithManager<T, ArduinoControllerManager> where T : Enum
{
    public ArduinoControllerManager Manager { get; }
    private Dictionary<T, ControllerInputEnum> Map { get; } = new();
    
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

    public ControllerInputEnum MapInput(T input)
    {
        return Map[input];
    }
    
    public void AddMapping(T input, ControllerInputEnum controllerInput)
    {
        Map.Add(input, controllerInput);
    }
}
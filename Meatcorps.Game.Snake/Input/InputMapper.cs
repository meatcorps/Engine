using Meatcorps.Engine.Core.Interfaces.Input;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Hardware.ArduinoController.ArduinoController;
using Meatcorps.Game.Snake.Resources;

namespace Meatcorps.Game.Snake.Input;

public static class InputMapper
{
    public static IInputMapper<SnakeInput> ArduinoInput()
    {
        var mapper = new ArduinoInputMapper<SnakeInput>(GlobalObjectManager.ObjectManager.Get<ArduinoControllerManager>()!);

        // map logical → hardware
        mapper.AddMapping(SnakeInput.Up,    ControllerInputEnum.Up);
        mapper.AddMapping(SnakeInput.Down,  ControllerInputEnum.Down);
        mapper.AddMapping(SnakeInput.Left,  ControllerInputEnum.Left);
        mapper.AddMapping(SnakeInput.Right, ControllerInputEnum.Right);
        mapper.AddMapping(SnakeInput.Start, ControllerInputEnum.Button1);
        mapper.AddMapping(SnakeInput.Action,ControllerInputEnum.Button1);
        
        GlobalObjectManager.ObjectManager.Register<IInputMapper<SnakeInput>>(mapper);
        GlobalObjectManager.ObjectManager.Register<IInputMapperWithManager<SnakeInput, ArduinoControllerManager>>(mapper);
        
        return mapper;
    }
}
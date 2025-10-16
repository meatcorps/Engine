using Meatcorps.Engine.Core.Interfaces.Input;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Hardware.ArduinoController.ArduinoController;
using Meatcorps.Game.KillTheSkulls.GameEnums;
using Meatcorps.Game.KillTheSkulls.Resources;

namespace Meatcorps.Game.KillTheSkulls.Input;

public static class InputMapper
{
    public static IInputMapper<GameInput> ArduinoInput()
    {
        var mapper =
            new ArduinoInputMapper<GameInput>(GlobalObjectManager.ObjectManager.Get<ArduinoControllerManager>()!);

        // map logical → hardware
        mapper.AddMapping(GameInput.Smash1White, ControllerInputEnum.Button1);
        mapper.AddMapping(GameInput.Smash5Red, ControllerInputEnum.Up);
        mapper.AddMapping(GameInput.Smash2Blue, ControllerInputEnum.Down);
        mapper.AddMapping(GameInput.Smash3Yellow, ControllerInputEnum.Left);
        mapper.AddMapping(GameInput.Smash4Green, ControllerInputEnum.Right);
        mapper.AddMapping(GameInput.Action, ControllerInputEnum.Button1);

        GlobalObjectManager.ObjectManager.Register<IInputMapper<GameInput>>(mapper);
        GlobalObjectManager.ObjectManager
            .Register<IInputMapperWithManager<GameInput, ArduinoControllerManager>>(mapper);

        return mapper;
    }
}
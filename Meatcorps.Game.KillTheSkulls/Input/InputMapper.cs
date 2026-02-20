using Meatcorps.Engine.Core.Interfaces.Input;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Hardware.ArduinoController.ArduinoController;
using Meatcorps.Game.KillTheSkulls.GameEnums;

namespace Meatcorps.Game.KillTheSkulls.Input;

public static class InputMapper
{
    public static IInputMapper<GameInput> ArduinoInput()
    {
        var mapper =
            new ArduinoInputMapper<GameInput>(GlobalObjectManager.ObjectManager.Get<ArduinoControllerManager>()!);

        // map logical → hardware
        mapper.AddMapping(GameInput.Smash1White, ControllerInputEnum.Button1);
        mapper.AddMapping(GameInput.Smash2Blue, ControllerInputEnum.Button2);
        mapper.AddMapping(GameInput.Smash3Yellow, ControllerInputEnum.Button3);
        mapper.AddMapping(GameInput.Smash4Green, ControllerInputEnum.Button4);
        mapper.AddMapping(GameInput.Smash5Red, ControllerInputEnum.Button5);
        mapper.AddMapping(GameInput.Action, ControllerInputEnum.Button1);

        GlobalObjectManager.ObjectManager.Register<IInputMapper<GameInput>>(mapper);
        GlobalObjectManager.ObjectManager
            .Register<IInputMapperWithManager<GameInput, ArduinoControllerManager>>(mapper);

        return mapper;
    }
}
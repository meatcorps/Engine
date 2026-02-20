using Meatcorps.Engine.Core.Interfaces.Input;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Hardware.ArduinoController.ArduinoController;
using Meatcorps.Game.HorrorJackpot.GameEnums;

namespace Meatcorps.Game.HorrorJackpot.Input;

public static class InputMapper
{
    public static IInputMapper<GameInput> ArduinoInput()
    {
        var mapper =
            new ArduinoInputMapper<GameInput>(GlobalObjectManager.ObjectManager.Get<ArduinoControllerManager>()!);

        // map logical → hardware
        mapper.AddMapping(GameInput.Action, ControllerInputEnum.Button1);

        GlobalObjectManager.ObjectManager.Register<IInputMapper<GameInput>>(mapper);
        GlobalObjectManager.ObjectManager
            .Register<IInputMapperWithManager<GameInput, ArduinoControllerManager>>(mapper);

        return mapper;
    }
}
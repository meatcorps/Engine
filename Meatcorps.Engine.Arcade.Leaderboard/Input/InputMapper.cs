using Meatcorps.Engine.Core.Interfaces.Input;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Hardware.ArduinoController.ArduinoController;
using Meatcorps.Engine.Arcade.Leaderboard.GameEnums;
using Meatcorps.Engine.Arcade.Leaderboard.Resources;

namespace Meatcorps.Engine.Arcade.Leaderboard.Input;

public static class InputMapper
{
    public static IInputMapper<GameInput> ArduinoInput()
    {
        var mapper =
            new ArduinoInputMapper<GameInput>(GlobalObjectManager.ObjectManager.Get<ArduinoControllerManager>()!);

        // map logical → hardware
        mapper.AddMapping(GameInput.Up, ControllerInputEnum.Up);
        mapper.AddMapping(GameInput.Down, ControllerInputEnum.Down);
        mapper.AddMapping(GameInput.Left, ControllerInputEnum.Left);
        mapper.AddMapping(GameInput.Right, ControllerInputEnum.Right);
        mapper.AddMapping(GameInput.Start, ControllerInputEnum.Button1);
        mapper.AddMapping(GameInput.Action, ControllerInputEnum.Button1);

        GlobalObjectManager.ObjectManager.Register<IInputMapper<GameInput>>(mapper);
        GlobalObjectManager.ObjectManager
            .Register<IInputMapperWithManager<GameInput, ArduinoControllerManager>>(mapper);

        return mapper;
    }
}
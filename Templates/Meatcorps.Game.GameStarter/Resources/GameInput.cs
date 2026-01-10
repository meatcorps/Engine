using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Core.Modules;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Hardware.Controllers.Enums;
using Meatcorps.Engine.Hardware.Controllers.Mapper;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Engine.RayLib.Input;
using Meatcorps.Game.GameStarter.GameEnums;
using Raylib_cs;

namespace Meatcorps.Game.GameStarter.Resources;

public static class GameInput
{
    public static void Load()
    {
          InputModule<GameEnums.GameInput>.Create()
            .AddInputMapper(new GenericMapper<GameEnums.GameInput>(GlobalObjectManager.ObjectManager.Register(new RaylibKeyboardBinder<GameEnums.GameInput>()))
                .AddInputKeyboard(0, GameEnums.GameInput.Up, KeyboardKey.Up)
                .AddInputKeyboard(0, GameEnums.GameInput.Down, KeyboardKey.Down)
                .AddInputKeyboard(0, GameEnums.GameInput.Left, KeyboardKey.Left)
                .AddInputKeyboard(0, GameEnums.GameInput.Right, KeyboardKey.Right)
                .AddInputKeyboard(0, GameEnums.GameInput.Start, KeyboardKey.Enter)
                .AddInputKeyboard(0, GameEnums.GameInput.Action, KeyboardKey.Enter)
                .AddAxis(0, 1, GameEnums.GameInput.Left, GameEnums.GameInput.Right, GameEnums.GameInput.Up, GameEnums.GameInput.Down)
                .AddInputKeyboard(1, GameEnums.GameInput.Up, KeyboardKey.W)
                .AddInputKeyboard(1, GameEnums.GameInput.Down, KeyboardKey.S)
                .AddInputKeyboard(1, GameEnums.GameInput.Left, KeyboardKey.A)
                .AddInputKeyboard(1, GameEnums.GameInput.Right, KeyboardKey.D)
                .AddInputKeyboard(1, GameEnums.GameInput.Start, KeyboardKey.F)
                .AddInputKeyboard(1, GameEnums.GameInput.Action, KeyboardKey.F)
                .AddAxis(1, 1, GameEnums.GameInput.Left, GameEnums.GameInput.Right, GameEnums.GameInput.Up, GameEnums.GameInput.Down))
            .AddInputMapper(new ControllerInputMapper<GameEnums.GameInput>(new SDLControllerDeviceManager())
                .Map(GameEnums.GameInput.Start, ControllerInputEnum.AorCross)
                .Map(GameEnums.GameInput.Action, ControllerInputEnum.Start)
                .Map(GameEnums.GameInput.Down, ControllerInputEnum.DPadDown)
                .Map(GameEnums.GameInput.Left, ControllerInputEnum.DPadLeft)
                .Map(GameEnums.GameInput.Up, ControllerInputEnum.DPadUp)
                .Map(GameEnums.GameInput.Right, ControllerInputEnum.DPadRight)
                .SetDPadIsAxis(true))
            .WithAutoAssign()
            .Setup();
    }
}
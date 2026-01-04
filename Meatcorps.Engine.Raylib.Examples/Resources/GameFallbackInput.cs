using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Core.Modules;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Hardware.Controllers.Enums;
using Meatcorps.Engine.Hardware.Controllers.Mapper;
using Meatcorps.Engine.Raylib.Examples.Enums;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Engine.RayLib.Input;
using Raylib_cs;

namespace Meatcorps.Engine.Raylib.Examples.Resources;

public static class GameFallbackInput
{
    public static void Load()
    {
        InputModule<GameInput>.Create()
            .AddInputMapper(new GenericMapper<GameInput>(GlobalObjectManager.ObjectManager.Register(new RaylibKeyboardBinder<GameInput>()))
                .AddInputKeyboard(0, GameInput.Up, KeyboardKey.Up)
                .AddInputKeyboard(0, GameInput.Down, KeyboardKey.Down)
                .AddInputKeyboard(0, GameInput.Left, KeyboardKey.Left)
                .AddInputKeyboard(0, GameInput.Right, KeyboardKey.Right)
                .AddInputKeyboard(0, GameInput.Start, KeyboardKey.Enter)
                .AddInputKeyboard(0, GameInput.Action, KeyboardKey.Enter)
                .AddInputKeyboard(1, GameInput.Up, KeyboardKey.W)
                .AddInputKeyboard(1, GameInput.Down, KeyboardKey.S)
                .AddInputKeyboard(1, GameInput.Left, KeyboardKey.A)
                .AddInputKeyboard(1, GameInput.Right, KeyboardKey.D)
                .AddInputKeyboard(1, GameInput.Start, KeyboardKey.F)
                .AddInputKeyboard(1, GameInput.Action, KeyboardKey.F))
            .AddInputMapper(new ControllerInputMapper<GameInput>(new SDLControllerDeviceManager())
                .Map(GameInput.Start, ControllerInputEnum.AorCross)
                .Map(GameInput.Action, ControllerInputEnum.XorSquare)
                .Map(GameInput.Back, ControllerInputEnum.BorCircle)
                .Map(GameInput.Down, ControllerInputEnum.DPadDown)
                .Map(GameInput.Left, ControllerInputEnum.DPadLeft)
                .Map(GameInput.Up, ControllerInputEnum.DPadUp)
                .Map(GameInput.Right, ControllerInputEnum.DPadRight)
                .SetDPadIsAxis(true)
            )
            .WithAutoAssign()
            .Setup();
    }
}
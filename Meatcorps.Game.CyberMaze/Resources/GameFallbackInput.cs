using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Core.Modules;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Hardware.Controllers.Enums;
using Meatcorps.Engine.Hardware.Controllers.Mapper;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Engine.RayLib.Input;
using Meatcorps.Engine.SDL.Controller;
using Meatcorps.Game.CyberMaze.GameEnums;
using Raylib_cs;

namespace Meatcorps.Game.CyberMaze.Resources;

public static class GameFallbackInput
{
    public static void Load()
    {
        /*var mapper = new GenericMapper<GameInput>()
            .AddInputKeyboard(0, GameInput.Up, KeyboardKey.Up)
            .AddInputKeyboard(0, GameInput.Down, KeyboardKey.Down)
            .AddInputKeyboard(0, GameInput.Left, KeyboardKey.Left)
            .AddInputKeyboard(0, GameInput.Right, KeyboardKey.Right)
            .AddInputKeyboard(0, GameInput.Start, KeyboardKey.Enter)
            .AddInputKeyboard(0, GameInput.Action, KeyboardKey.Enter)
            .AddAxis(0, 1, GameInput.Left, GameInput.Right, GameInput.Up, GameInput.Down)
            .AddInputKeyboard(1, GameInput.Up, KeyboardKey.W)
            .AddInputKeyboard(1, GameInput.Down, KeyboardKey.S)
            .AddInputKeyboard(1, GameInput.Left, KeyboardKey.A)
            .AddInputKeyboard(1, GameInput.Right, KeyboardKey.D)
            .AddInputKeyboard(1, GameInput.Start, KeyboardKey.F)
            .AddInputKeyboard(1, GameInput.Action, KeyboardKey.F)
            .AddAxis(1, 1, GameInput.Left, GameInput.Right, GameInput.Up, GameInput.Down);
            
           InputModule<GameInput>.CreateOnlyKeyboardMouseMapper(mapper, 2);*/
        
        InputModule<GameInput>.Create()
            .AddInputMapper(new GenericMapper<GameInput>(GlobalObjectManager.ObjectManager.Register(new RaylibKeyboardBinder<GameInput>()))
                .AddInputKeyboard(0, GameInput.Up, KeyboardKey.Up)
                .AddInputKeyboard(0, GameInput.Down, KeyboardKey.Down)
                .AddInputKeyboard(0, GameInput.Left, KeyboardKey.Left)
                .AddInputKeyboard(0, GameInput.Right, KeyboardKey.Right)
                .AddInputKeyboard(0, GameInput.Start, KeyboardKey.Enter)
                .AddInputKeyboard(0, GameInput.Action, KeyboardKey.Enter)
                .AddAxis(0, 1, GameInput.Left, GameInput.Right, GameInput.Up, GameInput.Down)
                .AddInputKeyboard(1, GameInput.Up, KeyboardKey.W)
                .AddInputKeyboard(1, GameInput.Down, KeyboardKey.S)
                .AddInputKeyboard(1, GameInput.Left, KeyboardKey.A)
                .AddInputKeyboard(1, GameInput.Right, KeyboardKey.D)
                .AddInputKeyboard(1, GameInput.Start, KeyboardKey.F)
                .AddInputKeyboard(1, GameInput.Action, KeyboardKey.F)
                .AddAxis(1, 1, GameInput.Left, GameInput.Right, GameInput.Up, GameInput.Down))
            .AddInputMapper(new ControllerInputMapper<GameInput>(new SDLControllerDeviceManager())
                .Map(GameInput.Start, ControllerInputEnum.AorCross)
                .Map(GameInput.Action, ControllerInputEnum.Start)
                .Map(GameInput.Down, ControllerInputEnum.DPadDown)
                .Map(GameInput.Left, ControllerInputEnum.DPadLeft)
                .Map(GameInput.Up, ControllerInputEnum.DPadUp)
                .Map(GameInput.Right, ControllerInputEnum.DPadRight)
                .SetDPadIsAxis(true))
            .WithAutoAssign()
            .Setup();

        /*var device = new SDLControllerDeviceManager();
        var mapper = new ControllerInputMapper<GameInput>(device);

        mapper.Map(GameInput.Start, ControllerInputEnum.BorCircle);
        mapper.Map(GameInput.Action, ControllerInputEnum.AorCross);
        mapper.Map(GameInput.Down, ControllerInputEnum.DPadDown);
        mapper.Map(GameInput.Left, ControllerInputEnum.DPadLeft);
        mapper.Map(GameInput.Up, ControllerInputEnum.DPadUp);
        mapper.Map(GameInput.Right, ControllerInputEnum.DPadRight);
        mapper.SetDPadIsAxis(true);
        device.AssignDevice(1, 0);
        device.AssignDevice(2, 1);
        GlobalObjectManager.ObjectManager.RegisterOnce(new PlayerInputRouter<GameInput>());
        var router = GlobalObjectManager.ObjectManager.Get<PlayerInputRouter<GameInput>>()!;

        router.AssignMapper(1, mapper);
        router.AssignMapper(2, mapper);

        GlobalObjectManager.ObjectManager.Add<IBackgroundService>(mapper);*/
    }
}
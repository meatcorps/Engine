using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Core.Modules;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Game.CyberMaze.GameEnums;
using Raylib_cs;

namespace Meatcorps.Game.CyberMaze.Resources;

public static class GameFallbackInput
{
    public static void Load()
    {
        var mapper = new GenericMapper<GameInput>()
            .AddInputKeyboard(1, GameInput.Up, KeyboardKey.Up)
            .AddInputKeyboard(1, GameInput.Down, KeyboardKey.Down)
            .AddInputKeyboard(1, GameInput.Left, KeyboardKey.Left)
            .AddInputKeyboard(1, GameInput.Right, KeyboardKey.Right)
            .AddInputKeyboard(1, GameInput.Start, KeyboardKey.Enter)
            .AddInputKeyboard(1, GameInput.Action, KeyboardKey.Enter)
            .AddAxis(1, 1, GameInput.Left, GameInput.Right, GameInput.Up, GameInput.Down)
            .AddInputKeyboard(2, GameInput.Up, KeyboardKey.W)
            .AddInputKeyboard(2, GameInput.Down, KeyboardKey.S)
            .AddInputKeyboard(2, GameInput.Left, KeyboardKey.A)
            .AddInputKeyboard(2, GameInput.Right, KeyboardKey.D)
            .AddInputKeyboard(2, GameInput.Start, KeyboardKey.F)
            .AddInputKeyboard(2, GameInput.Action, KeyboardKey.F)
            .AddAxis(2, 1, GameInput.Left, GameInput.Right, GameInput.Up, GameInput.Down);
        
        GenericInputModule.Create(mapper, 2);
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
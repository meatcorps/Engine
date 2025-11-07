using System.Numerics;
using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Core.Interfaces.Input;
using Meatcorps.Engine.Core.Modules;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Hardware.Controllers.Enums;
using Meatcorps.Engine.Hardware.Controllers.Mapper;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.Raylib.Examples.Enums;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Engine.RayLib.Input;
using Meatcorps.Engine.RayLib.Resources;
using Raylib_cs;

namespace Meatcorps.Engine.Raylib.Examples.GameObjects;

public class InputSelectionTest: BaseGameObject
{
    private IInputMapper<GameInput> _inputRouter;
    private InputManager<GameInput> _inputManager;
    private int _totalPlayers = 3;
    private TextManager<DefaultFont> _fonts = null!;

    protected override void OnInitialize()
    {
        Camera = CameraLayer.UI;
        
        InputModule<GameInput>.Create(Scene.SceneObjectManager)
            .AddInputMapper(new GenericMapper<GameInput>()
                .AddInputKeyboard(0, GameInput.Up, KeyboardKey.Up)
                .AddInputKeyboard(0, GameInput.Down, KeyboardKey.Down)
                .AddInputKeyboard(0, GameInput.Left, KeyboardKey.Left)
                .AddInputKeyboard(0, GameInput.Right, KeyboardKey.Right)
                .AddInputKeyboard(0, GameInput.Action, KeyboardKey.Enter)
                .AddInputKeyboard(0, GameInput.Back, KeyboardKey.Backspace)
                .AddAxis(0, 1, GameInput.Left, GameInput.Right, GameInput.Up, GameInput.Down)
                .AddInputKeyboard(1, GameInput.Up, KeyboardKey.W)
                .AddInputKeyboard(1, GameInput.Down, KeyboardKey.S)
                .AddInputKeyboard(1, GameInput.Left, KeyboardKey.A)
                .AddInputKeyboard(1, GameInput.Right, KeyboardKey.D)
                .AddInputKeyboard(1, GameInput.Action, KeyboardKey.F)
                .AddInputKeyboard(1, GameInput.Back, KeyboardKey.Q)
                .AddAxis(1, 1, GameInput.Left, GameInput.Right, GameInput.Up, GameInput.Down))
            .AddInputMapper( new ControllerInputMapper<GameInput>(new SDLControllerDeviceManager())
                .Map(GameInput.Action, ControllerInputEnum.AorCross)
                .Map(GameInput.Back, ControllerInputEnum.BorCircle))
            //.WithAutoAssign()
            .Setup();
        _fonts = GlobalObjectManager.ObjectManager.Get<TextManager<DefaultFont>>()!;

        _inputRouter = Scene.SceneObjectManager.Get<IInputMapper<GameInput>>()!;
        _inputManager = Scene.SceneObjectManager.Get<InputManager<GameInput>>()!;
        _inputManager.AssignPlayers(_totalPlayers);
    }

    protected override void OnUpdate(float deltaTime)
    {
    }

    protected override void OnDraw()
    {
        base.OnDraw();

        var counter = 16;

        foreach (var status in _inputManager.CurrentInputStatus)
        {
            Raylib_cs.Raylib.DrawTextEx(_fonts.GetFont(), $"{status.Id}: {status.Type} Online: {status.Online} Ready: {status.Assigned} | ROUTER ASSIGNED: {_inputRouter.IsAssigned(status.Id)}", new Vector2(16, counter), 12, 1, Color.White);
            counter += 16;
        }
        counter += 16;
        Raylib_cs.Raylib.DrawTextEx(_fonts.GetFont(), $"READY: {_inputManager.Ready}", new Vector2(16, counter), 12, 1, Color.White);
        counter += 16;
        if (_inputManager.Ready)
        {
            for (var i = 1; i <= _totalPlayers; i++)
            {
                var action = _inputRouter.GetState(i, GameInput.Action);
                var back = _inputRouter.GetState(i, GameInput.Back);
                var axis = _inputRouter.GetAxis(i, 1);
                Raylib_cs.Raylib.DrawTextEx(_fonts.GetFont(), $"{i}: Axis: {axis} Action({action.Label}): {action.Down} Back({back.Label}): {back.Down}", new Vector2(16, counter), 12, 1, Color.White);
                counter += 16;
            }
        }

    }

    protected override void OnDispose()
    {
    }
}
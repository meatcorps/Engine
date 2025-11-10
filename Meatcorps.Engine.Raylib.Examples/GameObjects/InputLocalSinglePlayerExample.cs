using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Core.Interfaces.Input;
using Meatcorps.Engine.Core.Modules;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Tween;
using Meatcorps.Engine.Hardware.Controllers.Enums;
using Meatcorps.Engine.Hardware.Controllers.Mapper;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.Raylib.Examples.Enums;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Engine.RayLib.GameObjects.UI;
using Meatcorps.Engine.RayLib.Input;
using Meatcorps.Engine.RayLib.Resources;
using Raylib_cs;

namespace Meatcorps.Engine.Raylib.Examples.GameObjects;

// Change the MainScene.cs load this example

public class InputLocalSinglePlayerExample: BaseGameObject
{
    private IInputMapper<GameInput> _inputRouter;
    private InputManager<GameInput> _inputManager;
    private int _totalPlayers = 1;
    private TextManager<DefaultFont> _fonts = null!;
    private List<InputEntity> _inputEntities = new List<InputEntity>();

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
            .WithAutoAssign()
            .Setup();
        
        _fonts = GlobalObjectManager.ObjectManager.Get<TextManager<DefaultFont>>()!;

        _inputRouter = Scene.SceneObjectManager.Get<IInputMapper<GameInput>>()!;
        _inputManager = Scene.SceneObjectManager.Get<InputManager<GameInput>>()!;
        
        _inputEntities.Add(new InputEntity
        {
            Color = Color.Green,
            Position = new Vector2(34, 0) * _inputEntities.Count + new Vector2(250, 160),
        });
        _inputEntities.Add(new InputEntity
        {
            Color = Color.Red,
            Position = new Vector2(34, 0) * _inputEntities.Count + new Vector2(250, 160),
        });
        _inputEntities.Add(new InputEntity
        {
            Color = Color.Blue,
            Position = new Vector2(34, 0) * _inputEntities.Count + new Vector2(250, 160),
        });
    }

    protected override void OnUpdate(float deltaTime)
    {
        if (_inputManager.Ready)
        {
            for (var i = 1; i <= _totalPlayers; i++)
            {
                var axis = _inputRouter.GetAxis(i, 1);
                var target = _inputEntities[i - 1];
                if (!axis.IsEqualsSafe(Vector2.Zero))
                    target.Velocity = (target.Velocity + axis * 10).Clamp(new Vector2(-250, -250), new Vector2(250, 250));
                else 
                    target.Velocity *= 0.9f;
                
                target.Position += target.Velocity * deltaTime;
                target.Position = target.Position.Warp(new Rect(0, 0, 640, 360));
            }
        }
    }

    protected override void OnDraw()
    {
        base.OnDraw();

        var counter = 16;
        Raylib_cs.Raylib.DrawTextEx(_fonts.GetFont(), $"READY: {_inputManager.Ready}", new Vector2(16, counter), 12, 1, Color.DarkGray);
        counter += 16;
        if (_inputManager.Ready)
        {
            for (var i = 1; i <= _totalPlayers; i++)
            {
                var action = _inputRouter.GetState(i, GameInput.Action);
                var back = _inputRouter.GetState(i, GameInput.Back);
                var axis = _inputRouter.GetAxis(i, 1);
                var target = _inputEntities[i - 1];
                Raylib_cs.Raylib.DrawTextEx(_fonts.GetFont(), $"{i}: Axis: {axis}\n  Action({action.Label}): {action.Down} Back({back.Label}): {back.Down}", new Vector2(16, counter), 8, 1, Raylib_cs.Raylib.ColorAlpha(target.Color, 0.3f));
                counter += 32;
                if (action.Down)
                    Raylib_cs.Raylib.DrawRectangle((int)target.Position.X, (int)target.Position.Y, 32, 32, target.Color);
                else
                    Raylib_cs.Raylib.DrawRectangleLinesEx(new Rectangle(target.Position, new Vector2(32, 32)), 2, target.Color);
                
            }
        }
        else
        {

            foreach (var status in _inputManager.CurrentInputStatus)
            {
                if (!status.Online)
                {
                    
                    var target = _inputEntities[status.Id - 1];
                    Raylib_cs.Raylib.DrawRectangle((int)target.Position.X, (int)target.Position.Y, 32, 32, Color.Red);
                }
            }
            
            foreach (var status in _inputManager.CurrentInputStatus)
            {
                if (!status.Online)
                {
                    Raylib_cs.Raylib.DrawTextEx(_fonts.GetFont(),
                        $"Player {status.Id}, Reconnect controller!",
                        new Vector2(16, counter), 8, 1, Color.Red);
                    counter += 32;
                }
            }
            
            for (var i = 1; i <= _totalPlayers; i++)
            {
                var target = _inputEntities[i - 1];
                Raylib_cs.Raylib.DrawRectangleLinesEx(new Rectangle(target.Position, new Vector2(32, 32)), 2, Color.Gray);
            }
        }

    }

    protected override void OnDispose()
    {
    }
    
    private class InputEntity
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public Color Color { get; set; }
    }
}
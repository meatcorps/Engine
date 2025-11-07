using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Core.Modules;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Game.ArcadeTemplate.GameEnums;
using Raylib_cs;

namespace Meatcorps.Game.ArcadeTemplate.Resources;

public static class GameFallbackInput
{
    public static void Load()
    {
        var mapper = new GenericMapper<GameInput>()
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

        InputModule<GameInput>.CreateOnlyKeyboardMouseMapper(mapper, 2);
    }
}
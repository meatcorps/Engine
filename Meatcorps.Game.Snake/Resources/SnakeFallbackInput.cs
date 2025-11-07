using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Core.Modules;
using Meatcorps.Engine.RayLib.Extensions;
using Raylib_cs;

namespace Meatcorps.Game.Snake.Resources;

public static class SnakeFallbackInput
{
    public static void Load()
    {
        var mapper = new GenericMapper<SnakeInput>()
            .AddInputKeyboard(0, SnakeInput.Up, KeyboardKey.Up)
            .AddInputKeyboard(0, SnakeInput.Down, KeyboardKey.Down)
            .AddInputKeyboard(0, SnakeInput.Left, KeyboardKey.Left)
            .AddInputKeyboard(0, SnakeInput.Right, KeyboardKey.Right)
            .AddInputKeyboard(0, SnakeInput.Start, KeyboardKey.Enter)
            .AddInputKeyboard(0, SnakeInput.Action, KeyboardKey.Enter)
            .AddAxis(0, 1, SnakeInput.Left, SnakeInput.Right, SnakeInput.Up, SnakeInput.Down)
            .AddInputKeyboard(1, SnakeInput.Up, KeyboardKey.W)
            .AddInputKeyboard(1, SnakeInput.Down, KeyboardKey.S)
            .AddInputKeyboard(1, SnakeInput.Left, KeyboardKey.A)
            .AddInputKeyboard(1, SnakeInput.Right, KeyboardKey.D)
            .AddInputKeyboard(1, SnakeInput.Start, KeyboardKey.F)
            .AddInputKeyboard(1, SnakeInput.Action, KeyboardKey.F)
            .AddAxis(1, 1, SnakeInput.Left, SnakeInput.Right, SnakeInput.Up, SnakeInput.Down);

        InputModule<SnakeInput>.CreateOnlyKeyboardMouseMapper(mapper, 2);
    }
}
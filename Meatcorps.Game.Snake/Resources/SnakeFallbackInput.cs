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
            .AddInputKeyboard(1, SnakeInput.Up, KeyboardKey.Up)
            .AddInputKeyboard(1, SnakeInput.Down, KeyboardKey.Down)
            .AddInputKeyboard(1, SnakeInput.Left, KeyboardKey.Left)
            .AddInputKeyboard(1, SnakeInput.Right, KeyboardKey.Right)
            .AddInputKeyboard(1, SnakeInput.Start, KeyboardKey.Enter)
            .AddInputKeyboard(1, SnakeInput.Action, KeyboardKey.Enter)
            .AddAxis(1, 1, SnakeInput.Left, SnakeInput.Right, SnakeInput.Up, SnakeInput.Down)
            .AddInputKeyboard(2, SnakeInput.Up, KeyboardKey.W)
            .AddInputKeyboard(2, SnakeInput.Down, KeyboardKey.S)
            .AddInputKeyboard(2, SnakeInput.Left, KeyboardKey.A)
            .AddInputKeyboard(2, SnakeInput.Right, KeyboardKey.D)
            .AddInputKeyboard(2, SnakeInput.Start, KeyboardKey.F)
            .AddInputKeyboard(2, SnakeInput.Action, KeyboardKey.F)
            .AddAxis(2, 1, SnakeInput.Left, SnakeInput.Right, SnakeInput.Up, SnakeInput.Down);

        GenericInputModule.Create(mapper, 2);
    }
}
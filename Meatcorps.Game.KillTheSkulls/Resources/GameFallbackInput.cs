using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Core.Modules;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Game.KillTheSkulls.GameEnums;
using Raylib_cs;

namespace Meatcorps.Game.KillTheSkulls.Resources;

public static class GameFallbackInput
{
    public static void Load()
    {
        var mapper = new GenericMapper<GameInput>()
            .AddInputKeyboard(0, GameInput.Smash1White, KeyboardKey.One)
            .AddInputKeyboard(0, GameInput.Smash2Blue, KeyboardKey.Two)
            .AddInputKeyboard(0, GameInput.Smash3Yellow, KeyboardKey.Three)
            .AddInputKeyboard(0, GameInput.Smash4Green, KeyboardKey.Four)
            .AddInputKeyboard(0, GameInput.Smash5Red, KeyboardKey.Five)
            .AddInputKeyboard(0, GameInput.Action, KeyboardKey.One);

        InputModule<GameInput>.CreateOnlyKeyboardMouseMapper(mapper, 1);
    }
}
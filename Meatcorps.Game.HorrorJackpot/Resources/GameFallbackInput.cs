using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Core.Modules;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Game.HorrorJackpot.GameEnums;
using Raylib_cs;

namespace Meatcorps.Game.HorrorJackpot.Resources;

public static class GameFallbackInput
{
    public static void Load()
    {
        var mapper = new GenericMapper<GameInput>()
            .AddInputKeyboard(0, GameInput.Action, KeyboardKey.Enter);

        InputModule<GameInput>.CreateOnlyKeyboardMouseMapper(mapper, 1);
    }
}
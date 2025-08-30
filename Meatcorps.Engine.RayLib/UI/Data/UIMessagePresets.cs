using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.GameObjects.UI;
using Meatcorps.Engine.RayLib.Text;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.UI.Data;

public static class UIMessagePresets
{
    public static UIMessageStyle Default(Font font) => new()
    {
        Style = TextKitStyles.HudDefault(font) with { Color = Color.White },
        AppearDurationInMilliseconds = 180,
        DisappearDurationInMilliseconds = 180,
        ColorFrom = Color.White,
        ColorTo = Color.White,
        ColorAfter = Color.White,
    };

    public static UIMessageStyle Warning(Font font) => new()
    {
        Style = TextKitStyles.HudDefault(font) with { Color = Color.Orange },
        AppearDurationInMilliseconds = 200,
        DisappearDurationInMilliseconds = 220,
    };

    public static UIMessageStyle Countdown(Font font) => new()
    {
        Style = TextKitStyles.BigCenter(font) with { Color = Color.White },
        AppearDurationInMilliseconds = 120,
        DisappearDurationInMilliseconds = 120,
        ColorFrom = Raylib.ColorAlpha(Color.White, 0),
        ColorAfter = Raylib.ColorAlpha(Color.White, 0),
        SizeFrom = 32,
        SizeTo = 32,
        SizeAfter = 32,
        AnchorFrom = Anchor.Center,
        AnchorTo = Anchor.Center,
        AnchorAfter = Anchor.Center,
    };
}
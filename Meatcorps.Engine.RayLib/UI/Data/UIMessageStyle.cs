using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Text;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.UI.Data;

public sealed class UIMessageStyle
{
    public Anchor AnchorAfter = Anchor.Bottom;
    public Anchor AnchorFrom = Anchor.Bottom;
    public Anchor AnchorTo = Anchor.Bottom;
    public readonly Action AppearAction = () => { };
    public int AppearDurationInMilliseconds = 200;
    public readonly EaseType AppearEasing = EaseType.EaseOutCubic;
    public Color ColorAfter = Color.White;
    public Color ColorFrom = Color.White;
    public Color ColorTo = Color.White;
    public int DisappearDurationInMilliseconds = 200;
    public readonly EaseType DisappearEasing = EaseType.EaseInCubic;
    public int HoldDurationInMilliseconds = 3000;
    public readonly float PaddingTopBottomLeftDown = 20;
    public float SizeAfter = 16;
    public float SizeFrom = 16;
    public float SizeTo = 16;
    public TextStyle Style;
}
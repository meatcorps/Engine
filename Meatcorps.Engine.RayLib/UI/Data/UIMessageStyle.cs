using System.Numerics;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Text;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.UI.Data;

public sealed class UIMessageStyle
{
    public TextStyle Style;
    public int AppearDurationInMilliseconds = 200;
    public int HoldDurationInMilliseconds = 3000;
    public int DisappearDurationInMilliseconds = 200;
    public EaseType AppearEasing = EaseType.EaseOutCubic;
    public EaseType DisappearEasing = EaseType.EaseInCubic;
    public float PaddingTopBottomLeftDown = 20;
    public Anchor AnchorFrom = Anchor.Bottom;
    public Anchor AnchorTo = Anchor.Bottom;
    public Anchor AnchorAfter = Anchor.Bottom;
    public float SizeFrom = 16;
    public float SizeTo = 16;
    public float SizeAfter = 16;
    public Color ColorFrom = Color.White;
    public Color ColorTo = Color.White;
    public Color ColorAfter = Color.White;
    public Action AppearAction = () => { };
}
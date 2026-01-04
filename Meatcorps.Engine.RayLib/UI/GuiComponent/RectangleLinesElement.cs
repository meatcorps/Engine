using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Engine.RayLib.UI.GuiComponent.Core;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.UI.GuiComponent;

public class RectangleLinesElement : BaseGuiItem
{
    private readonly float _radius;
    private readonly int _segments;
    private readonly float _thickness;

    public RectangleLinesElement(Color color, float thickness = 1, int segments = 4, float radius = 0)
    {
        Color = color;
        _thickness = thickness;
        _segments = segments;
        _radius = radius;
    }

    public override bool IsContainer => false;

    protected override void OnInitialize()
    {
        SetRect(new RectF(Vector2.Zero,
            (GuiService.CurrentContainer!.ElementBound + GuiService.CurrentContainer!.Padding).Size));
    }

    public override void UpdateChildren(BaseGuiItem parent)
    {
        ElementBound = parent.ElementBound;
    }

    public override void FinalizeLayout()
    {
        var rect = ElementBound;
        rect.Position += Offset;
        RegisterDraw(() => rect.DrawLines(Color, _thickness, _radius, _segments));
    }
}
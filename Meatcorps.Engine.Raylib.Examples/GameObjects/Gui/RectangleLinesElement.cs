using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Raylib.Examples.GameObjects.Gui.Core;
using Meatcorps.Engine.RayLib.Extensions;
using Raylib_cs;

namespace Meatcorps.Engine.Raylib.Examples.GameObjects.Gui;

public class RectangleLinesElement: BaseGuiItem
{
    private readonly float _thickness;
    private readonly int _segments;
    private readonly float _radius;
    public override bool IsContainer => false;
    protected override void OnInitialize()
    {
        SetRect(new RectF(Vector2.Zero, (GuiService.CurrentContainer!.ElementBound + GuiService.CurrentContainer!.Padding).Size));
    }

    public RectangleLinesElement(Color color, float thickness = 1, int segments = 4, float radius = 0)
    {
        Color = color;
        _thickness = thickness;
        _segments = segments;
        _radius = radius;
    }
    
    public override void UpdateChildren(BaseGuiItem parent)
    {
        ElementBound = parent.ElementBound;
    }
    
    public override void FinalizeLayout()
    {
        RegisterDraw(() => ElementBound.DrawLines(Color, _thickness, _radius, _segments)); 
    }
}
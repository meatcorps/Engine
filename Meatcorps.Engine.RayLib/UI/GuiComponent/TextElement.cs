using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.UI.GuiComponent.Core;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.UI.GuiComponent;

public class TextElement : BaseGuiItem
{
    private readonly Font _font;
    private readonly float _size;
    private readonly float _spacing;
    private readonly string _text;
    private readonly Vector2 _uv;

    public TextElement(Color color, Font font, string text, float size = 16, float spacing = 1, Vector2? uv = null)
    {
        _font = font;
        _text = text;
        _size = size;
        _spacing = spacing;
        _uv = uv ?? UVHelper.Center;
        Color = color;
    }

    public override bool IsContainer => false;

    protected override void OnInitialize()
    {
        SetRect(new RectF(Vector2.Zero,
            (GuiService.CurrentContainer!.ElementBound + GuiService.CurrentContainer!.Padding).Size));
    }

    private Vector2 TextSize()
    {
        return Raylib_cs.Raylib.MeasureTextEx(_font, _text, _size, _spacing);
    }

    public override void UpdateChildren(BaseGuiItem parent)
    {
        ElementBound = parent.ElementBound;
    }

    public override void FinalizeLayout()
    {
        RegisterDraw(() =>
        {
            var textBound = new RectF(Vector2.Zero, TextSize());
            textBound = (ElementBound + Padding).Align(textBound, _uv);
            Raylib_cs.Raylib.DrawTextEx(_font, _text, textBound.Position + Offset, _size, _spacing, Color);
        });
    }
}
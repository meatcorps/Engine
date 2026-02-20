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
    private bool _sizeBasedOnParent = true;

    public TextElement(Color color, Font font, string text, float size = 16, float spacing = 1, Vector2? uv = null)
    {
        _font = font;
        _text = text;
        _size = size;
        _spacing = spacing;
        _uv = uv ?? UVHelper.Center;
        Color = color;
    }

    public TextElement SetSizeBasedOnText(Vector2? position = null)
    {
        _sizeBasedOnParent = false;
        position ??= ElementBound.Position;
        ElementBound = new RectF(position.Value, TextSize());
        return this;
    }
    
    public override bool IsContainer => false;

    protected override void OnInitialize()
    {
        if (_sizeBasedOnParent)
            SetRect(new RectF(Vector2.Zero,
                (GuiService.CurrentContainer!.ElementBound + GuiService.CurrentContainer!.Padding).Size));
    }

    private Vector2 TextSize()
    {
        return Raylib.MeasureTextEx(_font, _text, _size, _spacing);
    }

    public override void UpdateChildren(BaseGuiItem parent)
    {
        if (_sizeBasedOnParent)
            ElementBound = parent.ElementBound;
    }

    public override void FinalizeLayout()
    {
        RegisterDraw(() =>
        {
            RectF textBound;
            if (!_sizeBasedOnParent)
            {
                textBound = ElementBound + Padding;
            }
            else
            {
                textBound = new RectF(Vector2.Zero, TextSize());
                textBound = (ElementBound + Padding).Align(textBound, _uv);
            }

            Raylib.DrawTextEx(_font, _text, textBound.Position + Offset, _size, _spacing, Color);
        });
    }
}
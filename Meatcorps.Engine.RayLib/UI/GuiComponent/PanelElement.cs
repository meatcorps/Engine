using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.UI.GuiComponent.Core;

namespace Meatcorps.Engine.RayLib.UI.GuiComponent;

public class PanelElement : BaseGuiItem
{
    private readonly List<BaseGuiItem> _items = new();
    private readonly bool _resizeElementX;
    private readonly bool _resizeElementY;
    private readonly Vector2 _uv;

    public PanelElement(RectF bounds, Vector2? Uv = null, bool resizeElementX = true, bool resizeElementY = true)
    {
        _resizeElementX = resizeElementX;
        _resizeElementY = resizeElementY;
        ElementBound = bounds;
        _uv = Uv ?? UVHelper.LeftTop;
    }

    public override bool IsContainer => true;
    public bool ResizeElementX { get; }
    public bool ResizeElementY { get; }

    protected override void OnInitialize()
    {
    }

    public override void ChildGuiItemAdded(BaseGuiItem item)
    {
        _items.Add(item);
        var newSize = ElementBound + Padding;

        newSize.Width = !_resizeElementX ? newSize.Width : item.ElementBound.Width;
        newSize.Height = !_resizeElementY ? newSize.Height : item.ElementBound.Height;

        item.ElementBound = (ElementBound + Padding).Align(newSize, _uv);

        base.ChildGuiItemAdded(item);
    }

    public override void UpdateChildren(BaseGuiItem parent)
    {
        foreach (var item in _items) item.UpdateChildren(this);
    }

    protected override void OnContainerStop(GuiService service)
    {
        foreach (var item in _items)
        {
            var innerElementBound = ElementBound + Padding;
            item.ElementBound = innerElementBound.Align(new RectF(Vector2.Zero, item.ElementBound.Size), _uv);
            item.UpdateChildren(this);
        }
    }

    public override void FinalizeLayout()
    {
        //RegisterDraw(() => ElementBound.DrawLines(Color.DarkBrown, 4)); 
    }
}
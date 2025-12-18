using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.UI.GuiComponent.Core;

namespace Meatcorps.Engine.RayLib.UI.GuiComponent;

public class ScrollElement : BaseGuiItem
{
    private readonly List<BaseGuiItem> _items = new();

    public ScrollElement(RectF bounds, Vector2 offset)
    {
        ElementBound = bounds;
        Offset = offset;
    }

    public override bool IsContainer => true;

    protected override void OnInitialize()
    {
    }

    public override void ChildGuiItemAdded(BaseGuiItem item)
    {
        _items.Add(item);
        item.ElementBound = ElementBound;
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
            var rectPosition =
                innerElementBound.Align(new RectF(Vector2.Zero, item.ElementBound.Size), UVHelper.LeftTop);
            rectPosition.Position += Offset;
            item.ElementBound = rectPosition;

            item.UpdateChildren(this);
        }
    }

    public override void FinalizeLayout()
    {
        //RegisterDraw(() => ElementBound.DrawLines(Color.Blue, 4)); 
    }
}
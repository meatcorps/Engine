using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.Raylib.Examples.GameObjects.Gui.Core;
using Meatcorps.Engine.RayLib.Extensions;
using Raylib_cs;

namespace Meatcorps.Engine.Raylib.Examples.GameObjects.Gui;

public class ScrollElement: BaseGuiItem
{
    public override bool IsContainer => true;
    private List<BaseGuiItem> _items = new List<BaseGuiItem>();
    
    protected override void OnInitialize()
    {
    }

    public ScrollElement(RectF bounds, Vector2 offset)
    {
        ElementBound = bounds;
        Offset = offset;
    }

    public override void ChildGuiItemAdded(BaseGuiItem item)
    {
        _items.Add(item);
        item.ElementBound = ElementBound;
        base.ChildGuiItemAdded(item);
    }

    public override void UpdateChildren(BaseGuiItem parent)
    {
        foreach (var item in _items)
        {
            item.UpdateChildren(this);
        }
    }

    protected override void OnContainerStop(GuiService service)
    {
        foreach (var item in _items)
        {
            var innerElementBound = ElementBound + Padding;
            var rectPosition = innerElementBound.Align(new RectF(Vector2.Zero, item.ElementBound.Size), UVHelper.LeftTop);
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
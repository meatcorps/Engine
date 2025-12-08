using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Raylib.Examples.GameObjects.Gui.Core;
using Meatcorps.Engine.RayLib.Extensions;
using Raylib_cs;

namespace Meatcorps.Engine.Raylib.Examples.GameObjects.Gui;

public class PanelElement: BaseGuiItem
{
    public override bool IsContainer => true;
    private List<BaseGuiItem> _items = new List<BaseGuiItem>();
    
    protected override void OnInitialize()
    {
    }

    public PanelElement(RectF bounds)
    {
        ElementBound = bounds;
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
            item.ElementBound = new RectF(ElementBound.Position, item.ElementBound.Size);
            item.UpdateChildren(this);
        }
    }

    public override void FinalizeLayout()
    {
        // RegisterDraw(() => ElementBound.DrawLines(Color.DarkBrown, 4)); 
    }
}
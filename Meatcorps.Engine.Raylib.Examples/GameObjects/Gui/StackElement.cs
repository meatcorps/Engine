using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.Raylib.Examples.GameObjects.Gui.Core;
using Meatcorps.Engine.RayLib.Extensions;
using Raylib_cs;

namespace Meatcorps.Engine.Raylib.Examples.GameObjects.Gui;

public class StackElement: BaseGuiItem
{
    public override bool IsContainer => true;
    private List<BaseGuiItem> _items = new List<BaseGuiItem>();
    public int Gap { get; set; } = 4;
    public Vector2 Direction { get; set; } = Vector2.UnitY;
    public Vector2 Uv { get; set; } = UVHelper.Top;

    protected override void OnInitialize()
    {
        _items = GuiService.Pool.Rent();
    }

    public StackElement(RectF bounds, int gap = 4, Vector2? direction = null, Vector2? uv = null)
    {
        bounds.Width = 0;
        bounds.Height = 0;
        Gap = gap;
        Direction = direction ?? Vector2.UnitY;
        Uv = uv ?? UVHelper.Top;
        ElementBound = bounds;
    }

    public override void ChildGuiItemAdded(BaseGuiItem item)
    {
        if (_items.Count == 0)
        {
            item.ElementBound = new RectF(ElementBound.Position, item.ElementBound.Size);
            ElementBound = item.ElementBound;
        }
        _items.Add(item);
        UpdateBounds();
        base.ChildGuiItemAdded(item);
    }

    public override void MutateGuiItem(BaseGuiItem item)
    {
        UpdateBounds();
        base.MutateGuiItem(item);
    }

    private void UpdateBounds()
    {
        var first = _items[0];
        ElementBound = first.ElementBound;
        var start = ElementBound.Position;
        for (var i = 1; i < _items.Count; i++)
        {
            var next = _items[i];
            next.ElementBound = first.ElementBound.NextPosition(next.ElementBound, Direction, Gap, Uv);
            ElementBound = ElementBound.Union(next.ElementBound);
            first = next;
        }
        var offset = start - ElementBound.Position;
        foreach (var item in _items) 
            item.ElementBound = new RectF(item.ElementBound.Position + offset, item.ElementBound.Size);

        ElementBound = new RectF(ElementBound.Position + offset, ElementBound.Size);
    }

    protected override void OnContainerStop(GuiService service)
    {
        foreach (var item in _items)
            item.UpdateChildren(this);
        
        base.OnContainerStop(service);
    }

    public override void UpdateChildren(BaseGuiItem parent)
    {
        if (_items.Count == 0)
            return;
        
        var first = _items[0];
        first.ElementBound = new RectF(ElementBound.Position, first.ElementBound.Size);
        
        UpdateBounds();
        
        foreach (var item in _items)
            item.UpdateChildren(this);
    }

    public override void FinalizeLayout()
    {
        // RegisterDraw(() => ElementBound.DrawLines(Color.DarkPurple, 4)); 
    }
}
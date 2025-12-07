using Meatcorps.Engine.Core.Data;

namespace Meatcorps.Engine.Raylib.Examples.GameObjects.Gui.Core;

public abstract class BaseGuiItem
{
    public RectF ContainerBound { get; set; }
    public RectF ElementBound { get; set; }
    public MarginF Margin { get; set; } = new MarginF(0);
    public PaddingF Padding { get; set; } = new PaddingF(0);
    protected GuiService GuiService { get; set; } = null!;

    public abstract bool IsContainer { get; }

    public void Initialize(GuiService service)
    {
        GuiService = service;
        SetRect(ElementBound);
        OnInitialize();
    }

    protected abstract void OnInitialize();

    public void ContainerStart()
    {
        OnContainerStart(GuiService);
    }

    protected virtual void OnContainerStart(GuiService service)
    {
    }


    public virtual void ChildGuiItemAdded(BaseGuiItem item)
    {
    }

    public virtual void MutateGuiItem(BaseGuiItem item)
    {
    }

    public void ContainerStop()
    {
        OnContainerStop(GuiService);
    }

    protected virtual void OnContainerStop(GuiService service)
    {
    }

    public abstract void FinalizeLayout();


    public virtual void FinalizeLayoutContainer()
    {
    }

    protected void RegisterDraw(Action draw)
    {
        GuiService.RegisterDraw(draw);
    }

    public void SetRect(RectF rect)
    {
        if (GuiService.CurrentContainer != null)
        {
            var parentRect = GuiService.CurrentContainer.ElementBound + GuiService.CurrentContainer.Padding + Margin;
            rect.Position += parentRect.Position;
        }
        ElementBound = rect;
    }

    public BaseGuiItem SetMargin(MarginF margin)
    {
        Margin = margin;
        return this;
    }

    public BaseGuiItem SetPadding(PaddingF padding)
    {
        Padding = padding;
        return this;
    }
}
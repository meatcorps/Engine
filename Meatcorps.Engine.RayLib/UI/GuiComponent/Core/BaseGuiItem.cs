using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.UI.GuiComponent.Core;

public abstract class BaseGuiItem
{
    private RectF _elementBound;
    public Color Color;
    public Vector2 Offset = Vector2.Zero;
    public RectF ContainerBound { get; set; }

    public RectF ElementBound
    {
        get => _elementBound;
        set
        {
            _elementBound = value;
            OnElementSizeChanged();
        }
    }

    public MarginF Margin { get; set; } = new(0);
    public PaddingF Padding { get; set; } = new(0);
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

    public virtual void UpdateChildren(BaseGuiItem parent)
    {
    }

    protected virtual void OnElementSizeChanged()
    {
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

    public BaseGuiItem SetColor(Color color)
    {
        Color = color;
        return this;
    }

    public BaseGuiItem SetOffset(Vector2 offset)
    {
        Offset = offset;
        return this;
    }
}
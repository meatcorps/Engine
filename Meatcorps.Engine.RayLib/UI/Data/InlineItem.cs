using System.ComponentModel.DataAnnotations;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.RayLib.Enums;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.UI.Data;

public class InlineItem
{
    public string Identifier { get; init; } = Guid.NewGuid().ToString();
    public VAlign VAlign { get; set; }
    public HAlign HAlign { get; set; }
    public bool Enabled { get; set; } = true;
    public bool Visible { get; set; } = true;
    public bool FillHeight { get; set; } = false;
    public bool FillWidth { get; set; } = false;
    public bool CacheSize { get; set; } = true;
    public bool NewLine { get; init; } = false;
    public object? Data { get; init; }
    
    public Insets Margin { get; set; } = Insets.Zero;
    public required Action<InlineRender, InlineItem> Initialize { get; init; }
    public required Func<InlineRender, InlineItem, PointInt> GetSize { get; init; }
    public Action<InlineRender, InlineItem, float> Update { get; init; } = (i, item, delta) => { };
    public required Action<InlineRender, InlineItem, Rectangle> Draw { get; init; }
    public required Action<InlineRender, InlineItem> Destroy { get; init; }
}


public readonly struct Insets
{
    public readonly int Left, Top, Right, Bottom;
    public Insets(int all) : this(all, all, all, all) {}
    public Insets(int left, int top, int right, int bottom)
    { Left = left; Top = top; Right = right; Bottom = bottom; }
    public static Insets Zero => new(0);
    public int Horizontal => Left + Right;
    public int Vertical => Top + Bottom;
}
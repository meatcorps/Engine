using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.UI.Data;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.UI;

public class InlineRender : IDisposable
{
    public bool AutoWidth { get; set; }
    public bool AutoHeight { get; set; }
    public bool Wrap { get; set; }
    public PointInt Direction { get; set; } = new(1, 1);
    public Orientation NewLineOrientation { get; set; }
    public Rect Bounds { get; set; }
    public VAlign VAlign { get; set; } = VAlign.Middle;
    public HAlign HAlign { get; set; } = HAlign.Left;
    public int ItemSpacing { get; set; } = 0;   // extra space between items on the same line
    public int LineSpacing { get; set; } = 0;   // extra pixels between lines

    private bool _disposed;
    private PointInt _currentPosition;
    private PointInt _currentSize;
    private bool _newLine;

    private readonly List<InlineItem> _items = new();
    private readonly Dictionary<string, PointInt> _cachedSizes = new();
    private readonly Dictionary<string, Rectangle> _drawPositions = new();
    private readonly Dictionary<int, PointInt> _lineSize = new();
    private readonly Dictionary<int, List<InlineItem>> _lineItems = new();
    private int _linePosition;
    private int _lineCount;
    private InlineItem? _currentItem;

    public InlineRender()
    {
        _lineItems[0] = new();
    }

    public InlineRender Register(InlineItem item)
    {
        var index = _items.FindIndex(x => x.Identifier == item.Identifier);
        if (index > -1)
        {
            _items[index].Destroy(this, _items[index]);
            _items.RemoveAt(index);
            _items.Insert(index, item);
        }
        else
            _items.Add(item);

        item.Initialize(this, item);

        if (item.CacheSize)
            ExtractCacheSize(item);

        return this;
    }

    private void ExtractCacheSize(InlineItem item)
    {
        _cachedSizes[item.Identifier] = item.GetSize(this, item);
    }

    public InlineRender Unregister(string identifier)
    {
        var item = _items.FirstOrDefault(x => x.Identifier == identifier);
        if (item == null)
            throw new Exception($"An item with the identifier {identifier} has not been registered.");

        _items.Remove(item);
        _cachedSizes.Remove(identifier);
        _drawPositions.Remove(identifier);

        return this;
    }
    
    public bool TryGetItem(string id, out InlineItem? item)
    {
        item = _items.FirstOrDefault(x => x.Identifier == id);
        return item != null;
    }

    public bool TryGetDrawRect(string id, out Rectangle rect) 
        => _drawPositions.TryGetValue(id, out rect);

    public void Update(float deltaTime = 0)
    {
        // Reset per-frame state
        _currentItem = null;
        _currentPosition = new PointInt();
        _currentSize = new PointInt();
        _newLine = false;
        _linePosition = 0;
        _lineCount = 0;

        _lineItems.Clear();
        _lineItems[0] = new();
        _lineSize.Clear();
        _drawPositions.Clear();

        // First pass: measure & partition into lines
        foreach (var item in _items)
        {
            _currentItem = item;
            if (!item.Enabled)
                continue;
            
            item.Update(this, item, deltaTime);

            if (!item.CacheSize)
                ExtractCacheSize(item);

            var raw = _cachedSizes[item.Identifier];
            var itemSize = new PointInt(raw.X + item.Margin.Horizontal,
                raw.Y + item.Margin.Vertical);

            if (item.NewLine)
                _newLine = true;

            if (NewLineOrientation == Orientation.Horizontal)
            {
                if (!AutoWidth && Wrap && itemSize.X + _currentSize.X > Bounds.Width && _currentSize.X != 0)
                    Newline();

                _currentSize.X += itemSize.X;
                _currentSize.Y = Math.Max(_currentSize.Y, itemSize.Y);
            }
            else // Vertical flow
            {
                if (!AutoHeight && Wrap && itemSize.Y + _currentSize.Y > Bounds.Height && _currentSize.Y != 0)
                    Newline();

                _currentSize.X = Math.Max(_currentSize.X, itemSize.X);
                _currentSize.Y += itemSize.Y;
            }

            _lineItems[_linePosition].Add(item);

            if (_newLine)
                Newline();
        }

        // Seal the last line if it has content
        if (_currentSize.X != 0 || _currentSize.Y != 0 || _lineItems[_linePosition].Count > 0)
        {
            _lineSize[_linePosition] = _currentSize;
            _linePosition++;
        }

        _lineCount = _linePosition;

        if (AutoWidth || AutoHeight)
        {
            var measured = new PointInt();
            for (var i = 0; i < _lineCount; i++)
            {
                // width of line i incl. item spacing (n-1 gaps)
                var lineOuterW = _lineSize[i].X + (Math.Max(0, _lineItems[i].Count - 1) * ItemSpacing);
                measured.X = Math.Max(measured.X, lineOuterW);
                measured.Y += _lineSize[i].Y;
            }
            // add line spacing between lines
            if (_lineCount > 1) measured.Y += (_lineCount - 1) * LineSpacing;

            if (AutoWidth)  Bounds = new Rect(Bounds.X, Bounds.Y, measured.X, Bounds.Height);
            if (AutoHeight) Bounds = new Rect(Bounds.X, Bounds.Y, Bounds.Width, measured.Y);
        }

        // Establish the initial perpendicular position (overall block alignment)
        SetInitialPosition();

        // Second pass: per-line placement, then per-item slot & inner alignment
        for (var i = 0; i < _lineCount; i++)
        {
            var totalSize = _lineSize[i];
            
            var fillSize = CalculateFillSize(i, Bounds.Size);

            // Set starting position along the flow axis for this line
            if (NewLineOrientation == Orientation.Horizontal)
                _currentPosition.X = GetLineStartX(i); 
            else
                _currentPosition.Y = GetLineStartY(i);  

            // --- place items in this line ---
            foreach (var item in _lineItems[i])
            {
                var raw = _cachedSizes[item.Identifier];
                var outer = GetRealSize(item, fillSize); // returns OUTER (raw + margins for non-fill)
                // negative-flow slot origin:
                var slotX = _currentPosition.X;
                var slotY = _currentPosition.Y;
                if (NewLineOrientation == Orientation.Horizontal && Direction.X < 0) slotX -= outer.X;
                if (NewLineOrientation == Orientation.Vertical   && Direction.Y < 0) slotY -= outer.Y;

                // start with OUTER rect
                var rect = new Rectangle(slotX, slotY, outer.X, outer.Y);

                // cross-axis fill works on OUTER first
                if (NewLineOrientation == Orientation.Horizontal && item.FillHeight) rect.Height = _lineSize[i].Y;
                if (NewLineOrientation == Orientation.Vertical   && item.FillWidth ) rect.Width  = _lineSize[i].X;

                // now shrink by margins to get the INNER content box
                rect.X      += item.Margin.Left;
                rect.Y      += item.Margin.Top;
                rect.Width  -= item.Margin.Horizontal;
                rect.Height -= item.Margin.Vertical;

                // inner-align using RAW content size
                switch (item.HAlign)
                {
                    case HAlign.Center: 
                        rect.X += (rect.Width  - raw.X) / 2f; 
                        break;
                    case HAlign.Right:  
                        rect.X +=  rect.Width  - raw.X;       
                        break;
                }
                switch (item.VAlign)
                {
                    case 
                        VAlign.Middle: rect.Y += (rect.Height - raw.Y) / 2f; 
                        break;
                    case 
                        VAlign.Bottom: rect.Y +=  rect.Height - raw.Y;       
                        break;
                }

                _drawPositions[item.Identifier] = rect;

                // advance along flow with OUTER size + spacing
                if (NewLineOrientation == Orientation.Horizontal)
                    _currentPosition.X += (outer.X + ItemSpacing) * Direction.X;
                else
                    _currentPosition.Y += (outer.Y + ItemSpacing) * Direction.Y;
            }

            // After finishing the line, advance on the PERPENDICULAR axis to the next line/column
            if (NewLineOrientation == Orientation.Horizontal)
                _currentPosition.Y += (_lineSize[i].Y + LineSpacing) * Direction.Y;
            else
                _currentPosition.X += (_lineSize[i].X + LineSpacing) * Direction.X;
        }
    }

    private void SetInitialPosition()
    {
        if (_lineCount == 0)
            return;

        if (NewLineOrientation == Orientation.Vertical)
        {
            // Vertical flow: we pre-position Y for the whole block (sum of line heights)
            var totalHeights = 0;
            if (VAlign == VAlign.Middle)
            {
                foreach (var kv in _lineSize)
                    totalHeights += kv.Value.Y;
            }

            if (VAlign == VAlign.Middle && Direction.Y > 0)
                _currentPosition.Y = Bounds.Center.Y - (Bounds.Height - totalHeights) / 2;
            else if (VAlign == VAlign.Middle && Direction.Y < 0)
                _currentPosition.Y = Bounds.Center.Y + (Bounds.Height - totalHeights) / 2;
            else if (Direction.Y > 0)
                _currentPosition.Y = Bounds.Top;
            else
                _currentPosition.Y = Bounds.Bottom - _lineSize[0].Y;

            // X will be decided per-line based on HAlign inside the placement loop.
        }

        if (NewLineOrientation == Orientation.Horizontal)
        {
            // Horizontal flow: we pre-position X for the whole block (sum of line widths)
            var totalWidths = 0;
            if (HAlign == HAlign.Center)
            {
                foreach (var kv in _lineSize)
                    totalWidths += kv.Value.X;
            }

            if (HAlign == HAlign.Center && Direction.X > 0)
                _currentPosition.X = Bounds.Center.X - (Bounds.Width - totalWidths) / 2;
            else if (HAlign == HAlign.Center && Direction.X < 0)
                _currentPosition.X = Bounds.Center.X + (Bounds.Width - totalWidths) / 2;
            else if (Direction.X > 0)
                _currentPosition.X = Bounds.Left;
            else
                _currentPosition.X = Bounds.Right - _lineSize[0].X;

            // Y will be decided per-line based on VAlign inside the placement loop.
            if (VAlign == VAlign.Middle && Direction.Y > 0)
                _currentPosition.Y = Bounds.Center.Y - (Bounds.Height - _lineSize[0].Y) / 2;
            else if (VAlign == VAlign.Middle && Direction.Y < 0)
                _currentPosition.Y = Bounds.Center.Y + (Bounds.Height - _lineSize[0].Y) / 2;
            else if (Direction.Y > 0)
                _currentPosition.Y = Bounds.Top;
            else
                _currentPosition.Y = Bounds.Bottom - _lineSize[0].Y;
        }
    }

    private PointInt GetRealSize(InlineItem item, int filledSize)
    {
        // start from RAW content size
        var raw = _cachedSizes[item.Identifier];
        var outer = new PointInt(raw.X + item.Margin.Horizontal, raw.Y + item.Margin.Vertical);

        if (NewLineOrientation == Orientation.Horizontal && item.FillWidth && outer.X < filledSize)
            outer.X = filledSize;
        if (NewLineOrientation == Orientation.Vertical && item.FillHeight && outer.Y < filledSize)
            outer.Y = filledSize;

        return outer; // OUTER size (content + margins, with fill applied on flow axis)
    }

    private int CalculateFillSize(int i, PointInt totalSize)
    {
        var totalFilled = 0;
        var fixedSize = 0;

        foreach (var item in _lineItems[i])
        {
            var isFill = (NewLineOrientation == Orientation.Horizontal && item.FillWidth) ||
                         (NewLineOrientation == Orientation.Vertical && item.FillHeight);

            if (isFill)
                totalFilled++;
            else
                fixedSize += GetXYValueFromOrientation(_cachedSizes[item.Identifier]);
        }

        var available = GetXYValueFromOrientation(totalSize) - fixedSize;
        var fillSize = totalFilled == 0 ? 0 : available / totalFilled;

        if (totalFilled > 0)
        {
            foreach (var inlineItem in _lineItems[i])
            {
                var isFill = (NewLineOrientation == Orientation.Horizontal && inlineItem.FillWidth) ||
                             (NewLineOrientation == Orientation.Vertical && inlineItem.FillHeight);

                if (isFill && GetXYValueFromOrientation(_cachedSizes[inlineItem.Identifier]) > fillSize)
                {
                    totalFilled--;
                    fixedSize += GetXYValueFromOrientation(_cachedSizes[inlineItem.Identifier]);
                }
            }

            available = GetXYValueFromOrientation(totalSize) - fixedSize;
            fillSize = totalFilled == 0 ? 0 : available / totalFilled;
        }

        return fillSize;
    }

    private int GetXYValueFromOrientation(PointInt value)
    {
        return NewLineOrientation == Orientation.Horizontal ? value.X : value.Y;
    }

    private void Newline()
    {
        _newLine = false;
        _lineSize[_linePosition] = _currentSize;
        _linePosition++;

        if (_lineItems.ContainsKey(_linePosition))
            _lineItems[_linePosition].Clear();
        else
            _lineItems[_linePosition] = new();

        _currentSize = new PointInt(0, 0);
    }

    private int GetLineSpanX(int i)
        => _lineSize[i].X + Math.Max(0, _lineItems[i].Count - 1) * ItemSpacing;

    private int GetLineSpanY(int i)
        => _lineSize[i].Y; // spacing between lines is applied when advancing, not here
    
    private int GetLineStartX(int i)
    {
        var span = GetLineSpanX(i);

        // anchor is determined ONLY by HAlign
        var anchor = HAlign switch
        {
            HAlign.Left   => Bounds.Left,
            HAlign.Center => Bounds.Center.X - span / 2,
            HAlign.Right  => Bounds.Right - span,
            _             => Bounds.Left
        };

        // cursor sits on the leading edge for the current direction
        return Direction.X > 0 ? anchor : anchor + span;
    }

    private int GetLineStartY(int i)
    {
        var span = GetLineSpanY(i);

        var anchor = VAlign switch
        {
            VAlign.Top    => Bounds.Top,
            VAlign.Middle => Bounds.Center.Y - span / 2,
            VAlign.Bottom => Bounds.Bottom - span,
            _             => Bounds.Top
        };

        return Direction.Y > 0 ? anchor : anchor + span;
    }

    public void Draw()
    {
        foreach (var item in _items)
        {
            if (!item.Visible || !item.Enabled)
                continue;

            if (_drawPositions.TryGetValue(item.Identifier, out var rect))
                item.Draw(this, item, rect);
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        
        foreach (var item in _items)
            item.Destroy(this, item);
        
        _items.Clear();
        _cachedSizes.Clear();
        _drawPositions.Clear();
        _lineSize.Clear();
        _lineItems.Clear();
    }
}
using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Visualizer.GameObjects;
using Meatcorps.Engine.Visualizer.VisualItems;

namespace Meatcorps.Engine.Visualizer.Data;

public class VisualData
{
    public string Name { get; set; } = Guid.NewGuid().ToString();
    public List<IVisualItem> Data { get; set; } = new List<IVisualItem>();
    public VisualType VisualType { get; set; }
    public EditType EditType { get; set; } = EditType.None;
    public IVisualItem? SelectedItem { get; set; }
    public IVisualItem? EditItem { get; set; }
    public bool MultiSelect { get; set; }
    
    private List<IVisualItem> _foundItems { get; set; } = new List<IVisualItem>();

    public bool GetItemBasedOn(Vector2 position, bool matchType, out IVisualItem? item)
    {
        item = null;
        for (var i = Data.Count; i > 0; i--)
        {
            var possibleItem = Data[i - 1];
            if (matchType)
            {
                if (possibleItem is TextNode && VisualType != VisualType.Node)
                    continue;
                if (possibleItem is NodeLine && VisualType != VisualType.Line)
                    continue;
            }

            if (possibleItem.CheckMouseIsInsideItem(position))
            {
                item = possibleItem;
                return true;
            }
        }

        return false;
    }
    
    public IReadOnlyList<IVisualItem> GetItemBasedOn(RectF rect)
    {
        _foundItems.Clear();
        for (var i = Data.Count; i > 0; i--)
        {
            var possibleItem = Data[i - 1];

            if (possibleItem.CheckMouseIsInsideItem(rect))
                _foundItems.Add(possibleItem);
        }

        return _foundItems;
    }
}
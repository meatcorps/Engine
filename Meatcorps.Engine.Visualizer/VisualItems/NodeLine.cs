using System.Numerics;
using ImGuiNET;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Visualizer.GameObjects;
using Raylib_cs;

namespace Meatcorps.Engine.Visualizer.VisualItems;

public class NodeLine: IVisualItem 
{
    public Guid Id { get; set; }
    public bool Selected { get; set; }
    public int Order { get; set; }
    private MainGameObject _mainGameObject = null!;
    public Vector2 StartPoint = Vector2.Zero;
    public Vector2 EndPoint = Vector2.Zero;
    private bool EditStartPoint;
    
    public void OnInitialize(MainGameObject mainGameObject)
    {
        _mainGameObject = mainGameObject;
    }

    public bool IsColliding(IVisualItem other)
    {
        return false;
    }

    public bool CheckMouseIsInsideItem(Vector2 position, IVisualItem other)
    {
        return position.IsEqualsSafe(StartPoint) || position.IsEqualsSafe(EndPoint);
    }

    public void OnDraw()
    {
        var color = new Color(0, 255, 255);
        if (Selected)
            color = _mainGameObject.ValidMove ? Color.Yellow : Color.Red;
        else
            color = Color.Magenta;

        Raylib.DrawLineEx(StartPoint, EndPoint, 5, color);
    }

    public void OnEditorDraw()
    {
        
    }

    public VisualType Type => VisualType.Node;

    public IVisualItem Clone()
    {
        return new NodeLine()
        {
            Id = new Guid(),
            StartPoint = StartPoint,
            EndPoint = EndPoint,
            Selected = Selected,
            Order = Order
        };
    }

    public void Update(float deltaTime)
    {
        // 
    }

    public void OnDragStart(Vector2 position, EditType type)
    {
        EditStartPoint = StartPoint.IsEqualsSafe(position);
    }

    public void OnDrag(Vector2 position, EditType type)
    {
        if (type is EditType.Drag or EditType.Resize)
        {
            if (EditStartPoint)
                StartPoint = position;
            else
                EndPoint = position;
        }
    }

    public void OnDragEnd(Vector2 position, EditType type)
    {
        //
    }

    public override string ToString()
    {
        return $"{StartPoint} : {EndPoint}";
    }
}
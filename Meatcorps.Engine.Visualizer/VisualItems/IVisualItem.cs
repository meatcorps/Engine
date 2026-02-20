using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Visualizer.GameObjects;
using Meatcorps.Engine.Visualizer.Scenes;

namespace Meatcorps.Engine.Visualizer.VisualItems;

public interface IVisualItem
{
    Guid Id { get; }
    bool Selected { get; set; }
    int Order { get; set; }
    void OnInitialize(MainScene scene);
    bool IsColliding(IVisualItem other);
    bool CheckMouseIsInsideItem(Vector2 position);
    bool CheckMouseIsInsideItem(RectF rect);
    void OnDraw();
    void OnEditorDraw();
    VisualType Type { get; }
    IVisualItem Clone();
    void Update(float deltaTime);
    void OnDragStart(Vector2 position, EditType type);
    void OnDrag(Vector2 position, EditType type);
    void OnDragEnd(Vector2 position, EditType type);
}
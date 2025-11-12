using System.Numerics;
using Meatcorps.Engine.Visualizer.GameObjects;
using Raylib_cs;

namespace Meatcorps.Engine.Visualizer.VisualItems;

public interface IVisualItem
{
    Guid Id { get; }
    bool Selected { get; set; }
    int Order { get; set; }
    void OnInitialize(MainGameObject mainGameObject);
    bool IsColliding(IVisualItem other);
    bool CheckMouseIsInsideItem(Vector2 position, IVisualItem other);
    void OnDraw();
    void OnEditorDraw();
    VisualType Type { get; }
    IVisualItem Clone();
    void Update(float deltaTime);
    void OnDragStart(Vector2 position, EditType type);
    void OnDrag(Vector2 position, EditType type);
    void OnDragEnd(Vector2 position, EditType type);
}
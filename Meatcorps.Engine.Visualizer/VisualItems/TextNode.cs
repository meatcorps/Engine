using System.Numerics;
using ImGuiNET;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Tween;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.Visualizer.GameObjects;
using Meatcorps.Engine.Visualizer.Scenes;
using Raylib_cs;

namespace Meatcorps.Engine.Visualizer.VisualItems;

public class TextNode: IVisualItem 
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "Lorem ipsum";
    public string Name2 { get; set; } = "Delore set atom";
    public int Size1 { get; set; } = 16;
    public int Size2 { get; set; } = 8;
    public Rectangle Bounds { get; set; }
    public bool Selected { get; set; }
    public int Order { get; set; }
    
    public Color Color { get; set; } = new Color(0, 255, 255);
    public Color Color2 { get; set; } = new Color(255, 255, 255);
    public Color BackgroundColor { get; set; } = new Color(255, 255, 255, 0);
    
    private MainScene _scene = null!;

    private Vector2 _offset = Vector2.Zero;
    
    public float AnimationSpeedMs { get; set; } = 1000;
    public bool AnimationEnabled { get; set; }
    private FixedTimer _animationTimer { get; set; } = new FixedTimer(1000);

    public void OnInitialize(MainScene scene)
    {
        _scene = scene;
        _animationTimer = new FixedTimer(AnimationSpeedMs);
    }

    public bool IsColliding(IVisualItem other)
    {
        if (other is not TextNode node) 
            return false;
        
        return Raylib.CheckCollisionRecs(Bounds, node.Bounds);
    }

    public bool CheckMouseIsInsideItem(Vector2 position, IVisualItem other)
    {
        var rect = new RectF(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height);
        return rect.Contains(position) && Type == VisualType.Node;
    }

    public void OnDraw()
    {
        var timerNormalized = 1f;
        
        if (AnimationEnabled)
        {
            timerNormalized = _animationTimer.NormalizedElapsed;
            timerNormalized = Tween.ApplyEasing(Tween.NormalToUpDown(timerNormalized), EaseType.EaseOut);
        }

        if (_scene is null)
            return;
        
        var rect = new RectF(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height);
        
        var color = Color;
        if (Selected && !_scene.EditorIsOpen)
            color = _scene.ValidMove ? Color.Yellow : Color.Red;

        if (Selected && _scene.EditorIsOpen)
        {
            var rectLarger = rect;
            rectLarger.Inflate(3, 3);
            Raylib.DrawRectangleLinesEx(new Rectangle(rectLarger.Position, rectLarger.Size.ToVector2()), 2, Color.Yellow);
        }

        var color2 = Color2;
        /*if (Raylib.IsKeyDown(KeyboardKey.F5))
        {
            color = Raylib.ColorAlpha(color, Tween.ApplyEasing(Tween.NormalToUpDown(_animationTimer.NormalizedElapsed), EaseType.EaseInOut));
            color2 = Raylib.ColorAlpha(color2, Tween.ApplyEasing(Tween.NormalToUpDown(_animationTimer.NormalizedElapsed), EaseType.EaseInOut));
        }*/


        color = Raylib.ColorLerp(new Color(0, 0, 0, 0), color, timerNormalized);
        color2 = Raylib.ColorLerp(new Color(0, 0, 0, 0), color2, timerNormalized);
        var backgroundColor = Raylib.ColorLerp(new Color(0, 0, 0, 0), BackgroundColor, timerNormalized); 
        Console.WriteLine(timerNormalized);
        
        if (BackgroundColor.A > 0)
            Raylib.DrawRectangleRec(Bounds, backgroundColor);
        
        if (color.A > 0)
            Raylib.DrawRectangleLinesEx(Bounds, 4, color);
        
        var textSize = Raylib.MeasureTextEx(_scene.Font, Name, Size1, 1);
        var textSize2 = Raylib.MeasureTextEx(_scene.Font, Name2, Size2, 1);
        var totalTextSize = new Vector2(Math.Max(textSize.X, textSize2.X), textSize.Y + textSize2.Y + 1);
            
        var textPosition = rect.Center - totalTextSize / 2;
        
        if (color.A > 0)
            Raylib.DrawTextEx(_scene.Font, Name, textPosition, Size1, 1, color);
        
        if (color2.A > 0)
            Raylib.DrawTextEx(_scene.Font, Name2, new Vector2(rect.Center.X - (textSize2.X / 2), textPosition.Y + textSize.Y + 1), Size2, 1, color2);
    }

    public void OnEditorDraw()
    {
        var name = Name;
        var name2 = Name2;
        var size1 = Size1;
        var size2 = Size2;
        var color = new Vector4(Color.R / 255f, Color.G / 255f, Color.B / 255f, Color.A / 255f);
        var color2 = new Vector4(Color2.R / 255f, Color2.G / 255f, Color2.B / 255f, Color.A / 255f);
        var backgroundColor = new Vector4(BackgroundColor.R / 255f, BackgroundColor.G / 255f, BackgroundColor.B / 255f, BackgroundColor.A / 255f);
        var animationEnabled = AnimationEnabled;
        var animationSpeedMs = AnimationSpeedMs;
        ImGui.ColorEdit4("BackgroundColor", ref backgroundColor);
        ImGui.Separator();
        ImGui.InputText("Name", ref name, 128);
        ImGui.ColorEdit4("Color", ref color);
        ImGui.SliderInt("Size1", ref size1, 6, 100);
        ImGui.Separator();
        ImGui.InputText("Name2", ref name2, 128);
        ImGui.ColorEdit4("Color2", ref color2);
        ImGui.SliderInt("Size2", ref size2, 6, 100);
        ImGui.Separator();
        ImGui.Checkbox("Animation", ref animationEnabled);
        ImGui.InputFloat("AnimationSpeedMs", ref animationSpeedMs);
        Color = new Color(color.X, color.Y, color.Z, color.W);
        Color2 = new Color(color2.X, color2.Y, color2.Z, color.W);
        BackgroundColor = new Color(backgroundColor.X, backgroundColor.Y, backgroundColor.Z, backgroundColor.W);
        Name = name;
        Name2 = name2;
        Size1 = size1;
        Size2 = size2;
        AnimationEnabled = animationEnabled;
        AnimationSpeedMs = Math.Abs(animationSpeedMs);
        _animationTimer.ChangeSpeed(AnimationSpeedMs);
    }

    public VisualType Type => VisualType.Node;

    public IVisualItem Clone()
    {
        return new TextNode
        {
            Id = Guid.NewGuid(),
            Name = Name,
            Name2 = Name2,
            Size1 = Size1,
            Size2 = Size2,
            Bounds = Bounds,
            Selected = Selected,
            Order = Order,
            Color = Color,
            Color2 = Color2,
            BackgroundColor = BackgroundColor
        };
    }

    public void Update(float deltaTime)
    {
        _animationTimer.Update(deltaTime);
    }

    public void OnDragStart(Vector2 position, EditType type)
    {
        _offset = Bounds.Position - position;
    }

    public void OnDrag(Vector2 position, EditType type)
    {
        switch (type)
        {
            case EditType.Resize:
                var size = position - Bounds.Position;
                var test = new Vector2(100, 100);
                size = size.Clamp(new Vector2(50, 50), new Vector2(10000, 10000));
                Bounds = new Rectangle(Bounds.Position, size);
                break;
            case EditType.Drag:
                Bounds = new Rectangle(position + _offset, Bounds.Size);
                break;
        }
    }

    public void OnDragEnd(Vector2 position, EditType type)
    {
        //
    }

    public override string ToString()
    {
        return $"X{Bounds.X} Y{Bounds.X} W{Bounds.Width} H{Bounds.Height}";
    }
}
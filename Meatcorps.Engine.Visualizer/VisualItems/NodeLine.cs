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

public class NodeLine: IVisualItem 
{
    public Guid Id { get; set; }
    public bool Selected { get; set; }
    public int Order { get; set; }
    private MainScene? _scene;
    public Vector2 StartPoint = Vector2.Zero;
    public Vector2 EndPoint = Vector2.Zero;
    public bool StartArrow { get; set; }
    public bool EndArrow { get; set; }
    
    public int Thickness { get; set; } = 5;
    public Color Color { get; set; } = new Color(255,0, 255);
    
    public int DashedSize { get; set; } = 4;
    public int ArrowSize { get; set; } = 10;
    public float ArrowDegrees { get; set; } = 45;
    public LineStyleEnum LineStyle = LineStyleEnum.Solid;
    
    public string Text { get; set; } = "";
    public int TextSize { get; set; } = 8;
    public Color ColorText { get; set; } = new Color(255,255, 255);

    public float AnimationSpeedMs { get; set; } = 1000;
    public bool AnimationEnabled { get; set; }
    public bool StartEndAnimation { get; set; } = true;
    private FixedTimer _animationTimer { get; set; } = new FixedTimer(1000);
    
    private bool _editStartPoint;
    private bool _editBoth;
    private Vector2 _offset = Vector2.Zero;
    
    public void OnInitialize(MainScene scene)
    {
        _scene = scene;
    }

    public bool IsColliding(IVisualItem other)
    {
        return false;
    }

    public bool CheckMouseIsInsideItem(Vector2 position)
    {
        return position.IsEqualsSafe(StartPoint) || position.IsEqualsSafe(EndPoint);
    }

    public bool CheckMouseIsInsideItem(RectF rect)
    {
        return rect.Contains(StartPoint) || rect.Contains(EndPoint);
    }

    public void OnDraw()
    {
        if (_scene is null)
            return;
        
        var timerNormalized = 1f;
        
        if (AnimationEnabled)
        {
            timerNormalized = _animationTimer.NormalizedElapsed;
            timerNormalized = Tween.ApplyEasing(timerNormalized, EaseType.EaseInOut);
        }
        
        var color = new Color(0, 255, 255);

        if (Selected && _scene.EditorIsOpen)
        {
            Raylib.DrawLineEx(StartPoint, EndPoint, Thickness + 4, Color.Yellow);
        } else if (Selected)
            color = _scene.ValidMove ? Color.Yellow : Color.Red;
        else
            color = Color;

        var line = new LineF(StartPoint, EndPoint);

        var startPoint = StartPoint;
        var endPoint = EndPoint;
                
        if (!StartArrow)
            startPoint = startPoint - line.DirectionStartNormalized * Thickness / 2;
        if (!EndArrow)
            endPoint = endPoint - line.DirectionEndNormalized * Thickness / 2;
        
        switch (LineStyle)
        {
            case LineStyleEnum.Solid:
                if (AnimationEnabled)
                    Raylib.DrawLineEx(startPoint, endPoint, Thickness, Raylib.ColorAlpha(color, 0.3f));
                else 
                    Raylib.DrawLineEx(startPoint, endPoint, Thickness, color);
                
                break;
            case LineStyleEnum.Dashed: 
            case LineStyleEnum.Dotted:
                var strokeLength = LineStyle == LineStyleEnum.Dotted ? Thickness * 2 : Thickness * DashedSize;
                var length = line.Length;
                var dash = length / strokeLength;
                var start = line.Start;
                for (var i = 0; i < dash; i++)
                {
                    var direction = line.DirectionStartNormalized * Math.Min(length, strokeLength / 2f);
                    
                    if (AnimationEnabled)
                        Raylib.DrawLineEx(start, start + direction, Thickness, Raylib.ColorAlpha(color, 0.3f));
                    else 
                        Raylib.DrawLineEx(start, start + direction, Thickness, color);
                    
                    start += direction * 2;
                    length -= strokeLength;
                }
                break;
        }
        
        if (AnimationEnabled)
        {
            var realStartPoint = startPoint;
            var realEndPoint = endPoint;
            var startPointWithOffset = StartPoint - line.DirectionStartNormalized * Thickness * 10;
            var endPointWithOffset = EndPoint + line.DirectionStartNormalized * Thickness * 10;
            
            var currentPosition = StartEndAnimation 
                ? startPointWithOffset.Lerp(endPointWithOffset, timerNormalized) 
                : endPointWithOffset.Lerp(startPointWithOffset, timerNormalized);
            
            for (var i = 10; i > 4; i--)
            {
                var normal = 1 - Math.Clamp((i - 4) / 6f, 0, 1);
                        
                startPoint = currentPosition - line.DirectionStartNormalized * Thickness * i;
                endPoint = currentPosition + line.DirectionStartNormalized * Thickness * i;
                startPoint = startPoint.ClampSafe(realStartPoint, realEndPoint);
                endPoint = endPoint.ClampSafe(realStartPoint, realEndPoint);
                    
                Raylib.DrawLineEx(startPoint, endPoint, Thickness, Raylib.ColorLerp(Raylib.ColorAlpha(color, 0f), color, normal));
            }
        }
        
        if (StartArrow)
        {
            var offset1 = line.DirectionStartNormalized.Rotate(-MathHelper.ToRadians(ArrowDegrees));
            var offset2 = line.DirectionStartNormalized.Rotate(MathHelper.ToRadians(ArrowDegrees));
            Raylib.DrawLineEx(StartPoint - offset1 * (Thickness / 2f), StartPoint + offset1 * ArrowSize, Thickness, color);
            Raylib.DrawLineEx(StartPoint - offset2 * (Thickness / 2f), StartPoint + offset2 * ArrowSize, Thickness, color);
        }
        if (EndArrow)
        {
            
            var offset1 = line.DirectionEndNormalized.Rotate(-MathHelper.ToRadians(ArrowDegrees));
            var offset2 = line.DirectionEndNormalized.Rotate(MathHelper.ToRadians(ArrowDegrees));
            Raylib.DrawLineEx(EndPoint - offset1 * (Thickness / 2f), EndPoint + offset1 * ArrowSize, Thickness, color);
            Raylib.DrawLineEx(EndPoint - offset2 * (Thickness / 2f), EndPoint + offset2 * ArrowSize, Thickness, color);
        }
        
        if (Text.Length > 0)
        {
            var textSize = Raylib.MeasureTextEx(_scene.Font, Text, TextSize, 1);
            var textPosition = (StartPoint + EndPoint) / 2;
            var rotation = MathHelper.ToDegrees(line.Start.X < line.End.X ? line.RadiusStart : line.RadiusEnd);
            var offsetDirection = line.DirectionStartNormalized;
            Raylib.DrawTextPro(_scene.Font, Text, textPosition + offsetDirection.PerpendicularCounterClockwise() * (TextSize + 2), textSize / 2, rotation, TextSize, 1, ColorText);
        }
    }

    public void OnEditorDraw()
    {

        var color = new Vector4(Color.R / 255f, Color.G / 255f, Color.B / 255f, Color.A / 255f);
        var thickness = Thickness;
        var arrowDegrees = ArrowDegrees;
        var startArrow = StartArrow;
        var endArrow = EndArrow;
        var arrowSize = ArrowSize;
        var lineStyle = (int)LineStyle;
        var dashedSize = DashedSize;
        var items = Enum.GetNames(typeof(LineStyleEnum));
        var text = Text;
        var textSize = TextSize;
        var textColor = new Vector4(ColorText.R / 255f, ColorText.G / 255f, ColorText.B / 255f, ColorText.A / 255f);
        var animationEnabled = AnimationEnabled;
        var startEndAnimation = StartEndAnimation;
        var animationSpeedMs = AnimationSpeedMs;
        
        ImGui.ColorEdit4("Color", ref color);
        ImGui.SliderInt("Thickness", ref thickness, 1, 100);
        ImGui.Combo("LineStyle", ref lineStyle, items, items.Length);
        ImGui.SliderInt("DashedSize", ref dashedSize, 4, 10);
        ImGui.Separator();
        ImGui.Checkbox("StartArrow", ref startArrow);
        ImGui.Checkbox("EndArrow", ref endArrow);
        ImGui.SliderFloat("ArrowDegrees", ref arrowDegrees, 0, 90);
        ImGui.SliderInt("ArrowSize", ref arrowSize, 1, 100);
        ImGui.Separator();
        ImGui.InputText("Text", ref text, 128);
        ImGui.SliderInt("TextSize", ref textSize, 6, 100);
        ImGui.ColorEdit4("ColorText", ref textColor);
        ImGui.Separator();
        ImGui.Checkbox("Animation", ref animationEnabled);
        ImGui.Checkbox("startEndAnimation", ref startEndAnimation);
        ImGui.InputFloat("AnimationSpeedMs", ref animationSpeedMs);
        
        Color = new Color(color.X, color.Y, color.Z, color.W);
        Thickness = thickness;
        ArrowDegrees = arrowDegrees;
        StartArrow = startArrow;
        EndArrow = endArrow;
        ArrowSize = arrowSize;
        DashedSize = dashedSize;
        LineStyle = (LineStyleEnum)lineStyle;
        Text = text;
        TextSize = textSize;
        ColorText = new Color(textColor.X, textColor.Y, textColor.Z, textColor.W);
        AnimationEnabled = animationEnabled;
        AnimationSpeedMs = Math.Abs(animationSpeedMs);
        StartEndAnimation = startEndAnimation;
    }

    public VisualType Type => VisualType.Node;

    public IVisualItem Clone()
    {
        return new NodeLine()
        {
            Id = Guid.NewGuid(),
            StartPoint = StartPoint,
            EndPoint = EndPoint,
            Selected = Selected,
            Order = Order,
            StartArrow = StartArrow,
            EndArrow = EndArrow,
            Thickness = Thickness,
            Color = Color,
            ArrowDegrees = ArrowDegrees,
            ArrowSize = ArrowSize,
            LineStyle = LineStyle,
            DashedSize = DashedSize
        };
    }

    public void Update(float deltaTime)
    {
        if (_scene is null)
            return;
        
        _animationTimer.Update(deltaTime); 
        
        if (StartPoint.IsEqualsSafe(EndPoint) && _scene.VisualData.EditType == EditType.None)
            EndPoint += new Vector2(10, 0);
    }

    public void OnDragStart(Vector2 position, EditType type)
    {
        _editStartPoint = StartPoint.IsEqualsSafe(position);
        if (_editStartPoint)
            _offset = StartPoint - position;
        else
            _offset = EndPoint - position;
    }

    public void OnDrag(Vector2 position, EditType type)
    {
        if (_scene is null)
            return;
        
        if (type == EditType.Resize && _scene.VisualData.MultiSelect)
            return;
        
        if (type is EditType.Drag or EditType.Resize)
        {
            
            position += _offset;
            _editBoth = type == EditType.Drag;
            if (_editStartPoint)
            {
                if (_editBoth) 
                    EndPoint += position - StartPoint;
                
                StartPoint = position;
            }
            else
            {
                if (_editBoth) 
                    StartPoint += position - EndPoint;
                
                EndPoint = position;
            }
        }
    }

    public void OnDragEnd(Vector2 position, EditType type)
    {
        if (StartPoint.IsEqualsSafe(EndPoint))
            EndPoint += new Vector2(10, 0);
        //
    }

    public override string ToString()
    {
        return $"{StartPoint} : {EndPoint}";
    }
}

public enum LineStyleEnum
{
    Solid,
    Dashed,
    Dotted
}
using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Tween;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Interfaces;
using Raylib_cs;

namespace Meatcorps.Engine.Visualizer.GameObjects;

public class MainGameObject: BaseGameObject
{
    private Vector2 _mousePosition;
    private ICameraFixedWidthAndHeight _camera = null!;
    private List<VisualItem> _visualItems = new List<VisualItem>();
    private VisualItem? _selectedVisualItem = null;
    private VisualType _visualTypeToAdd = VisualType.Node;
    private EditType _editType;
    private IDefaultFont _font = null!;
    private Vector2 _offset;
    private bool _validMove;
    private FixedTimer _animationTimer = new FixedTimer(1000);
    private Editor _editor = null!;

    protected override void OnInitialize()
    {
        if (GlobalObjectManager.ObjectManager.Get<ICamera>()! is ICameraFixedWidthAndHeight camera)
            _camera = camera;

        _font = GlobalObjectManager.ObjectManager.Get<IDefaultFont>()!;
        _editor = Scene.GetGameObject<Editor>()!;
    }

    protected override void OnUpdate(float deltaTime)
    {
        _validMove = true;
        _mousePosition = Vector2ToGrid(_camera.ScreenToWorld(Raylib.GetMousePosition() / ((float)Scene.GameHost.Width / _camera.TargetWidth)));

        _animationTimer.Update(deltaTime);
        if (_selectedVisualItem is not null && _selectedVisualItem.Type == VisualType.Node)
        {
            _validMove = !CheckItemBasedOn(_selectedVisualItem.Bounds);
        }
        
        if (Raylib.IsKeyDown(KeyboardKey.Escape) && _selectedVisualItem is not null)
        {
            _selectedVisualItem.Selected = false;
            _selectedVisualItem = null;
        }

        if (Raylib.IsMouseButtonDown(MouseButton.Right) && _editType == EditType.None)
        {
            if (GetItemBasedOn(_mousePosition, out var item))
            {
                _editor.Item = item;
                _selectedVisualItem = item;
                _selectedVisualItem!.Selected = true;
                _editType = EditType.DataEnter;
            }
        }

        if (_editor.Item == null && _editType == EditType.DataEnter)
        {
            _selectedVisualItem!.Selected = false;
            _selectedVisualItem = null;
            _editType = EditType.None;
        }
        
        if (_selectedVisualItem is null && Raylib.IsMouseButtonDown(MouseButton.Left) && _editType == EditType.None)
        {
            if (GetItemBasedOn(_mousePosition, out var item))
            {
                if (Raylib.IsKeyDown(KeyboardKey.LeftAlt))
                {
                    _selectedVisualItem = item!.Clone();
                    _selectedVisualItem.Selected = true;
                    _visualItems.Add(_selectedVisualItem);
                    _editType = EditType.Drag;
                }
                else
                {
                    _selectedVisualItem = item!;
                    _selectedVisualItem.Selected = true;
                    _editType = Raylib.IsKeyDown(KeyboardKey.R) ? EditType.Resize : EditType.Drag;
                }
                _offset = new Vector2(item!.Bounds.X - _mousePosition.X, item!.Bounds.Y - _mousePosition.Y);
            }
        }

        if (_selectedVisualItem is null && Raylib.IsMouseButtonDown(MouseButton.Left) && _editType == EditType.None)
        {
            var item = new VisualItem
            {
                Id = Guid.NewGuid(),
                Bounds = new Rectangle(_mousePosition, new Vector2(10, 10)),
                Type = _visualTypeToAdd,
                Name = "Lorem ipsum",
                Name2 = "Delore set atom",
                Order = _visualItems.Count + 1,
                Selected = true,
            };
            _visualItems.Add(item);
            _selectedVisualItem = item;
            _editType = EditType.Resize;
        }

        if (_selectedVisualItem is not null && _editType == EditType.Resize)
        {
            var size = _mousePosition - _selectedVisualItem.Bounds.Position;
            size = size.Clamp(new Vector2(50, 50), new Vector2(10000, 10000));
            _selectedVisualItem.Bounds = new Rectangle(_selectedVisualItem.Bounds.Position, size);
        }
        
        if (_selectedVisualItem is not null && _editType == EditType.Drag)
        {
            _selectedVisualItem.Bounds = new Rectangle(_mousePosition + _offset, _selectedVisualItem.Bounds.Size);
        }

        if (_selectedVisualItem is not null && Raylib.IsMouseButtonUp(MouseButton.Left) && _validMove && _editType != EditType.DataEnter)
        {
            _selectedVisualItem.Selected = false;
            _selectedVisualItem = null;
            _editType = EditType.None;
        }

        if (_selectedVisualItem is not null && Raylib.IsKeyDown(KeyboardKey.Delete))
        {
            _visualItems.Remove(_selectedVisualItem);
            _selectedVisualItem = null;
        }

        if (Raylib.IsMouseButtonUp(MouseButton.Left) && _editType != EditType.None && _editType != EditType.DataEnter && _validMove)
        {
            _editType = EditType.None;
        }
    }

    
    private bool CheckItemBasedOn(Rectangle rect)
    {
        foreach (var possibleItem in _visualItems)
        {
            if (possibleItem != _selectedVisualItem && Raylib.CheckCollisionRecs(rect, possibleItem.Bounds) && possibleItem.Type == VisualType.Node)
            {
                return true;
            }
        } 
        return false;
    }
    
    private bool GetItemBasedOn(Vector2 position, out VisualItem? item)
    {
        item = null;
        foreach (var possibleItem in _visualItems)
        {
            var rect = new RectF(possibleItem.Bounds.X, possibleItem.Bounds.Y, possibleItem.Bounds.Width, possibleItem.Bounds.Height);
            if (rect.Contains(position) && possibleItem.Type == VisualType.Node)
            {
                item = possibleItem;
                return true;
            }
        } 
        return false;
    }

    private Vector2 Vector2ToGrid(Vector2 vector)
    {
        return new Vector2(MathF.Floor(vector.X / 10f), MathF.Floor(vector.Y / 10f)) * 10;
    }

    protected override void OnDraw()
    {
        Raylib.DrawTriangleLines(_mousePosition, _mousePosition + new Vector2(5, 30), _mousePosition + new Vector2(30, 15), Color.White);
        Raylib.DrawTriangleLines(_mousePosition + new Vector2(1, 1), _mousePosition + new Vector2(6, 31), _mousePosition + new Vector2(31, 16), Color.White);
        Raylib.DrawLineEx(new Vector2((int)_mousePosition.X, -_camera.TargetHeight), new Vector2((int)_mousePosition.X, _camera.TargetHeight), 2, new Color(255, 255, 255, 50));
        Raylib.DrawLineEx(new Vector2(-_camera.TargetWidth, (int)_mousePosition.Y),new Vector2( _camera.TargetWidth, (int)_mousePosition.Y), 2, new Color(255, 255, 255, 50));
        
        if (_selectedVisualItem is not null)
        {
            
            Raylib.DrawTextEx(_font.GetFont(), $"X{_selectedVisualItem.Bounds.X} Y{_selectedVisualItem.Bounds.X} W{_selectedVisualItem.Bounds.Width} H{_selectedVisualItem.Bounds.Height}", _mousePosition + new Vector2(16, 0), 8, 1, Color.Gray);
        }
        
        foreach (var item in _visualItems)
        {
            var color = new Color(0, 255, 255);
            if (item.Selected)
            {
                color = _validMove ? Color.Yellow : Color.Red;
            }
            var color2 = Color.White;
            if (Raylib.IsKeyDown(KeyboardKey.F5))
            {
                color = Raylib.ColorAlpha(color, Tween.ApplyEasing(Tween.NormalToUpDown(_animationTimer.NormalizedElapsed), EaseType.EaseInOut));
                color2 = Raylib.ColorAlpha(color2, Tween.ApplyEasing(Tween.NormalToUpDown(_animationTimer.NormalizedElapsed), EaseType.EaseInOut));
            }

            Raylib.DrawRectangleLinesEx(item.Bounds, 4, color);
            var textSize = Raylib.MeasureTextEx(_font.GetFont(), item.Name, item.Size1, 1);
            var textSize2 = Raylib.MeasureTextEx(_font.GetFont(), item.Name2, item.Size2, 1);
            var totalTextSize = new Vector2(Math.Max(textSize.X, textSize2.X), textSize.Y + textSize2.Y + 1);
            
            var rect = new RectF(item.Bounds.X, item.Bounds.Y, item.Bounds.Width, item.Bounds.Height);
            var textPosition = rect.Center - totalTextSize / 2;
            Raylib.DrawTextEx(_font.GetFont(), item.Name, textPosition, item.Size1, 1, color);
            Raylib.DrawTextEx(_font.GetFont(), item.Name2, new Vector2(rect.Center.X - (textSize2.X / 2), textPosition.Y + textSize.Y + 1), item.Size2, 1, color2);
        }
        /*
        var length = 60;
        for (var i = 0; i < length; i++)
        {
            var animationPos = (int)(_animationTimer.NormalizedElapsed * length);
            var pos = 1 - Tween.ApplyEasing(((float)Math.Abs(i - animationPos) / 25), Core.Enums.EaseType.EaseInOut);
            Console.WriteLine(pos + "|" + i);
            //if (pos > 0)
            //{
                Raylib.DrawLineEx(new Vector2(i * 10 - (_camera.TargetWidth / 2) + 200, 0),
                    new Vector2(i * 10 - (_camera.TargetWidth / 2) + 210, 0), 5,
                    Raylib.ColorLerp(new Color(255, 50, 255, 20), new Color(255, 128, 255, 255), pos));
                Raylib.DrawLineEx(new Vector2(i * 10 - (_camera.TargetWidth / 2) + 200, 0),
                    new Vector2(i * 10 - (_camera.TargetWidth / 2) + 210, 0), 10,
                    Raylib.ColorLerp(new Color(255, 50, 255, 20), new Color(255, 50, 255, 50), pos));
                Raylib.DrawLineEx(new Vector2(i * 10 - (_camera.TargetWidth / 2) + 200, 0),
                    new Vector2(i * 10 - (_camera.TargetWidth / 2) + 210, 0), 15,
                    Raylib.ColorLerp(new Color(255, 50, 255, 20), new Color(255, 50, 255, 10), pos));

                //Raylib.DrawLineEx(new Vector2(i * 10 - (_camera.TargetWidth / 2) + 50, 200), new Vector2(i * 10 + 10, 200), 10, Raylib.ColorLerp(new Color(255, 50, 255, 50), new Color(255, 50, 255, 0), pos));
                //Raylib.DrawLineEx(new Vector2(i * 10 - (_camera.TargetWidth / 2) + 50, 200), new Vector2(i * 10 + 10, 200), 20, Raylib.ColorLerp(new Color(255, 50, 255, 10), new Color(255, 50, 255, 0), pos));
            //}
        }*/
        
        base.OnDraw();
    }

    protected override void OnDispose()
    {
    }
}

public class VisualItem
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Name2 { get; set; }
    public int Size1 { get; set; } = 16;
    public int Size2 { get; set; } = 8;
    public Rectangle Bounds { get; set; }
    public bool Selected { get; set; }
    public int Order { get; set; }
    public VisualType Type { get; set; }

    public VisualItem Clone()
    {
        return new VisualItem
        {
            Id = new Guid(),
            Name = Name,
            Name2 = Name2,
            Size1 = Size1,
            Size2 = Size2,
            Bounds = Bounds,
            Selected = Selected,
            Order = Order,
            Type = Type
        };
    }
}

public enum VisualType
{
    Line,
    Node
}

public enum EditType
{
    None,
    Resize,
    DataEnter,
    Drag
}
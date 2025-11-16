using System.Numerics;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Camera;
using Meatcorps.Engine.RayLib.GameObjects.UI;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.RemixIcons;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Engine.Visualizer.Enums;
using Meatcorps.Engine.Visualizer.Services;
using Meatcorps.Engine.Visualizer.VisualItems;
using Raylib_cs;

namespace Meatcorps.Engine.Visualizer.GameObjects;

public class MainGameObject : BaseGameObject
{
    private Vector2 _mousePosition;
    private ICameraFixedWidthAndHeight _camera = null!;
    private List<IVisualItem> _visualItems = new List<IVisualItem>();
    private IVisualItem? _selectedVisualItem = null;
    private VisualType _visualTypeToAdd = VisualType.Node;
    private EditType _editType;
    private TextManager<FontEnum> _font = null!;
    private Vector2 _offset;
    private bool _validMove;
    private FixedTimer _animationTimer = new FixedTimer(1000);
    private Editor _editor = null!;
    private CameraControllerGameObject _cameraController;
    private Toolbox _toolbox;
    private UIMessageEmitter _uiMessage;
    private string _mouseText = "";
    private bool _hideUI = false;
    public DataLoaderService DataLoaderService { get; } = new DataLoaderService();

    public bool EditorIsOpen => _editor.Item != null;
    public bool ValidMove => _validMove;
    public Font Font => _font.GetFont();

    protected override void OnInitialize()
    {
        if (GlobalObjectManager.ObjectManager.Get<ICamera>()! is ICameraFixedWidthAndHeight camera)
            _camera = camera;

        _toolbox = Scene.GetGameObject<Toolbox>()!;
        _cameraController = Scene.GetGameObject<CameraControllerGameObject>()!;
        _uiMessage = Scene.GetGameObject<UIMessageEmitter>()!;

        _font = GlobalObjectManager.ObjectManager.Get<TextManager<FontEnum>>()!;
        _editor = Scene.GetGameObject<Editor>()!;
        _editor.SetDataLoaderService(this);

        _toolbox.Items.Add(new ToolboxItem
        {
            Highlight = () => _visualTypeToAdd == VisualType.Node,
            Icon = RemixIcon.t_box_fill,
            Action = () => _visualTypeToAdd = VisualType.Node,
            Name = "Add Node"
        });
        _toolbox.Items.Add(new ToolboxItem
        {
            Highlight = () => _visualTypeToAdd == VisualType.Line,
            Icon = RemixIcon.separator,
            Action = () => _visualTypeToAdd = VisualType.Line,
            Name = "Add Line"
        });
        _toolbox.Items.Add(new ToolboxItem
        {
            Icon = null
        });
        _toolbox.Items.Add(new ToolboxItem
        {
            Icon = RemixIcon.text_snippet,
            Action = () => _editor.EditName(),
            Name = "Rename document"
        });
        _toolbox.Items.Add(new ToolboxItem
        {
            Icon = RemixIcon.upload_2_fill,
            Action = () => _editor.OpenFile(),
            Name = "Load document"
        });
        _toolbox.Items.Add(new ToolboxItem
        {
            Icon = RemixIcon.save_2_fill,
            Action = SaveData,
            Name = "Save document"
        });
        _toolbox.Items.Add(new ToolboxItem
        {
            Icon = null
        });
        _toolbox.Items.Add(new ToolboxItem
        {
            Icon = RemixIcon.focus_mode,
            Action = () =>
            {
                _cameraController.SetZoom(0);
                _cameraController.SetPosition(new Vector2(0, 0));
            },
            Name = "Reset zoom and position"
        });
    }

    protected override void OnUpdate(float deltaTime)
    {
        _mouseText = "";
        _validMove = true;
        _mousePosition =
            Vector2ToGrid(_camera.ScreenToWorld(Raylib.GetMousePosition() /
                                                ((float)Scene.GameHost.Width / _camera.TargetWidth)));

        _animationTimer.Update(deltaTime);

        if (IsKeyDown(KeyboardKey.F2))
            _visualTypeToAdd = VisualType.Node;

        if (IsKeyDown(KeyboardKey.F3))
            _visualTypeToAdd = VisualType.Line;

        _hideUI = (IsKeyDown(KeyboardKey.LeftControl) || IsKeyDown(KeyboardKey.LeftSuper)) && IsKeyDown(KeyboardKey.LeftShift);
        
        _toolbox.Enabled = !_hideUI;
        
        if (IsKeyDown(KeyboardKey.F5))
        {
            var items = DataLoaderService.LoadFile(_editor.Name);
            LoadData(items);
        }

        if (IsKeyDown(KeyboardKey.LeftControl) && IsKeyDown(KeyboardKey.S))
            SaveData();

        if (IsMouseDown(MouseButton.Middle))
        {
            var delta = Raylib.GetMouseDelta() * -1;
            _cameraController.SetPosition(_cameraController.Position + delta);
        }

        if (!_editor.BlockTheEditor && Math.Abs(Raylib.GetMouseWheelMove()) > 0.1f)
        {
            _cameraController.SetZoom(_cameraController.Zoom + Raylib.GetMouseWheelMove() * 0.1f);
        }

        if (!_editor.BlockTheEditor && _selectedVisualItem is not null)
        {
            if (IsKeyPressed(KeyboardKey.Home))
            {
                _visualItems.Remove(_selectedVisualItem);
                _visualItems.Add(_selectedVisualItem);
            }
            if (IsKeyPressed(KeyboardKey.End))
            {
                _visualItems.Remove(_selectedVisualItem);
                _visualItems.Insert(0, _selectedVisualItem);
            }
            if (IsKeyPressed(KeyboardKey.PageUp))
            {
                var index = _visualItems.IndexOf(_selectedVisualItem);
                _visualItems.Remove(_selectedVisualItem);
                _visualItems.Insert(Math.Clamp(index + 1, 0, _visualItems.Count), _selectedVisualItem);
            }
            if (IsKeyPressed(KeyboardKey.PageDown))
            {
                var index = _visualItems.IndexOf(_selectedVisualItem);
                _visualItems.Remove(_selectedVisualItem);
                _visualItems.Insert(Math.Clamp(index - 1, 0, _visualItems.Count), _selectedVisualItem);
            }
        }

        //UpdateValidMove();
        UpdateCheckIfWeNeedToDeselect();
        UpdateCheckIfWeNeedToOpenEditor();
        UpdateCheckIfWeNeedToCloseTheEditor();
        UpdateOnDragStartLogic();
        UpdateCheckIfWeCanAdd();
        UpdateOnDrag();
        UpdateOnDragEnd();
        UpdateOnDelete();
        UpdateIfTheEditorIsDone();
    }

    public void LoadData(IEnumerable<IVisualItem>? items)
    {
        if (items is not null)
        {
            _visualItems.Clear();

            foreach (var item in items)
                item.OnInitialize(this);

            _visualItems.AddRange(items);

            _cameraController.SetZoom(0);
            _cameraController.SetPosition(new Vector2(0, 0));

            _uiMessage.Show("Data loaded!");
        }
    }

    public void SaveData()
    {
        DataLoaderService.SaveFile(_editor.Name, _visualItems);
        _uiMessage.Show("Data saved!");
    }

    private void UpdateIfTheEditorIsDone()
    {
        if (IsMouseUp(MouseButton.Left) && _editType != EditType.None && _editType != EditType.DataEnter &&
            _validMove)
        {
            _editType = EditType.None;
        }
    }

    private void UpdateOnDelete()
    {
        if (_selectedVisualItem is not null && IsKeyDown(KeyboardKey.Delete) && _editType != EditType.DataEnter)
        {
            _visualItems.Remove(_selectedVisualItem);
            _selectedVisualItem = null;
        }
    }

    private void UpdateOnDragEnd()
    {
        if (_selectedVisualItem is not null && IsMouseUp(MouseButton.Left) && _validMove &&
            _editType != EditType.DataEnter)
        {
            _selectedVisualItem.OnDragEnd(_mousePosition, _editType);
            _selectedVisualItem.Selected = false;
            _selectedVisualItem = null;
            _editType = EditType.None;
        }
    }

    private void UpdateOnDrag()
    {
        if (_selectedVisualItem is not null && _editType == EditType.Resize)
            _selectedVisualItem.OnDrag(_mousePosition, _editType);

        if (_selectedVisualItem is not null && _editType == EditType.Drag)
            _selectedVisualItem.OnDrag(_mousePosition, _editType);

        if (_selectedVisualItem is not null)
        {
            _mouseText = " DEL: Remove";
        }
    }

    private void UpdateCheckIfWeCanAdd()
    {
        if (_selectedVisualItem is null && _editType == EditType.None && _mouseText == "")
            _mouseText = "Add: " + _visualTypeToAdd;

        if (_selectedVisualItem is null && IsMouseDown(MouseButton.Left) && _editType == EditType.None)
        {
            IVisualItem? item = null;

            if (_visualTypeToAdd == VisualType.Node)
            {
                item = new TextNode
                {
                    Id = Guid.NewGuid(),
                    Bounds = new Rectangle(_mousePosition, new Vector2(10, 10)),
                    Name = "Lorem ipsum",
                    Name2 = "Delore set atom",
                    Order = _visualItems.Count + 1
                };
            }
            else if (_visualTypeToAdd == VisualType.Line)
            {
                item = new NodeLine()
                {
                    Id = Guid.NewGuid(),
                    StartPoint = _mousePosition,
                    EndPoint = _mousePosition,
                    Order = _visualItems.Count + 1,
                };
            }

            if (item is null)
                return;

            item.Selected = true;

            _visualItems.Add(item);
            _selectedVisualItem = item;
            _editType = EditType.Resize;

            _selectedVisualItem.OnInitialize(this);
            _selectedVisualItem.OnDragStart(_mousePosition, _editType);
        }
    }

    private void UpdateOnDragStartLogic()
    {
        if (!GetItemBasedOn(_mousePosition, out var item))
            return;
        
        if (_selectedVisualItem is null && _editType == EditType.None)
        {
            if (IsKeyDown(KeyboardKey.LeftAlt))
            {
                _mouseText = "Copy: " + _visualTypeToAdd;
            }
            else if (IsKeyDown(KeyboardKey.R))
            {
                _mouseText = "Resize: " + _visualTypeToAdd;
            }
            else
            {
                _mouseText = "Drag: " + _visualTypeToAdd + "\n  LALT: Copy | R: resize";
            }
        }


        if (_selectedVisualItem is null && IsMouseDown(MouseButton.Left) && _editType == EditType.None)
        {
            if (IsKeyDown(KeyboardKey.LeftAlt))
            {
                _selectedVisualItem = item!.Clone();
                _selectedVisualItem.OnInitialize(this);
                _selectedVisualItem.Selected = true;
                _visualItems.Add(_selectedVisualItem);
                _editType = EditType.Drag;
            }
            else
            {
                _selectedVisualItem = item!;
                _selectedVisualItem.Selected = true;
                _editType = IsKeyDown(KeyboardKey.R) ? EditType.Resize : EditType.Drag;
            }

            _selectedVisualItem.OnDragStart(_mousePosition, _editType);
        }
    }

    private void UpdateCheckIfWeNeedToCloseTheEditor()
    {
        if (_editor.Item == null && _editType == EditType.DataEnter)
        {
            _selectedVisualItem!.Selected = false;
            _selectedVisualItem = null;
            _editType = EditType.None;
        }
    }

    private void UpdateCheckIfWeNeedToOpenEditor()
    {
        if (IsMouseDown(MouseButton.Right) && _editType == EditType.None)
        {
            if (GetItemBasedOn(_mousePosition, out var item))
            {
                _editor.Item = item;
                _selectedVisualItem = item;
                _selectedVisualItem!.Selected = true;
                _editType = EditType.DataEnter;
            }
        }
    }

    private void UpdateCheckIfWeNeedToDeselect()
    {
        if (IsKeyDown(KeyboardKey.Escape, true) && _selectedVisualItem is not null)
        {
            _selectedVisualItem.Selected = false;
            _selectedVisualItem = null;
            _editor.Item = null;
            _editType = EditType.None;
        }
    }

    private void UpdateValidMove()
    {
        if (_selectedVisualItem is not null && _selectedVisualItem.Type == VisualType.Node)
        {
            foreach (var possibleItem in _visualItems)
            {
                if (possibleItem != _selectedVisualItem && _selectedVisualItem.IsColliding(possibleItem))
                {
                    _validMove = false;
                    return;
                }
            }
        }

        _validMove = true;
    }

    private bool IsKeyDown(KeyboardKey key, bool ignoreEditor = false)
    {
        if ((_editType == EditType.DataEnter || _editor.BlockTheEditor) && !ignoreEditor)
            return false;

        return Raylib.IsKeyDown(key);
    }
    
    private bool IsKeyPressed(KeyboardKey key, bool ignoreEditor = false)
    {
        if ((_editType == EditType.DataEnter || _editor.BlockTheEditor) && !ignoreEditor)
            return false;

        return Raylib.IsKeyPressed(key);
    }

    private bool IsMouseDown(MouseButton button)
    {
        if (_editType == EditType.DataEnter || _editor.BlockTheEditor || _toolbox.IsMouseOverToolbox)
            return false;

        return Raylib.IsMouseButtonDown(button);
    }

    private bool IsMouseUp(MouseButton button)
    {
        if (_editType == EditType.DataEnter || _editor.BlockTheEditor)
            return false;

        return Raylib.IsMouseButtonUp(button);
    }

    private bool GetItemBasedOn(Vector2 position, out IVisualItem? item)
    {
        item = null;
        for (var i = _visualItems.Count; i > 0; i--)
        {
            var possibleItem = _visualItems[i - 1];
            if (possibleItem is TextNode && _visualTypeToAdd != VisualType.Node)
                continue;
            if (possibleItem is NodeLine && _visualTypeToAdd != VisualType.Line)
                continue;

            if (possibleItem.CheckMouseIsInsideItem(position, possibleItem))
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
        foreach (var item in _visualItems)
            item.OnDraw();
        
        if (!_hideUI)
            Raylib.DrawTextEx(_font.GetFont(), "Name: " + _editor.Name, _camera.ScreenToWorld(new Vector2(32, 32)), 8, 1,
                Color.Gray);

        if (!_toolbox.IsMouseOverToolbox && !_editor.BlockTheEditor && !_hideUI)
        {
            Raylib.DrawTriangleLines(_mousePosition, _mousePosition + new Vector2(5, 30),
                _mousePosition + new Vector2(30, 15), Color.White);
            Raylib.DrawTriangleLines(_mousePosition + new Vector2(1, 1), _mousePosition + new Vector2(6, 31),
                _mousePosition + new Vector2(31, 16), Color.White);
            Raylib.DrawLineEx(new Vector2((int)_mousePosition.X, (int)_mousePosition.Y - _camera.TargetHeight),
                new Vector2((int)_mousePosition.X, _camera.TargetHeight), 2, new Color(255, 255, 255, 50));
            Raylib.DrawLineEx(new Vector2((int)_mousePosition.X - _camera.TargetWidth, (int)_mousePosition.Y),
                new Vector2(_camera.TargetWidth, (int)_mousePosition.Y), 2, new Color(255, 255, 255, 50));

            if (_selectedVisualItem is not null)
                Raylib.DrawTextEx(_font.GetFont(), _selectedVisualItem.ToString() + " | " + _mouseText, _mousePosition + new Vector2(16, 0),
                    8,
                    1, Color.Gray);
            else
                Raylib.DrawTextEx(_font.GetFont(), _mousePosition.ToString() + " | " + _mouseText,
                    _mousePosition + new Vector2(16, 0), 8,
                    1, Color.Gray);
        }
        
        base.OnDraw();
    }

    protected override void OnDispose()
    {
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
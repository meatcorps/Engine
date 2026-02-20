using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Camera;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.Renderer;
using Meatcorps.Engine.Visualizer.Scenes;
using Meatcorps.Engine.Visualizer.VisualItems;
using Raylib_cs;

namespace Meatcorps.Engine.Visualizer.GameObjects;

public class MainGameObject : BaseGameObject
{
    private ICameraFixedWidthAndHeight _camera = null!;
    private bool _validMove;
    private CameraControllerGameObject _cameraController = null!;
    private Toolbox _toolbox = null!;
    private RectF _selectionRect = new(0, 0, 0, 0);

    private MainScene _scene = null!;

    private bool _selectionStarted;
    
    protected override void OnInitialize()
    {
        if (GlobalObjectManager.ObjectManager.Get<ICamera>()! is ICameraFixedWidthAndHeight camera)
            _camera = camera;

        _toolbox = Scene.GetGameObject<Toolbox>()!;
        _cameraController = Scene.GetGameObject<CameraControllerGameObject>()!;

        if (RenderTarget is PixelPerfectRenderTarget pixelPerfectRenderTarget)
            pixelPerfectRenderTarget.UsePixelOffset = true;
        
        _scene = (Scene as MainScene)!;
    }

    protected override void OnUpdate(float deltaTime)
    {
        _validMove = true;

        if (_scene.IsKeyDown(KeyboardKey.F2))
            _scene.VisualData.VisualType = VisualType.Node;

        if (_scene.IsKeyDown(KeyboardKey.F3))
            _scene.VisualData.VisualType = VisualType.Line;

        _scene.HideUI = (_scene.IsKeyDown(KeyboardKey.LeftControl) || _scene.IsKeyDown(KeyboardKey.LeftSuper)) && _scene.IsKeyDown(KeyboardKey.LeftShift);
        
        _toolbox.Enabled = !_scene.HideUI;
        
        if (_scene.IsKeyDown(KeyboardKey.F5))
        {
            var items = _scene.DataLoaderService.LoadFile(_scene.VisualData.Name);
            _scene.LoadData(items);
        }

        if (_scene.IsKeyDown(KeyboardKey.LeftControl) && _scene.IsKeyDown(KeyboardKey.S))
            _scene.SaveData();
        
        if (_scene.IsMouseDown(MouseButton.Middle))
        {
            var delta = Raylib.GetMouseDelta() * -1;
            _cameraController.SetPosition(_cameraController.Position + delta);
        }

        if (!_scene.BlockTheEditor && Math.Abs(Raylib.GetMouseWheelMove()) > 0.1f)
        {
            _cameraController.SetZoom(_cameraController.Zoom + Raylib.GetMouseWheelMove() * 0.1f);
        }

        if (!_scene.BlockTheEditor && _scene.VisualData.SelectedItem is not null)
        {
            if (_scene.IsKeyPressed(KeyboardKey.Home))
            {
                _scene.VisualData.Data.Remove(_scene.VisualData.SelectedItem);
                _scene.VisualData.Data.Add(_scene.VisualData.SelectedItem);
            }
            if (_scene.IsKeyPressed(KeyboardKey.End))
            {
                _scene.VisualData.Data.Remove(_scene.VisualData.SelectedItem);
                _scene.VisualData.Data.Insert(0, _scene.VisualData.SelectedItem);
            }
            if (_scene.IsKeyPressed(KeyboardKey.PageUp))
            {
                var index = _scene.VisualData.Data.IndexOf(_scene.VisualData.SelectedItem);
                _scene.VisualData.Data.Remove(_scene.VisualData.SelectedItem);
                _scene.VisualData.Data.Insert(Math.Clamp(index + 1, 0, _scene.VisualData.Data.Count), _scene.VisualData.SelectedItem);
            }
            if (_scene.IsKeyPressed(KeyboardKey.PageDown))
            {
                var index = _scene.VisualData.Data.IndexOf(_scene.VisualData.SelectedItem);
                _scene.VisualData.Data.Remove(_scene.VisualData.SelectedItem);
                _scene.VisualData.Data.Insert(Math.Clamp(index - 1, 0, _scene.VisualData.Data.Count), _scene.VisualData.SelectedItem);
            }
        }

        //UpdateValidMove();

        
        
        UpdateCheckIfWeNeedToDeselect();
        UpdateCheckIfWeNeedToOpenEditor();
        UpdateCheckIfWeNeedToCloseTheEditor();
        if (!_selectionStarted && !_scene.IsKeyDown(KeyboardKey.LeftShift))
        {
            UpdateOnDragStartLogic();
            UpdateCheckIfWeCanAdd();
            UpdateOnDrag();
            UpdateOnDragEnd();
            UpdateOnDelete();
        }
        
        if (_scene.VisualData.MultiSelect && !_scene.BlockTheEditor && _scene.IsKeyDown(KeyboardKey.LeftShift))
        {
            if (_scene.IsMouseDown(MouseButton.Left))
            {
                if (!_selectionStarted)
                {
                    _selectionRect.Position = _scene.MousePositionGrid;

                    if (_scene.VisualData.GetItemBasedOn(_scene.MousePositionGrid, false, out var item))
                        item!.Selected = !item.Selected;
                }

                _selectionStarted = true;
                _selectionRect.Width = Math.Abs(_scene.MousePositionGrid.X - _selectionRect.Position.X);
                _selectionRect.Height = Math.Abs(_scene.MousePositionGrid.Y - _selectionRect.Position.Y);
            }
            else if (_scene.IsMouseUp(MouseButton.Left) && _selectionStarted)
            {
                if (_selectionRect.Width > 1 && _selectionRect.Height > 1)
                {

                    foreach (var itemP in _scene.VisualData.Data)
                        itemP.Selected = false;

                    foreach (var item in _scene.VisualData.GetItemBasedOn(_selectionRect))
                        item.Selected = true;
                }
                
                _selectionStarted = false;
            }
        }

        UpdateIfTheEditorIsDone();
    }

    private void UpdateIfTheEditorIsDone()
    {
        if (_scene.VisualData.MultiSelect)
            return;
        if (_scene.IsMouseUp(MouseButton.Left) && _scene.VisualData.EditType != EditType.None && _scene.VisualData.EditType != EditType.DataEnter &&
            _validMove)
        {
            _scene.VisualData.EditType = EditType.None;
        }
    }

    private void UpdateOnDelete()
    {
        if (_scene.VisualData.SelectedItem is not null && _scene.IsKeyDown(KeyboardKey.Delete) && _scene.VisualData.EditType != EditType.DataEnter)
        {
            foreach (var item in _scene.VisualData.Data.ToArray())
            {
                if (item.Selected)
                    _scene.VisualData.Data.Remove(item);
            }
            _scene.VisualData.SelectedItem = null;
        }
    }

    private void UpdateOnDragEnd()
    {
        if (_scene.VisualData.SelectedItem is not null && _scene.IsMouseUp(MouseButton.Left) && _validMove &&
            _scene.VisualData.EditType != EditType.DataEnter)
        {
            foreach (var item in _scene.VisualData.Data)
            {
                if (!item.Selected)
                    continue;
                
                item.OnDragEnd(_scene.MousePositionGrid, _scene.VisualData.EditType);
                _scene.VisualData.EditType = EditType.None;

                if (!_scene.VisualData.MultiSelect)
                {
                    item.Selected = false;
                    _scene.VisualData.SelectedItem = null;
                }
            }
        }
    }

    private void UpdateOnDrag()
    {
        if (_scene.VisualData.EditType != EditType.Resize && _scene.VisualData.EditType != EditType.Drag) 
            return;

        foreach (var item in _scene.VisualData.Data)
            if (item.Selected)
                item.OnDrag(_scene.MousePositionGrid, _scene.VisualData.EditType);
        
        if (_scene.VisualData.SelectedItem is not null)
            _scene.MouseText = " DEL: Remove";
    }

    private void UpdateCheckIfWeCanAdd()
    {
        if (_scene.VisualData.MultiSelect)
            return;
        
        if (_scene.VisualData.SelectedItem is null && _scene.VisualData.EditType == EditType.None && _scene.MouseText == "")
            _scene.MouseText = "Add: " + _scene.VisualData.VisualType;

        if (_scene.VisualData.SelectedItem is null && _scene.IsMouseDown(MouseButton.Left) && _scene.VisualData.EditType == EditType.None)
        {
            IVisualItem? item = null;

            if (_scene.VisualData.VisualType == VisualType.Node)
            {
                item = new TextNode
                {
                    Id = Guid.NewGuid(),
                    Bounds = new Rectangle(_scene.MousePositionGrid, new Vector2(10, 10)),
                    Name = "Lorem ipsum",
                    Name2 = "Delore set atom",
                    Order = _scene.VisualData.Data.Count + 1
                };
            }
            else if (_scene.VisualData.VisualType == VisualType.Line)
            {
                item = new NodeLine()
                {
                    Id = Guid.NewGuid(),
                    StartPoint = _scene.MousePositionGrid,
                    EndPoint = _scene.MousePositionGrid,
                    Order = _scene.VisualData.Data.Count + 1,
                };
            }

            if (item is null)
                return;

            item.Selected = true;

            _scene.VisualData.Data.Add(item);
            _scene.VisualData.SelectedItem = item;
            _scene.VisualData.EditType = EditType.Resize;

            _scene.VisualData.SelectedItem.OnInitialize(_scene);
            _scene.VisualData.SelectedItem.OnDragStart(_scene.MousePositionGrid, _scene.VisualData.EditType);
        }
    }

    private void UpdateOnDragStartLogic()
    {
        if (!_scene.VisualData.GetItemBasedOn(_scene.MousePositionGrid, !_scene.VisualData.MultiSelect, out var item))
            return;
        
        IVisualItem[] items = [];
        bool canStart;

        if (!_scene.VisualData.MultiSelect)
        {
            items = new[] { item! };
            canStart = _scene.VisualData.SelectedItem is null;
        }
        else
        {
            if (_scene.IsMouseDown(MouseButton.Left) && _scene.VisualData.EditType == EditType.None)
                items = _scene.VisualData.Data.Where(x => x.Selected).ToArray();
            canStart = items.Length > 0;
        }

        if (canStart && _scene.VisualData.EditType == EditType.None)
        {
            if (_scene.IsKeyDown(KeyboardKey.LeftAlt))
            {
                _scene.MouseText = "Copy: " + _scene.VisualData.VisualType;
            }
            else if (_scene.IsKeyDown(KeyboardKey.R))
            {
                _scene.MouseText = "Resize: " + _scene.VisualData.VisualType;
            }
            else
            {
                _scene.MouseText = "Drag: " + _scene.VisualData.VisualType + "\n  LALT: Copy | R: resize";
            }
        }


        if (canStart && _scene.IsMouseDown(MouseButton.Left) && _scene.VisualData.EditType == EditType.None)
        {
            foreach (var visualItem in items)
            {
                if (_scene.IsKeyDown(KeyboardKey.LeftAlt))
                {
                    _scene.VisualData.SelectedItem = visualItem.Clone();
                    visualItem.Selected = false;
                    _scene.VisualData.SelectedItem.OnInitialize(_scene);
                    _scene.VisualData.SelectedItem.Selected = true;
                    _scene.VisualData.Data.Add(_scene.VisualData.SelectedItem);
                    _scene.VisualData.EditType = EditType.Drag;
                }
                else
                {
                    _scene.VisualData.SelectedItem = visualItem;
                    _scene.VisualData.SelectedItem.Selected = true;
                    _scene.VisualData.EditType = _scene.IsKeyDown(KeyboardKey.R) ? EditType.Resize : EditType.Drag;
                }

                _scene.VisualData.SelectedItem.OnDragStart(_scene.MousePositionGrid, _scene.VisualData.EditType);
            }
        }
    }

    private void UpdateCheckIfWeNeedToCloseTheEditor()
    {
        if (_scene.VisualData.EditItem == null && _scene.VisualData.EditType == EditType.DataEnter)
        {
            _scene.VisualData.SelectedItem!.Selected = false;
            _scene.VisualData.SelectedItem = null;
            _scene.VisualData.EditType = EditType.None;
        }
    }

    private void UpdateCheckIfWeNeedToOpenEditor()
    {
        if (_scene.VisualData.MultiSelect)
            return;
        
        if (_scene.IsMouseDown(MouseButton.Right) && _scene.VisualData.EditType == EditType.None)
        {
            if (_scene.VisualData.GetItemBasedOn(_scene.MousePositionGrid, true, out var item))
            {
                _scene.VisualData.EditItem = item;
                _scene.VisualData.SelectedItem = item;
                _scene.VisualData.SelectedItem!.Selected = true;
                _scene.VisualData.EditType = EditType.DataEnter;
            }
        }
    }

    private void UpdateCheckIfWeNeedToDeselect()
    {
        if (_scene.IsKeyDown(KeyboardKey.Escape, true))
        {
            if (_scene.VisualData.SelectedItem != null)
                _scene.VisualData.SelectedItem.Selected = false;
            
            _scene.VisualData.SelectedItem = null;
            _scene.VisualData.EditItem = null;
            _scene.VisualData.EditType = EditType.None;
            foreach (var item in _scene.VisualData.Data) 
                item.Selected = false;
        }
    }
    /*
    private void UpdateValidMove()
    {
        if (_scene.VisualData.SelectedItem is not null && _scene.VisualData.SelectedItem.Type == VisualType.Node)
        {
            foreach (var possibleItem in _scene.VisualData.Data)
            {
                if (possibleItem != _scene.VisualData.SelectedItem && _scene.VisualData.SelectedItem.IsColliding(possibleItem))
                {
                    _validMove = false;
                    return;
                }
            }
        }

        _validMove = true;
    }*/

    protected override void OnDraw()
    {
        foreach (var item in _scene.VisualData.Data)
            item.OnDraw();
        
        if (!_scene.HideUI)
            Raylib.DrawTextEx(_scene.Font, "Name: " + _scene.VisualData.Name, _camera.ScreenToWorld(new Vector2(32, 32)), 8, 1,
                Color.Gray);

        if (!_toolbox.IsMouseOverToolbox && !_scene.BlockTheEditor && !_scene.HideUI)
        {
            Raylib.DrawTriangleLines(_scene.MousePositionGrid, _scene.MousePositionGrid + new Vector2(5, 30),
                _scene.MousePositionGrid + new Vector2(30, 15), Color.White);
            Raylib.DrawTriangleLines(_scene.MousePositionGrid + new Vector2(1, 1), _scene.MousePositionGrid + new Vector2(6, 31),
                _scene.MousePositionGrid + new Vector2(31, 16), Color.White);
            
            var topLeftCorner = _camera.ScreenToWorld(Vector2.Zero);
            var bottomRightCorner = _camera.ScreenToWorld(new Vector2(Scene.GameHost.Width, Scene.GameHost.Height));
            Raylib.DrawLineEx(
                new Vector2((int)_scene.MousePositionGrid.X, topLeftCorner.Y),
                new Vector2((int)_scene.MousePositionGrid.X, bottomRightCorner.Y), 2, new Color(255, 255, 255, 50));
            Raylib.DrawLineEx(
                new Vector2(topLeftCorner.X, (int)_scene.MousePositionGrid.Y),
                new Vector2(bottomRightCorner.X, (int)_scene.MousePositionGrid.Y), 2, new Color(255, 255, 255, 50));
            
            if (_scene.VisualData.SelectedItem is not null)
                Raylib.DrawTextEx(_scene.Font, _scene.VisualData.SelectedItem + " | " + _scene.MouseText, _scene.MousePositionGrid + new Vector2(16, 0),
                    8,
                    1, Color.Gray);
            else
                Raylib.DrawTextEx(_scene.Font, _scene.MousePositionGrid.ToString() + " | " + _scene.MouseText,
                    _scene.MousePositionGrid + new Vector2(16, 0), 8,
                    1, Color.Gray);
            
            if (_selectionStarted)
                Raylib.DrawRectangleLinesEx(new Rectangle(_selectionRect.Position, _selectionRect.Width, _selectionRect.Height), 2, Color.White);
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
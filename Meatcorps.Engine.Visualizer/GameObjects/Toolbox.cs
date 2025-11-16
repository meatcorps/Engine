using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Tween;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.RemixIcons;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Engine.Visualizer.Enums;
using Raylib_cs;

namespace Meatcorps.Engine.Visualizer.GameObjects;

public class Toolbox: BaseGameObject
{
    public List<ToolboxItem> Items { get; set; } = new();
    private RectF _size;
    private Vector2 _mousePosition;
    private ICameraFixedWidthAndHeight _camera;
    private TextManager<FontEnum> _font;
    private bool _ignoreMouse;

    public bool IsMouseOverToolbox { get; private set; }

    protected override void OnInitialize()
    {
        Camera = CameraLayer.UI;
        if (GlobalObjectManager.ObjectManager.Get<ICamera>()! is ICameraFixedWidthAndHeight camera)
            _camera = camera;
        _font = GlobalObjectManager.ObjectManager.Get<TextManager<FontEnum>>()!;
    }

    protected override void OnUpdate(float deltaTime)
    {
        _mousePosition = Raylib.GetMousePosition() /
                         ((float) Scene.GameHost.Width / _camera.TargetWidth);
        
        var totalHeight = 0;
        
        if (Raylib.IsMouseButtonUp(MouseButton.Left))
            _ignoreMouse = false;

        foreach (var item in Items)
        {
            if (!item.Enabled())
                continue;
            item.ClickTimer.Update(true, deltaTime);
            var rect = new RectF(32, totalHeight + 32, 40, 40);
            totalHeight += item.Icon is null ? 4 : 42;

            if (item.Icon is null) 
                continue;
            
            item.IsMouseOver = rect.Contains(_mousePosition);

            if (item.IsMouseOver && Raylib.IsMouseButtonDown(MouseButton.Left) && !_ignoreMouse)
            {
                item.Action();
                item.ClickTimer.Reset();
                _ignoreMouse = true;
            }
        }

        _size = new RectF(32, 32, 40, totalHeight);
        IsMouseOverToolbox = _size.Contains(_mousePosition);
        
        var screen = new RectF(0, 0, _camera.TargetWidth, _camera.TargetHeight);
        if (screen.Contains(_mousePosition))
            Raylib.HideCursor();
    }

    protected override void OnDraw()
    {
        var totalX = 32;
        var position = new Vector2(32, 32);
        var toolText = "";
        foreach (var item in Items)
        {
            if (!item.Enabled())
                continue;

            if (item.Icon is not null)
            {
                var color = Raylib.ColorLerp(Color.White, Color.Magenta, item.ClickTimer.NormalizedElapsed);
                if (item.ClickTimer.Output)
                {
                    if (IsMouseOverToolbox) 
                        color = item.IsMouseOver ? Color.Magenta : Color.White;
                    else
                        color = new Color(255, 255, 255, 100);
                }

                Raylib.DrawRectangleRec(new Rectangle(position, 40, 40),
                    item.Highlight() ? new Color(100, 0, 100) : Color.Black);
                Raylib.DrawRectangleLinesEx(new Rectangle(position, 40, 40), 2, color);
                _font.DrawRemixIcon(FontEnum.Icons, item.Icon.Value, position + new Vector2(4, 4), 32, color);

                if (item.IsMouseOver)
                {
                    toolText = item.Name;
                }
            }

            position += new Vector2(0, item.Icon is null ? 4 : 42);
        }
        
        if (IsMouseOverToolbox)
        {
            Raylib.DrawTriangleLines(_mousePosition, _mousePosition + new Vector2(5, 30),
                _mousePosition + new Vector2(30, 15), Color.White);
            Raylib.DrawTriangleLines(_mousePosition + new Vector2(1, 1), _mousePosition + new Vector2(6, 31),
                _mousePosition + new Vector2(31, 16), Color.White);

            if (!string.IsNullOrEmpty(toolText))
            {
                var size = Raylib.MeasureTextEx(_font.GetFont(), toolText, 8, 1);
                Raylib.DrawRectangleRec(new Rectangle(_mousePosition + new Vector2(8, 0), size + new Vector2(2, 2)),
                    Color.Black);
                Raylib.DrawTextEx(_font.GetFont(), toolText, _mousePosition + new Vector2(9, 1), 8, 1, Color.White);
            }
        }
    }

    protected override void OnDispose()
    {
    }
}

public class ToolboxItem
{
    public string Name { get; set; } = "";
    public RemixIcon? Icon { get; set; }
    public Action Action { get; set; } = () => { };
    public Func<bool> Enabled { get; set; } = () => true;
    public Func<bool> Highlight { get; set; } = () => false;
    public bool IsMouseOver { get; set; } = false;
    public int Order { get; set; } = 0;
    public TimerOn ClickTimer { get; set; } = new TimerOn(500);
}
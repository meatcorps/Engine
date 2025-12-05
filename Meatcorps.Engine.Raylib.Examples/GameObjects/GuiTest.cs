using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Interfaces.Components;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Engine.RayLib.Resources;
using Raylib_cs;

namespace Meatcorps.Engine.Raylib.Examples.GameObjects;

public class GuiTest: BaseGameObject
{
    private GuiServiceComponent _gui;
    private TextManager<DefaultFont> _textManager;
    private float _offsetX = 32;
    
    protected override void OnInitialize()
    {
        Scene.SceneObjectManager.Register(new GuiService());
        _gui = new GuiServiceComponent(Scene.SceneObjectManager, this);
        _textManager = GlobalObjectManager.ObjectManager.Get<TextManager<DefaultFont>>()!;
        AddComponent(_gui);
        Camera = CameraLayer.UI;
    }

    protected override void OnUpdate(float deltaTime)
    {
        _offsetX -= deltaTime * 5;
        _gui.SetStartPosition(new Vector2(32 + _offsetX, 32));
        _gui.AddItem(GuiItem.ClipStart(new Rect(32, 32, 200, 200)));
        _gui.AddItem(GuiItem.CreateLabel(_textManager, DefaultFont.Default, "Hello ", 20, 1));
        _gui.AddItem(GuiItem.CreateLabel(_textManager, DefaultFont.Default, "World! ", 20, 1, Color.Magenta));
        _gui.AddItem(GuiItem.CreateLabel(_textManager, DefaultFont.Default, "Can we see this? ", 20, 1));
        _gui.AddItem(GuiItem.NewLine());
        _gui.AddItem(GuiItem.CreateLabel(_textManager, DefaultFont.Default, "Another line ", 8, 1, Color.Red));
        _gui.AddItem(GuiItem.CreateLabel(_textManager, DefaultFont.Default, "and it works ", 8, 1, Color.Red));
        _gui.AddItem(GuiItem.NewLine());
        _gui.AddItem(GuiItem.CreateLabel(_textManager, DefaultFont.Default, "Does it works? ", 8, 1, Color.Red));
        _gui.AddItem(GuiItem.CreateLabel(_textManager, DefaultFont.Default, "Yes it does! ", 8, 1, Color.Red));
        _gui.AddItem(GuiItem.ClipStop());
        
    }

    protected override void OnDispose()
    {
    }
}

public class GuiServiceComponent : IGameComponent
{
    private readonly ObjectManager _objectManager;
    private readonly BaseGameObject _gameObject;
    private GuiService _guiService;
    private List<GuiItem> _guiItems = new List<GuiItem>();
    public RectF StartBound { get; set; }
    public RectF CurrentBound { get; set; }
    public SizeF LargestSize { get; set; }

    public GuiServiceComponent(ObjectManager objectManager, BaseGameObject gameObject)
    {
        _objectManager = objectManager;
        _gameObject = gameObject;
    }

    public void AddItem(GuiItem item)
    {
        _guiItems.Add(item);
    }

    public void SetStartPosition(Vector2 position)
    {
        StartBound = new RectF(position.X, position.Y, StartBound.Width, StartBound.Height);
    } 
    
    public void Initialize()
    {
        _guiService = _objectManager.Get<GuiService>()!;
        StartBound = new RectF(0, 0, _gameObject.RenderTarget!.RenderWidth, _gameObject.RenderTarget!.RenderHeight);
    }

    public void PreUpdate(float deltaTime)
    {
        _guiItems.Clear();
    }

    public void Update(float deltaTime)
    {
        
    }

    public void LateUpdate(float deltaTime)
    {
        
    }

    public void Draw()
    {
        CurrentBound = StartBound;
        
        foreach (var item in _guiItems)
        {
            var nextBound = item.Draw(this, CurrentBound);
            LargestSize = new SizeF(
                Math.Max(LargestSize.Width, nextBound.Size.Width), 
                Math.Max(LargestSize.Height, nextBound.Size.Height));
            
            var currentBound = CurrentBound;
            currentBound.X += nextBound.Size.Width;
            CurrentBound = currentBound;
        }
        
    }
}


public class GuiItem
{
    public RectF Bound { get; set; }
    
    public Func<GuiServiceComponent, RectF, RectF> Draw { get; set; }

    public GuiItem(Func<GuiServiceComponent, RectF, RectF> draw)
    {
        Draw = draw;
    }

    public void Update(float deltaTime)
    {
        
    }

    public static GuiItem CreateLabel<T>(TextManager<T> manager, T font, string text, float size, float spacing, Color? color = null) where T: Enum
    {
        var fontSize = manager.MeasureText(font, text, size, spacing);
        color ??= Color.White;
            
        return new GuiItem((gui, bound) =>
        {
            Raylib_cs.Raylib.DrawTextEx(manager.GetFont(font), text, new Vector2(bound.X, bound.Y), size, spacing, color.Value);
            return new RectF(0, 0, fontSize.X, fontSize.Y);
        });
    }

    public static GuiItem NewLine()
    {
        return new GuiItem((gui, bound) =>
        {
            gui.CurrentBound = new RectF(gui.StartBound.X, gui.CurrentBound.Y + gui.LargestSize.Height,
                gui.StartBound.Width, gui.StartBound.Height);
            
            gui.LargestSize = new SizeF(0, 0);
            return new RectF();
        });
    }

    public static GuiItem ClipStart(Rect clipBound)
    {
        return new GuiItem((gui, bound) =>
        {
            Raylib_cs.Raylib.BeginScissorMode(clipBound.X, clipBound.Y, clipBound.Width, clipBound.Height);
            return new RectF();
        });
    }
    public static GuiItem ClipStop()
    {
        return new GuiItem((gui, bound) =>
        {
            Raylib_cs.Raylib.EndScissorMode();
            return new RectF();
        });
    }
}

public class GuiService
{
    
}
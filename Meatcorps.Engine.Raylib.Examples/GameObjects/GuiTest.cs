using System.Buffers;
using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Tween;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Audio;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.Raylib.Examples.Enums;
using Meatcorps.Engine.Raylib.Examples.GameObjects.Gui;
using Meatcorps.Engine.Raylib.Examples.GameObjects.Gui.Core;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Engine.RayLib.Resources;
using Raylib_cs;

namespace Meatcorps.Engine.Raylib.Examples.GameObjects;

public class GuiTest : BaseGameObject
{
    private GuiService _guiService = null!;
    private GuiServiceComponent _gui = null!;
    private TextManager<DefaultFont> _textManager = null!;
    private PlayerInputRouter<GameInput> _input = null!;
    private int _menuPosition = 0;
    private List<MenuItem> _menuItems = new List<MenuItem>();
    private TimerOn _selectedTimer = new TimerOn(500);
    private FixedTimer _animationTimer = new FixedTimer(50);
    private FixedTimer _helperAnimation = new FixedTimer(2000);
    private bool _selected = false;
    private SoundFxManager<GameSounds> _soundManager = null!;
    private SmoothValue _menuPositionSmooth = new SmoothValue(0, 0.5f);

    protected override void OnInitialize()
    {
        _input = GlobalObjectManager.ObjectManager.Get<PlayerInputRouter<GameInput>>()!;
        _guiService = new GuiService();
        Scene.SceneObjectManager.Register(_guiService);
        Scene.SceneObjectManager.Add<IBackgroundService>(_guiService);
        _gui = new GuiServiceComponent(Scene.SceneObjectManager);
        _textManager = GlobalObjectManager.ObjectManager.Get<TextManager<DefaultFont>>()!;
        _soundManager = GlobalObjectManager.ObjectManager.Get<SoundFxManager<GameSounds>>()!;
        AddComponent(_gui);
        Camera = CameraLayer.UI;
        
        _menuItems.Add(new MenuItem { Name = "START GAME", OnSelected = () => { Console.WriteLine("Start Game"); } });
        _menuItems.Add(new MenuItem { Name = "OPTIONS", OnSelected = () => { Console.WriteLine("Options"); } });
        _menuItems.Add(new MenuItem { Name = "ARCHIEVEMENTS", OnSelected = () => { Console.WriteLine("Archievements"); } });
        _menuItems.Add(new MenuItem { Name = "CREDITS", OnSelected = () => { Console.WriteLine("CREDITS"); } });
        _menuItems.Add(new MenuItem { Name = "EXIT", OnSelected = () => { Environment.Exit(0); } });
    }

    protected override void OnUpdate(float deltaTime)
    {
        _selectedTimer.Update(_selected, deltaTime);
        _animationTimer.Update(deltaTime);
        _menuPositionSmooth.Update(deltaTime);
        _helperAnimation.Update(deltaTime);
        
        if (!_selected)
        {
            if (_input.GetState(1, GameInput.Left).IsPressed && _menuPosition > 0)
            {
                _menuPosition--;
                _soundManager.Play(GameSounds.Scorechange);
            }

            if (_input.GetState(1, GameInput.Right).IsPressed && _menuPosition < _menuItems.Count - 1)
            {
                _menuPosition++;
                _soundManager.Play(GameSounds.Scorechange);
            }

            if (_input.GetState(1, GameInput.Start).IsPressed)
            {
                _selected = true;
                _soundManager.Play(GameSounds.PowerUpScore);
            }
        }

        _menuPositionSmooth.RealValue = _menuPosition * 180f;

        if (_selectedTimer.Output)
        {
            _selected = false;
            _menuItems[_menuPosition].OnSelected();
        }
        
        var offsetHelperX = (Tween.ApplyEasing(Tween.NormalToUpDown(_helperAnimation.NormalizedElapsed), EaseType.EaseInOut) * 32f) - 16;

        _gui.Start();
        _gui.AddItem(new PanelElement(new RectF(0, 0, RenderTarget!.RenderWidth, RenderTarget!.RenderHeight), UVHelper.Bottom).SetPadding(PaddingF.All(32)));
        _gui.AddItem(new PanelElement(new RectF(0, 0, 180, 64), UVHelper.Center, false, false));
        if (!_selected)
            _gui.AddItem(new TextElement(Color.White, _textManager.GetFont(), "< > ENTER", 8, 1, UVHelper.Top)
                .SetOffset(new Vector2(offsetHelperX, 40)));
        _gui.AddItem(new ScrollElement(new RectF(), new Vector2(-_menuPositionSmooth.DisplayValue, 0)));
        _gui.AddItem(new StackElement(new RectF(32, 32, 220, 32), 4, Direction.Right, UVHelper.Left));

        for (var i = 0; i < _menuItems.Count; i++)
        {
            var color = Color.Gray;
            var menuItem = _menuItems[i];

            if (i == _menuPosition)
            {
                if (_selected)
                    color = _animationTimer.Output ? Color.Magenta : Color.Blank;
                else
                    color = Color.Magenta;
            }

            _gui.AddItem(new PanelElement(new RectF(0, 0, 180, 32)));
            _gui.AddItem(new RectangleLinesElement(color, 2));
            _gui.AddItem(new TextElement(color, _textManager.GetFont(), menuItem.Name, 12));
            _gui.CloseItem();
        }
        
        _gui.CloseItem();
        _gui.CloseItem();
        _gui.CloseItem();
        _gui.CloseItem();
    }


    protected override void OnDraw()
    {
        base.OnDraw();
    }
    
    protected override void OnDispose()
    {
    }
}

public class MenuItem
{
    public string Name { get; set; }
    public Action OnSelected { get; set; }
}

/*
 *
 * _gui.AddItem(new PanelElement(new RectF(32, 32, 200, 200)));
            //_gui.AddItem(new RectangleLinesElement(Color.White, 2));
            _gui.AddItem(new StackElement(new RectF(0, 0, 200, 200), uv: UVHelper.LeftTop));
                _gui.AddItem(new PanelElement(new RectF(0, 0, 50, 50)));
                    _gui.AddItem(new RectangleLinesElement(Color.White, 2));
                _gui.CloseItem();
                _gui.AddItem(new PanelElement(new RectF(0, 0, 100, 20)));
                    _gui.AddItem(new RectangleLinesElement(Color.Red, 2));
                    _gui.AddItem(new TextElement<DefaultFont>(Color.White, _textManager.GetFont(), "Hello"));
                _gui.CloseItem();
                _gui.AddItem(new PanelElement(new RectF(0, 0, 40, 50)));
                    _gui.AddItem(new RectangleLinesElement(Color.Green, 2));
                _gui.CloseItem();
                _gui.AddItem(new PanelElement(new RectF(0, 0, 30, 50)));
                    _gui.AddItem(new RectangleLinesElement(Color.Blue, 2));
                _gui.CloseItem();
                _gui.AddItem(new StackElement(new RectF(0, 0, 0, 0), 4, new Vector2(1, 0), UVHelper.Left));
                    _gui.AddItem(new PanelElement(new RectF(0, 0, 50, 50)));
                    _gui.AddItem(new RectangleLinesElement(Color.White, 2));
                    _gui.CloseItem();
                    _gui.AddItem(new PanelElement(new RectF(0, 0, 50, 50)));
                    _gui.AddItem(new RectangleLinesElement(Color.White, 2));
                    _gui.CloseItem();
                    _gui.AddItem(new PanelElement(new RectF(0, 0, 50, 50)));
                    _gui.AddItem(new RectangleLinesElement(Color.White, 2));
                    _gui.CloseItem();
                _gui.CloseItem();
            _gui.CloseItem();
        _gui.CloseItem();
 */
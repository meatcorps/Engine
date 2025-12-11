using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.Raylib.Examples.Enums;
using Meatcorps.Engine.Raylib.Examples.GameObjects.Gui;
using Meatcorps.Engine.Raylib.Examples.GameObjects.Gui.Components;
using Meatcorps.Engine.Raylib.Examples.GameObjects.Gui.Core;
using Meatcorps.Engine.Raylib.Examples.GameObjects.Gui.GuiSettings;
using Meatcorps.Engine.RayLib.Resources;
using Raylib_cs;

namespace Meatcorps.Engine.Raylib.Examples.GameObjects;

public class GuiTest : BaseGameObject
{
    private GuiService _guiService = null!;
    private GuiServiceComponent _gui = null!;
    private TextManager<DefaultFont> _textManager = null!;
    private GuiMenuComponent _guiMenu = null!;
    private bool _boolValueTest;
    private float _normalValueTest;
    private int _intValueTest;
    private int _optionValueTest;
    private bool _areYouSure = false;

    protected override void OnInitialize()
    {
        _guiService = new GuiService();
        Scene.SceneObjectManager.Register(_guiService);
        Scene.SceneObjectManager.Add<IBackgroundService>(_guiService);
        _textManager = GlobalObjectManager.ObjectManager.Get<TextManager<DefaultFont>>()!;
        var uiSettings = new DefaultGuiSettings<GameInput, GameSounds>()
        {
            Font = _textManager.GetFont(),
            BackPressed = GameInput.Back,
            DownKey = GameInput.Down,
            UpKey = GameInput.Up,
            LeftKey = GameInput.Left,
            RightKey = GameInput.Right,
            OnSelectionPressed = GameInput.Action,
            ErrorSound = GameSounds.Alarm,
            NavigationSound = GameSounds.Scorechange,
            SelectionSound = GameSounds.PowerUpScore,
            NotificationSound = GameSounds.Backgroundplaced,
            PlayerInputId = 1
        };
        uiSettings.Load();
        _gui = AddComponent(new GuiServiceComponent(Scene.SceneObjectManager));
        _guiMenu = AddComponent(new GuiMenuComponent(uiSettings));
        
        Camera = CameraLayer.UI;
    }

    protected override void OnUpdate(float deltaTime)
    {
        _gui.Start();
        _gui.AddItem(new PanelElement(new RectF(0, 0, RenderTarget!.RenderWidth, RenderTarget!.RenderHeight),
            UVHelper.RightBottom).SetPadding(PaddingF.All(32)));
        _guiMenu.SetOrientation(MenuDirection.UpDown);
        _guiMenu.SetSizeMenuItems(new SizeF(250, 24));
        _guiMenu.SetActiveColor(new Color(0, 255, 255));
        _guiMenu.SetBorderStyle(1f, 2);
        _guiMenu.SetUseSmoothCenter(false);
        _guiMenu.SetTextUv(UVHelper.Left);
        _guiMenu.SetGap(8);
        _guiMenu.Start();

        if (_areYouSure)
        {
            _guiMenu.MenuLabel("Are you sure?");
            if (_guiMenu.MenuItem("Yes!"))
                Environment.Exit(0);
            if (_guiMenu.MenuItem("Nope..."))
            {
                _areYouSure = false;
                _guiMenu.Reset();
            }
        }
        else
        {
            if (_guiMenu.MenuItem("Start"))
                Console.WriteLine("Start");

            if (_guiMenu.MenuItem("Options"))
                Console.WriteLine("Options");

            _guiMenu.MenuBoolSwitch("Achievements", ref _boolValueTest);
            _guiMenu.MenuNormalSlider("Volume", ref _normalValueTest, playSoundBasedOnNormal: true);
            _guiMenu.MenuNextItemIsDisabled();
            _guiMenu.MenuIntSlider("Total players", ref _intValueTest, minValue: 1, maxValue: 4);
            _guiMenu.MenuOptions("Difficulty", ["Easy", "Normal", "Hard"], ref _optionValueTest);

            if (_boolValueTest)
                if (_guiMenu.MenuItem("Credits"))
                    Console.WriteLine("Credits");

            if (_guiMenu.MenuItem("Exit"))
            {
                _areYouSure = true;
                _guiMenu.Reset();
            }
        }

        _guiMenu.Stop();
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
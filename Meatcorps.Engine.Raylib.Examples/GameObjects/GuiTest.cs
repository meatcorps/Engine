using System.Globalization;
using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.Raylib.Examples.Enums;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Engine.RayLib.GameObjects.UI;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Engine.RayLib.UI.GuiComponent;
using Meatcorps.Engine.RayLib.UI.GuiComponent.Components;
using Meatcorps.Engine.RayLib.UI.GuiComponent.Core;
using Meatcorps.Engine.RayLib.UI.GuiComponent.GuiSettings;
using Raylib_cs;

namespace Meatcorps.Engine.Raylib.Examples.GameObjects;

public class GuiTest : BaseGameObject
{
    private GuiServiceComponent _gui = null!;
    private GuiService _guiService = null!;
    private List<RectF> _rains = new();
    private DefaultGuiSettings<GameInput, GameSounds> _uiSettings;
    private OneTexture _bgTexture = null!;

    protected override void OnInitialize()
    {
        _guiService = new GuiService();
        Scene.SceneObjectManager.Register(_guiService);
        Scene.SceneObjectManager.Add<IBackgroundService>(_guiService);
        _bgTexture = GlobalObjectManager.ObjectManager.Get<OneTexture>("BGPIC")!;
        _uiSettings = new DefaultGuiSettings<GameInput, GameSounds>
        {
            Font = GlobalObjectManager.ObjectManager.Get<IDefaultFont>()!.GetFont(),
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
            PlayerInputId = 1,
            FontScaleSize = 1
        };
        
        _uiSettings.Load();
        _gui = AddComponent(new GuiServiceComponent(Scene.SceneObjectManager));

        //Scene.AddGameObject(new GameSettingsGameObject<GameInput>(_uiSettings, ["16:9"]));
        Scene.AddGameObject(new MainMenuGameObject<GameInput>(_uiSettings));

        Camera = CameraLayer.UI;

        for (var i = 0; i < 200; i++)
        {
            _rains.Add(RandomRainPosition());
        }
    }

    private RectF RandomRainPosition()
    {
        return new RectF(Raylib_cs.Raylib.GetRandomValue(0, RenderTarget!.RenderWidth), Raylib_cs.Raylib.GetRandomValue(100, -RenderTarget!.RenderHeight - 100), 2, Raylib_cs.Raylib.GetRandomValue(10, 100));
    }

    protected override void OnPreUpdate(float deltaTime)
    {
        _uiSettings.Update(deltaTime);
        base.OnPreUpdate(deltaTime);
    }

    protected override void OnUpdate(float deltaTime)
    {
        
        _gui.Start();
        _gui.AddItem(new PanelElement(new RectF(0, 0, RenderTarget!.RenderWidth, RenderTarget!.RenderHeight),
            UVHelper.Center).SetPadding(PaddingF.All(32)));
        _gui.AddItem(new TextElement(Color.Magenta, GlobalObjectManager.ObjectManager.Get<IDefaultFont>()!.GetFont(), "GUI DEMO", 64, 1, UVHelper.Center));
        
        _gui.CloseItem();

        for (var i = 0; i < _rains.Count; i++)
        {
            var rain = _rains[i];
            rain.Y += rain.Height * 10 * deltaTime;
            rain.X = (rain.X + rain.Height * deltaTime).Wrap(RenderTarget!.RenderWidth);
            if (rain.Y > RenderTarget!.RenderHeight)
                rain.Y -= Raylib_cs.Raylib.GetRandomValue(RenderTarget!.RenderHeight + 100, RenderTarget!.RenderHeight * 2);
            
            _rains[i] = rain;
        }
        
        if (Scene.GetGameObject<GameSettingsGameObject<GameInput>>() == null && _uiSettings.IsBackPressed)
            Environment.Exit(0);
    }


    protected override void OnDraw()
    {
        base.OnDraw();

        Raylib_cs.Raylib.DrawTexturePro(
            _bgTexture.Texture, 
            new Rectangle(0,0, 640, 360), 
            new Rectangle(0,0, 640, 360), Vector2.Zero, 0, Color.Gray);

        /*foreach (var rain in _rains)
        {
            rain.DrawFilled(new Color((byte)0, (byte)255, (byte)255, (byte)(rain.Height + 20)));
        }*/
    }

    protected override void OnDispose()
    {
    }
}

/*
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
*/
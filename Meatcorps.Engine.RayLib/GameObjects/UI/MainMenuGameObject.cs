using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.UI.GuiComponent;
using Meatcorps.Engine.RayLib.UI.GuiComponent.Components;
using Meatcorps.Engine.RayLib.UI.GuiComponent.Core;
using Meatcorps.Engine.RayLib.UI.GuiComponent.GuiSettings;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.GameObjects.UI;

public class MainMenuGameObject<TInput>: BaseGameObject where TInput : Enum
{
    private readonly IGuiSettings _guiSettings;
    private GuiServiceComponent _gui = null!;
    private GuiMenuComponent _guiMenu = null!;
    private readonly List<Tuple<string, Action>> _actions = new();

    public MainMenuGameObject(IGuiSettings guiSettings)
    {
        _guiSettings = guiSettings;
        
    }
    
    protected override void OnInitialize()
    {
        Camera = CameraLayer.UI;
        _gui = AddComponent(new GuiServiceComponent(Scene.SceneObjectManager));
        _guiMenu = AddComponent(new GuiMenuComponent(_guiSettings));
    }

    public MainMenuGameObject<TInput> AddMenuAction(string name, Action action)
    {
        _actions.Add(new Tuple<string, Action>(name, action));
        return this;
    }

    protected override void OnUpdate(float deltaTime)
    {
        var settingsMenuActive = Scene.GetGameObject<GameSettingsGameObject<TInput>>() != null; 
        _gui.AddItem(new PanelElement(new RectF(0, 0, RenderTarget!.RenderWidth, RenderTarget!.RenderHeight),
            UVHelper.Bottom).SetPadding(PaddingF.All(48)));
        _gui.AddItem(new PanelElement(new RectF(0, 0, RenderTarget!.RenderWidth, 40 * _guiSettings.FontScaleSize),
            UVHelper.Center));
        _gui.AddItem(new RectangleElement(new Color(0,0,0,200)));

        _guiMenu.SetOrientation(MenuDirection.LeftRight);
        _guiMenu.SetSizeMenuItems(new SizeF(250 * _guiSettings.FontScaleSize, 30 * _guiSettings.FontScaleSize));
        _guiMenu.SetActiveColor(_guiSettings.TextColorActive);
        _guiMenu.SetBorderStyle(0, 2);
        _guiMenu.SetUseSmoothCenter(true);
        _guiMenu.SetTextUv(UVHelper.Center);
        _guiMenu.SetFontSize((int)(16 * _guiSettings.FontScaleSize));
        _guiMenu.SetGap(4);
        _guiMenu.IsActive = !settingsMenuActive;
        
        _guiMenu.Start();
        if (!settingsMenuActive)
        {
            foreach (var action in _actions)
            {
                if (_guiMenu.MenuItem(action.Item1))
                    action.Item2.Invoke();
            }
            if (_guiMenu.MenuItem("Settings"))
                Scene.AddGameObject(new GameSettingsGameObject<TInput>(_guiSettings));

            if (_guiMenu.MenuItem("Exit"))
                Environment.Exit(0);
        }

        _guiMenu.Stop();
        _gui.CloseItem();
        _gui.CloseItem();
    }

    protected override void OnDispose()
    {
    }
}
using System.Globalization;
using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Input;
using Meatcorps.Engine.RayLib.UI.GuiComponent;
using Meatcorps.Engine.RayLib.UI.GuiComponent.Components;
using Meatcorps.Engine.RayLib.UI.GuiComponent.Core;
using Meatcorps.Engine.RayLib.UI.GuiComponent.GuiSettings;
using Meatcorps.Engine.RayLib.Utilities;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.GameObjects.UI;

public class GameSettingsGameObject<TInput>: BaseGameObject where TInput : Enum
{
    private readonly IGuiSettings _guiSettings;
    private readonly string[]? _ratios;
    private ScreenResolutionIterator _screenResolutionIterator = null!;
    private IUniversalConfig _config = null!;
    private GuiServiceComponent _gui = null!;
    private GuiMenuComponent _guiMenu = null!;

    private string? _currentGroup;
    
    private int _resolutionIndex;
    private int _monitor;
    private int _modeIndex;
    private readonly string[] _modes = ["Windowed", "Fullscreen", "Borderless"];
    private string[] _groups = null!;
    private GenericMapper<TInput> _input = null!;
    private RaylibKeyboardBinder<TInput> _keyboardBinder = null!;

    private int _currentKeyboardProfile = -1;
    private readonly EdgeDetector _rebindTrigger = new();
    private readonly Queue<TInput> _toRebind = new();

    public GameSettingsGameObject(IGuiSettings guiSettings, string[]? ratios = null)
    {
        _guiSettings = guiSettings;
        _ratios = ratios;
        
        if (Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix)
            _modes = ["Windowed", "Fullscreen"];
    }
    
    protected override void OnInitialize()
    {
        _screenResolutionIterator = new ScreenResolutionIterator(_ratios);
        _screenResolutionIterator.Load();
        _config = GlobalObjectManager.ObjectManager.Get<IUniversalConfig>()!;
        _gui = AddComponent(new GuiServiceComponent(Scene.SceneObjectManager));
        _guiMenu = AddComponent(new GuiMenuComponent(_guiSettings));
        _keyboardBinder = GlobalObjectManager.ObjectManager.Get<RaylibKeyboardBinder<TInput>>()!;
        _input = GlobalObjectManager.ObjectManager.Get<GenericMapper<TInput>>()!;
        Camera = CameraLayer.UI;
        Layer = 11;
        var groupItems = _config.GetGroups().ToList();
        if (!groupItems.Contains("Input"))
            groupItems.Add("Input");
        groupItems.Sort();
        _groups = groupItems.ToArray();
        
        LoadCurrentConfigurationData();
    }
    
    private void LoadCurrentConfigurationData()
    {
        _resolutionIndex = _screenResolutionIterator.GetModeIndex(Scene.GameHost.Width, Scene.GameHost.Height);
        _monitor = _config.GetOrDefault("Graphics", "Monitor", -1);
        
        if (Raylib.IsWindowFullscreen())
            _modeIndex = 1;
        else if (Raylib.GetScreenWidth() == Raylib.GetMonitorWidth(_monitor) && Raylib.GetScreenHeight() == Raylib.GetMonitorHeight(_monitor))
            _modeIndex = 2;
        else
            _modeIndex = 0;
    }

    protected override void OnUpdate(float deltaTime)
    {
        _gui.Start();
        
        _gui.AddItem(new PanelElement(new RectF(0, 0, RenderTarget!.RenderWidth, RenderTarget!.RenderHeight),
            UVHelper.Right).SetPadding(PaddingF.All(32)));
        HandleMenu();
        _gui.CloseItem();
        _gui.AddItem(new PanelElement(new RectF(0, 0, RenderTarget!.RenderWidth, RenderTarget!.RenderHeight),
            UVHelper.LeftTop).SetPadding(PaddingF.All(32)));
        HandleLeftInfo();
        _gui.CloseItem();
        _gui.AddItem(new PanelElement(new RectF(0, 0, RenderTarget!.RenderWidth, RenderTarget!.RenderHeight),
            UVHelper.LeftBottom));
        HandleBottomInfo();
        _gui.CloseItem();
    }

    private void HandleBottomInfo()
    {
        _gui.AddItem(new PanelElement(new RectF(0, 0, RenderTarget!.RenderWidth, 24 * _guiSettings.FontScaleSize)));
        _gui.AddItem(new RectangleElement(new Color(32, 32, 32)).SetOffset(new Vector2(0, 1)));
        _gui.AddItem(new PanelElement(new RectF(0, 0, RenderTarget!.RenderWidth, 24 * _guiSettings.FontScaleSize), UVHelper.Right).SetPadding(PaddingF.Horizontal(32)));
        _gui.AddItem(new StackElement(new RectF(), 0, Direction.Right, UVHelper.Left));
        _gui.AddItem(new TextElement(_guiSettings.TextColorActive, _guiSettings.Font, $"{_guiSettings.UpPressedText.ToUpper()} {_guiSettings.DownPressedText.ToUpper()} ", 8 * _guiSettings.FontScaleSize).SetSizeBasedOnText());
        _gui.AddItem(new TextElement(_guiSettings.TextColorValue, _guiSettings.Font, "Navigate ", 8 * _guiSettings.FontScaleSize).SetSizeBasedOnText());
        _gui.AddItem(new TextElement(_guiSettings.TextColorActive, _guiSettings.Font, $"{_guiSettings.SelectionPressedText.ToUpper()} ", 8 * _guiSettings.FontScaleSize).SetSizeBasedOnText());
        _gui.AddItem(new TextElement(_guiSettings.TextColorValue, _guiSettings.Font, "Confirm ", 8 * _guiSettings.FontScaleSize).SetSizeBasedOnText());
        _gui.AddItem(new TextElement(_guiSettings.TextColorActive, _guiSettings.Font, $"{_guiSettings.BackPressedText.ToUpper()} ", 8 * _guiSettings.FontScaleSize).SetSizeBasedOnText());
        _gui.AddItem(new TextElement(_guiSettings.TextColorValue, _guiSettings.Font, "Back", 8 * _guiSettings.FontScaleSize).SetSizeBasedOnText());
        _gui.CloseItem();
        _gui.CloseItem();
        _gui.CloseItem();
    }

    private void HandleLeftInfo()
    {
        _gui.AddItem(new StackElement(new RectF()));
        _gui.AddItem(new PanelElement(new RectF(0,0, 300, (_currentGroup == null ? 16 : 10) * _guiSettings.FontScaleSize), UVHelper.LeftTop, false, false));
        _gui.AddItem(new TextElement(_guiSettings.TextColor, _guiSettings.Font, "Settings", (_currentGroup == null ? 16 : 10) * _guiSettings.FontScaleSize, 1, UVHelper.LeftTop));
        _gui.CloseItem();
        if (_currentGroup != null)
        {
            _gui.AddItem(new PanelElement(new RectF(0, 0, 300, 16 * _guiSettings.FontScaleSize), UVHelper.LeftTop, false, false));
            _gui.AddItem(new TextElement(_guiSettings.TextColor, _guiSettings.Font,
                "> " + _currentGroup, 16 * _guiSettings.FontScaleSize, 1, UVHelper.LeftTop));
            _gui.CloseItem();
            if (_currentKeyboardProfile > -1)
            {
                _gui.AddItem(new PanelElement(new RectF(0, 0, 300, 16 * _guiSettings.FontScaleSize), UVHelper.LeftTop, false, false));
                _gui.AddItem(new TextElement(_guiSettings.TextColor, _guiSettings.Font,
                    "> Keyboard " + (_currentKeyboardProfile + 1), 16 * _guiSettings.FontScaleSize, 1, UVHelper.LeftTop));
                _gui.CloseItem();
            }
        }

        _gui.CloseItem();
    }

    private void HandleMenu()
    {
        if (_toRebind.Count > 0)
        {
            var key = _keyboardBinder.IsAnyKeyPressed();
            _rebindTrigger.Update(key != null);
            
            var text = "Press any key\nto rebind\n";
            var target = _toRebind.Peek();
            _gui.AddItem(new StackElement(new RectF()));
            _gui.AddItem(new TextElement(_guiSettings.TextColorValue, _guiSettings.Font, text, 16 * _guiSettings.FontScaleSize).SetSizeBasedOnText());
            _gui.AddItem(new TextElement(_guiSettings.TextColorActive, _guiSettings.Font, target.ToString(), 32 * _guiSettings.FontScaleSize).SetSizeBasedOnText());
            _gui.AddItem(new TextElement(_guiSettings.TextColor, _guiSettings.Font, "\n\nCurrent: " + _input.GetStateByProfile(_currentKeyboardProfile, target).Label, 8 * _guiSettings.FontScaleSize).SetSizeBasedOnText());
            _gui.AddItem(new TextElement(_guiSettings.TextColor, _guiSettings.Font, "Press ESC to stop", 8 * _guiSettings.FontScaleSize).SetSizeBasedOnText());
            _gui.CloseItem();
            
            if (_guiSettings.IsBackPressed)
            {
                _toRebind.TryDequeue(out _);
                return;
            }

            if (_rebindTrigger.IsRisingEdge && key != null)
                _input.SetInput(_currentKeyboardProfile, _toRebind.Dequeue(), key);
            
            return;
        }
        
        _guiMenu.SetOrientation(MenuDirection.UpDown);
        _guiMenu.SetSizeMenuItems(new SizeF(370 * _guiSettings.FontScaleSize, 30 * _guiSettings.FontScaleSize));
        _guiMenu.SetActiveColor(_guiSettings.TextColorActive);
        _guiMenu.SetBorderStyle(0, 2);
        _guiMenu.SetUseSmoothCenter(true);
        _guiMenu.SetTextUv(UVHelper.Left);
        _guiMenu.SetFontSize((int)(16 * _guiSettings.FontScaleSize));
        _guiMenu.SetGap(4);
        _guiMenu.Start();

        if (_currentGroup == null)
            HandleMainMenu();
        else if (_currentGroup == "Input")
            HandleInput();
        else
            HandleCategoryMenu(true);

        _guiMenu.Stop();
    }

    private void HandleInput()
    {
        if (_currentKeyboardProfile < 0)
        {
            for (var i = 0; i < _keyboardBinder.TotalProfiles; i++)
            {
                if (_guiMenu.MenuItem("Bindings Keyboard " + (i + 1)))
                {
                    _currentKeyboardProfile = i;
                    _guiMenu.Reset();
                }
            }

            HandleCategoryMenu(false);
        }
        else
        {
            _guiMenu.MenuLabel("Enter to rebind");
            if (_guiMenu.MenuItem("Rebind all"))
            {
                foreach (var item in _input.GetInputs(_currentKeyboardProfile))
                {
                    _toRebind.Enqueue(item.Key);
                }
            }
            
            if (_guiMenu.MenuItem("Reset"))
            {
                foreach (var item in _input.GetInputs(_currentKeyboardProfile))
                {
                    _input.Reset(_currentKeyboardProfile, item.Key);
                }
            }

            foreach (var input in _input.GetInputs(_currentKeyboardProfile))
            {
                if (_guiMenu.MenuStringValue(input.Key.ToString() + " key", input.Value.Label))
                {
                    _toRebind.Enqueue(input.Key);
                }
            }
        }


        if (_guiMenu.MenuItem("Back") || _guiSettings.IsBackPressed)
        {
            if (_currentKeyboardProfile == -1)
                _currentGroup = null;
            else
                _currentKeyboardProfile = -1;
            
            _guiMenu.Reset();
        }
    }

    private void HandleMainMenu()
    {
        foreach (var group in _groups)
        {
            if (_guiMenu.MenuItem(group))
            {
                _currentGroup = group;
                _guiMenu.Reset();
            }
        }
        if (_guiMenu.MenuItem("Back") || _guiSettings.IsBackPressed)
        {
            Scene.RemoveGameObject(this);
        }
    }
    
    private void HandleCategoryMenu(bool addBack)
    {
        if (_currentGroup == "Graphics")
            HandleCustomGraphicsSettings();
            
        foreach (var item in _config.GetKeys(_currentGroup!))
        {
            var name = item.key.Replace("_", " ").Replace("SetProcessing ", "");
            if (name.Length > 18) 
                name = name[..15] + "..."; 
            switch (item.type)
            {
                case ConfigValueType.IsString:
                    // Not supported yet :)
                    break;
                case ConfigValueType.IsInt:
                    var dataInt = int.Parse(item.value);
                    _guiMenu.MenuIntSlider(name, ref dataInt, minValue: 0, maxValue: 4096);
                    _config.Set(_currentGroup!, item.key, dataInt);
                    break;
                case ConfigValueType.IsFloat:
                    var dataFloat = float.Parse(item.value, CultureInfo.InvariantCulture);
                    _guiMenu.MenuNormalSlider(name, ref dataFloat);
                    _config.Set(_currentGroup!, item.key, dataFloat);
                    break;
                case ConfigValueType.IsBool:
                    var dataBool = bool.Parse(item.value);
                    _guiMenu.MenuBoolSwitch(name, ref dataBool);
                    _config.Set(_currentGroup!, item.key, dataBool);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        if (addBack && _guiMenu.MenuItem("Back") || _guiSettings.IsBackPressed)
        {
            if (_currentGroup == "Graphics")
                LoadCurrentConfigurationData();
            _currentGroup = null;
            _guiMenu.Reset();
        }
    }

    private void HandleCustomGraphicsSettings()
    {
        var resolutions = _screenResolutionIterator.GetModes().ToArray();
                
        _guiMenu.MenuOptions("Mode", _modes, ref _modeIndex);
        if (_modeIndex != 2)
            _guiMenu.MenuOptions("Resolution", resolutions.Select(x => x.ToString()).ToArray(), ref _resolutionIndex);

        if (_guiMenu.MenuIntSlider("Monitor", ref _monitor, minValue: -1,
                maxValue: Raylib.GetMonitorCount() - 1))
        {
            _screenResolutionIterator.Load(_monitor);
            _resolutionIndex = _screenResolutionIterator.GetModeIndex(Scene.GameHost.Width, Scene.GameHost.Height);
            if (_resolutionIndex == -1) 
                _resolutionIndex = _screenResolutionIterator.GetModes().Count() - 1;
        }

        if (_guiMenu.MenuItem("Apply"))
        {
            _config.Set("Graphics", "Monitor", _monitor);
            _config.Set("Graphics", "WindowWidth", resolutions[_resolutionIndex].Width);
            _config.Set("Graphics", "WindowHeight", resolutions[_resolutionIndex].Height);
            _config.Set("Graphics", "FullScreen", false);
            _config.Set("Graphics", "Borderless", false);
            _config.Set("Graphics", "FullScreen", _modeIndex == 1);
            _config.Set("Graphics", "Borderless", _modeIndex == 2);
        }
    }

    protected override void OnDraw()
    {
        Raylib.DrawRectangle(0, 0, RenderTarget!.RenderWidth, RenderTarget!.RenderHeight, new Color(0,0,0,230));
        base.OnDraw();
    }

    protected override void OnDispose()
    {
    }
}
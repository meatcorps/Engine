using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Tween;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.UI.GuiComponent.Core;
using Meatcorps.Engine.RayLib.UI.GuiComponent.GuiSettings;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.UI.GuiComponent.Components;

public class GuiMenuComponent : IRaylibGameComponent
{
    private readonly FixedTimer _animationTimer = new(50);
    private readonly FixedTimer _focusAnimation = new(1000);
    private readonly IGuiSettings _guiSettings;
    private readonly SmoothValue _menuPositionSmooth = new(0, 0.5f);
    private readonly TimerOn _selectedTimer = new(250);
    private Color _activeColor = Color.Magenta;
    private float _borderRoundness;
    private float _borderThickness = 2;
    private bool _disabledMenuItem;
    private int _fontSize = 12;
    private int _gap = 4;
    private GuiServiceComponent _gui = null!;
    private int _menuItemsCount;
    private int _menuPosition;
    private Color _nonActiveColor = Color.Gray;
    private BaseGameObject _owner = null!;
    private bool _selected;
    private SizeF _size = new(220, 32);
    private PaddingF _textPadding = new(8, 8, 8, 8);
    private Vector2 _TextUv = UVHelper.Center;
    private bool _useBorder = true;
    private bool _useSmoothCenter = true;
    private Color _valueColor = Color.White;

    public GuiMenuComponent(IGuiSettings guiSettings)
    {
        _guiSettings = guiSettings;
    }

    public bool IsActive { get; set; } = true;
    public MenuDirection MenuDirection { get; set; } = MenuDirection.LeftRight;

    public void SetOwner(BaseGameObject owner)
    {
        _owner = owner;
    }

    public void Initialize()
    {
        if (!_owner.TryGetComponent(out _gui!))
            throw new Exception("Menu component requires the GuiServiceComponent to be registered first!");
    }

    public void PreUpdate(float deltaTime)
    {
        //
    }

    public void Update(float deltaTime)
    {
        _selectedTimer.Update(_selected, deltaTime);
        _animationTimer.Update(deltaTime);
        _menuPositionSmooth.Update(deltaTime);
        _focusAnimation.Update(deltaTime);

        _menuPositionSmooth.RealValue = _useSmoothCenter ? _menuPosition : 0f;
    }

    public void LateUpdate(float deltaTime)
    {
        //
    }

    public void Draw()
    {
        //
    }

    public void SetSizeMenuItems(SizeF size)
    {
        _size = size;
    }

    public void Start()
    {
        var width = !_useSmoothCenter && MenuDirection == MenuDirection.LeftRight
            ? (_size.Width + _gap) * Math.Max(1, _menuItemsCount)
            : _size.Width;
        var height = !_useSmoothCenter && MenuDirection == MenuDirection.UpDown
            ? (_size.Height + _gap) * Math.Max(1, _menuItemsCount)
            : _size.Height;
        _gui.AddItem(new PanelElement(new RectF(0, 0, width, height), UVHelper.Center, false, false));

        if (MenuDirection == MenuDirection.LeftRight)
        {
            _gui.AddItem(new ScrollElement(new RectF(),
                new Vector2(-(_menuPositionSmooth.DisplayValue * (_size.Width + _gap + 1)), 0)));
            _gui.AddItem(new StackElement(new RectF(32, 32, _size.Width, _size.Height), _gap, Direction.Right,
                UVHelper.Left));
        }
        else
        {
            _gui.AddItem(new ScrollElement(new RectF(),
                new Vector2(0, -(_menuPositionSmooth.DisplayValue * (_size.Height + _gap + 1)))));
            _gui.AddItem(new StackElement(new RectF(32, 32, _size.Width, _size.Height), _gap, Direction.Bottom,
                UVHelper.Top));
        }

        _menuItemsCount = 0;
    }

    public void SetOrientation(MenuDirection direction)
    {
        MenuDirection = direction;
    }

    public void MenuNextItemIsDisabled()
    {
        _disabledMenuItem = true;
    }

    public void SetFontSize(int size)
    {
        _fontSize = size;
    }

    public void SetActiveColor(Color color)
    {
        _activeColor = color;
    }

    public void SetNonActiveColor(Color color)
    {
        _nonActiveColor = color;
    }

    public void SetValueColor(Color color)
    {
        _valueColor = color;
    }

    public void SetUseBorder(bool useBorder)
    {
        _useBorder = useBorder;
    }

    public void SetTextUv(Vector2 uv)
    {
        _TextUv = uv;
    }

    public void SetGap(int gap)
    {
        _gap = gap;
    }

    public void SetTextPadding(PaddingF padding)
    {
        _textPadding = padding;
    }

    public void SetUseSmoothCenter(bool useSmoothCenter)
    {
        _useSmoothCenter = useSmoothCenter;
    }

    public void SetBorderStyle(float roundness, float thickness)
    {
        (_borderRoundness, _borderThickness) = (roundness, thickness);
    }

    public void GetMenuPosition(out int position)
    {
        position = _menuPosition;
    }

    public bool MenuItem(string name)
    {
        if (_guiSettings.IsOnSelectionPressed && !_selected && _menuItemsCount == _menuPosition && !_disabledMenuItem)
        {
            _selected = true;
            _guiSettings.PlaySelectionSound();
        }

        var isSelected = _menuItemsCount == _menuPosition && _selectedTimer.Output;

        var color = DefaultComponentStart();

        _gui.AddItem(new TextElement(color, _guiSettings.Font, name, _fontSize, 1, _TextUv).SetPadding(_textPadding));
        DefaultComponentStop();

        return isSelected;
    }

    public bool MenuStringValue(string name, string value)
    {
        if (_guiSettings.IsOnSelectionPressed && !_selected && _menuItemsCount == _menuPosition && !_disabledMenuItem)
        {
            _selected = true;
            _guiSettings.PlaySelectionSound();
        }

        var isSelected = _menuItemsCount == _menuPosition && _selectedTimer.Output;

        var color = DefaultComponentStart();

        _gui.AddItem(new TextElement(color, _guiSettings.Font, name, _fontSize, 1, UVHelper.Left).SetPadding(_textPadding));
        _gui.AddItem(new TextElement(_valueColor, _guiSettings.Font, value, _fontSize, 1, UVHelper.Right).SetPadding(_textPadding));
        DefaultComponentStop();

        return isSelected;
    }

    public void MenuLabel(string name)
    {
        if (_menuItemsCount == _menuPosition)
            _menuPosition++;

        _gui.AddItem(new PanelElement(new RectF(0, 0, _size.Width, _size.Height)));
        _gui.AddItem(
            new TextElement(_valueColor, _guiSettings.Font, name, _fontSize, 1, _TextUv).SetPadding(_textPadding));
        DefaultComponentStop();
    }

    public bool MenuBoolSwitch(string name, ref bool value)
    {
        var oldValue = value;
        if (_menuItemsCount == _menuPosition && !_disabledMenuItem)
        {
            if (_guiSettings.IsOnSelectionPressed)
            {
                value = !value;
                _guiSettings.PlayNavigationSound();
            }

            if (IsValueChangeDownPressed())
            {
                value = false;
                _guiSettings.PlayNavigationSound();
            }

            if (IsValueChangeUpPressed())
            {
                value = true;
                _guiSettings.PlayNavigationSound();
            }
        }

        var color = DefaultComponentStart();

        _gui.AddItem(
            new TextElement(color, _guiSettings.Font, name, _fontSize, 1, UVHelper.Left).SetPadding(_textPadding));
        _gui.AddItem(new TextElement(GetColor(value ? Color.Green : Color.Red), _guiSettings.Font, value ? "ON" : "OFF",
            _fontSize, 1, UVHelper.Right).SetPadding(_textPadding));
        DefaultComponentStop();
        return oldValue != value;
    }

    public bool MenuNormalSlider(string name, ref float value, float step = 0.05f, bool playSoundBasedOnNormal = false)
    {
        var oldValue = value;
        if (_menuItemsCount == _menuPosition && !_disabledMenuItem)
        {
            if (_guiSettings.IsOnSelectionPressed)
            {
                value = value > 0.5f ? 0 : 1;
                _guiSettings.PlayNavigationSound(playSoundBasedOnNormal ? value : 1);
            }

            if (IsValueChangeDownPressed() && value > 0.01f)
            {
                value -= step;
                _guiSettings.PlayNavigationSound(playSoundBasedOnNormal ? value : 1);
            }

            if (IsValueChangeUpPressed() && value < 0.99f)
            {
                value += step;
                _guiSettings.PlayNavigationSound(playSoundBasedOnNormal ? value : 1);
            }
        }

        value = MathHelper.Clamp(value, 0, 1);

        var color = DefaultComponentStart();
        var sliderText = (value * 100).ToString("F0") + "%";

        if (_menuItemsCount == _menuPosition && !_disabledMenuItem)
        {
            if (value > 0.01f)
                sliderText = "<" + sliderText;
            if (value < 0.99f)
                sliderText = sliderText + ">";
        }

        _gui.AddItem(
            new TextElement(color, _guiSettings.Font, name, _fontSize, 1, UVHelper.Left).SetPadding(_textPadding));
        _gui.AddItem(new TextElement(GetColor(Raylib_cs.Raylib.ColorLerp(Color.Red, Color.Green, value)),
            _guiSettings.Font, sliderText, _fontSize, 1, UVHelper.Right).SetPadding(_textPadding));
        DefaultComponentStop();
        
        return !oldValue.EqualsSafe(value);
    }

    public bool MenuIntSlider(string name, ref int value, int step = 1, int minValue = 0, int maxValue = 10)
    {
        var oldValue = value;
        if (_menuItemsCount == _menuPosition && !_disabledMenuItem)
        {
            if (_guiSettings.IsOnSelectionPressed)
            {
                value = value > (maxValue - minValue) / 2 ? minValue : maxValue;
                _guiSettings.PlayNavigationSound();
            }

            if (IsValueChangeDownPressed() && value > minValue)
            {
                value -= step;
                _guiSettings.PlayNavigationSound();
            }

            if (IsValueChangeUpPressed() && value < maxValue)
            {
                value += step;
                _guiSettings.PlayNavigationSound();
            }
        }

        value = Math.Clamp(value, minValue, maxValue);

        var color = DefaultComponentStart();
        var sliderText = value.ToString();

        if (_menuItemsCount == _menuPosition && !_disabledMenuItem)
        {
            if (value > minValue)
                sliderText = "<" + sliderText;
            if (value < maxValue)
                sliderText = sliderText + ">";
        }

        _gui.AddItem(
            new TextElement(color, _guiSettings.Font, name, _fontSize, 1, UVHelper.Left).SetPadding(_textPadding));
        _gui.AddItem(new TextElement(GetColor(_valueColor), _guiSettings.Font, sliderText, _fontSize, 1, UVHelper.Right)
            .SetPadding(_textPadding));
        DefaultComponentStop();
        return oldValue != value;
    }

    public bool MenuOptions(string name, string[] options, ref int value)
    {
        var oldValue = value;
        if (_menuItemsCount == _menuPosition && !_disabledMenuItem)
        {
            if (_guiSettings.IsOnSelectionPressed)
            {
                value = (value + 1) % options.Length;
                _guiSettings.PlayNavigationSound();
            }

            if (IsValueChangeDownPressed() && value > 0)
            {
                value -= 1;
                _guiSettings.PlayNavigationSound();
            }

            if (IsValueChangeUpPressed() && value < options.Length - 1)
            {
                value += 1;
                _guiSettings.PlayNavigationSound();
            }
        }

        value = Math.Clamp(value, 0, options.Length - 1);

        var optionName = options[value];

        if (_menuItemsCount == _menuPosition && !_disabledMenuItem)
        {
            if (value > 0)
                optionName = "<" + optionName;
            if (value < options.Length - 1)
                optionName = optionName + ">";
        }

        var color = DefaultComponentStart();
        _gui.AddItem(
            new TextElement(color, _guiSettings.Font, name, _fontSize, 1, UVHelper.Left).SetPadding(_textPadding));
        _gui.AddItem(new TextElement(GetColor(_valueColor), _guiSettings.Font, optionName, _fontSize, 1, UVHelper.Right)
            .SetPadding(_textPadding));
        DefaultComponentStop();
        return oldValue != value;
    }

    private Color DefaultComponentStart(bool IsSelectable = false)
    {
        var color = GetColor(_nonActiveColor);

        if (_menuItemsCount == _menuPosition)
        {
            if (_selected)
                color = _animationTimer.Output ? _activeColor : Color.Blank;
            else
                color = GetColor(IsActive!
                    ? _activeColor
                    : Raylib_cs.Raylib.ColorLerp(Raylib_cs.Raylib.ColorAlpha(_activeColor, 0.2f), _activeColor,
                        Tween.ApplyEasing(Tween.NormalToUpDown(_focusAnimation.NormalizedElapsed),
                            EaseType.EaseInOut)));

            if (_selectedTimer.Output)
                _selected = false;
        }

        _gui.AddItem(new PanelElement(new RectF(0, 0, _size.Width, _size.Height)));
        if (_useBorder)
            _gui.AddItem(new RectangleLinesElement(color, _borderThickness, 0, _borderRoundness));
        return color;
    }

    private Color GetColor(Color color)
    {
        return _disabledMenuItem ? Raylib_cs.Raylib.ColorAlpha(color, 0.5f) : color;
    }

    private void DefaultComponentStop()
    {
        _menuItemsCount++;

        _disabledMenuItem = false;
        _gui.CloseItem();
    }

    public void Reset()
    {
        _menuPosition = 0;
    }

    public void Stop()
    {
        _gui.CloseItem();
        _gui.CloseItem();
        _gui.CloseItem();

        if (!_selected && IsActive)
        {
            if (IsPreviousPressed() && _menuPosition > 0)
            {
                _menuPosition--;
                _guiSettings.PlayNavigationSound();
            }

            if (IsNextPressed() && _menuPosition < _menuItemsCount - 1)
            {
                _menuPosition++;
                _guiSettings.PlayNavigationSound();
            }
        }
    }

    private bool IsPreviousPressed()
    {
        if (MenuDirection == MenuDirection.LeftRight)
            return _guiSettings.IsLeftPressed;
        return _guiSettings.IsUpPressed;
    }

    private bool IsNextPressed()
    {
        if (MenuDirection == MenuDirection.LeftRight)
            return _guiSettings.IsRightPressed;
        return _guiSettings.IsDownPressed;
    }


    private bool IsValueChangeDownPressed()
    {
        if (MenuDirection == MenuDirection.LeftRight)
            return _guiSettings.IsDownPressed;
        return _guiSettings.IsLeftPressed;
    }

    private bool IsValueChangeUpPressed()
    {
        if (MenuDirection == MenuDirection.LeftRight)
            return _guiSettings.IsUpPressed;
        return _guiSettings.IsRightPressed;
    }
}

public enum MenuDirection
{
    UpDown,
    LeftRight
}
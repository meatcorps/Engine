using System.Numerics;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Engine.RayLib.Utilities;
using Meatcorps.Game.KillTheSkulls.GameEnums;
using Meatcorps.Game.KillTheSkulls.GameObjects.Abstractions;
using Raylib_cs;

namespace Meatcorps.Game.KillTheSkulls.GameObjects;

public class LedBar: ResourceGameObject
{
    private readonly Color _color;
    private Color[] _renderColor;
    private SmoothColor[] _setRenderColor;
    private FixedTimer _blinkTimer = new(250);
    private FixedTimer _waveTimer = new(500);

    public float Charge { get; set; } = 0;
    public LedBarMode Mode { get; set; } = LedBarMode.On;
    
    public LedBar(Vector2 position, Color color)
    {
        _color = color;
        _renderColor =
        [
            color,
            color,
            color,
            color
        ];
        _setRenderColor =
        [
            new SmoothColor(_color, 1500),
            new SmoothColor(_color, 1500),
            new SmoothColor(_color, 1500),
            new SmoothColor(_color, 1500)
        ];
        Position = position;
    }
    
    protected override void OnInitialize()
    {
        base.OnInitialize();
        Layer = 9;
    }

    protected override void OnUpdate(float deltaTime)
    {
        _blinkTimer.Update(deltaTime);
        _waveTimer.Update(deltaTime);
        switch (Mode)
        {
            case LedBarMode.Off:
                for (var i = 0; i < 4; i++)
                {
                    _setRenderColor[i].RealValue = Color.Black;
                }
                break;
            case LedBarMode.On:
                for (var i = 0; i < 4; i++)
                {
                    _setRenderColor[i].RealValue = _color;
                }
                break;
            case LedBarMode.Blinking:
                for (var i = 0; i < 4; i++)
                {
                    _setRenderColor[i].RealValue = _blinkTimer.NormalizedElapsed < 0.5f ? Color.Black : _color;
                }
                break;
            case LedBarMode.Error:
                for (var i = 0; i < 4; i++)
                {
                    _setRenderColor[i].RealValue = _blinkTimer.NormalizedElapsed < 0.5f ? Color.Black : Color.Red;
                }
                break;
            case LedBarMode.Wave:
                var position = (int)(_waveTimer.NormalizedElapsed * 4);
                for (var i = 0; i < 4; i++)
                {
                    _setRenderColor[3 - i].RealValue = position == i ? _color : Color.Black;
                }
                break;
            case LedBarMode.Charge:
                var position2 = (int)(Math.Clamp(Charge, 0f, 1f) * 5);
                for (var i = 0; i < 4; i++)
                {
                    if (position2 == 0)
                        _setRenderColor[i].RealValue = Color.Black;
                    else
                        _setRenderColor[i].RealValue = 4 - (position2 - 1) <= i ? new Color(128, 255, 255) : Color.Black;
                }
                break;
        }

        for (var i = 0; i < 4; i++)
        {
            _setRenderColor[i].Update(deltaTime);
            _renderColor[i] = _setRenderColor[i].DisplayValue;
        }
    }

    protected override void OnDraw()
    {
        for (var i = 0; i < 4; i++)
        {
            Sprites.Draw(GameSprites.LedBarBackground, Position + new Vector2(24, (i * 24)), Color.White);
            Sprites.Draw(GameSprites.LedBar, Position + new Vector2(24, i * 24), _renderColor[i]);
        }
    }

    protected override void OnDispose()
    {
    }
}

public enum LedBarMode
{
    Off,
    On,
    Blinking,
    Error,
    Wave,
    Charge
}
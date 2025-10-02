using System.Numerics;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Game.HorrorJackpot.GameEnums;
using Meatcorps.Game.HorrorJackpot.GameObjects.Abstractions;
using Raylib_cs;

namespace Meatcorps.Game.HorrorJackpot.GameObjects;

public class OverlayGameObject: ResourceGameObject
{
    private OverlaySettings _settings;
    private TimerOn _showHideTimer;
    private Color _overlayColor = new Color(0, 0, 0, 0);
    private OverlayState _state = OverlayState.Appearing;
    private TimerOn _overlayTimer = new(30000);
    public bool Done { get; set; } = false;
    
    public OverlayGameObject(OverlaySettings settings)
    {
        _settings = settings;
        _showHideTimer = new TimerOn(_settings.AppearTimeMs);
        _overlayTimer = new TimerOn(_settings.DurationMs);
    }
    
    protected override void OnInitialize()
    {
        base.OnInitialize();
        Camera = CameraLayer.UI;
        Layer = 4;
    }

    public void Hide()
    {
        if (_state == OverlayState.Disappearing)
            return;
        
        _showHideTimer = new TimerOn(_settings.DisappearTimeMs);
        _state = OverlayState.Disappearing;
    }
    
    protected override void OnUpdate(float deltaTime)
    {
        _showHideTimer.Update(_state is OverlayState.Appearing or OverlayState.Disappearing, deltaTime);
        _overlayTimer.Update(_state is OverlayState.Visible, deltaTime);
        
        switch (_state)
        {
            case OverlayState.Appearing:
                _overlayColor = Raylib.ColorAlpha(Color.White, _showHideTimer.NormalizedElapsed);
                Console.WriteLine(_state.ToString() + ": " + _showHideTimer.NormalizedElapsed + "");
                if (_showHideTimer.Output)
                    _state = OverlayState.Visible;
                break;
            case OverlayState.Visible:
                if (_overlayTimer.Output)
                    Hide();

                break;
            case OverlayState.Disappearing:
                _overlayColor = Raylib.ColorAlpha(Color.White, 1 - _showHideTimer.NormalizedElapsed);
                
                Console.WriteLine(_state.ToString() + ": " + _showHideTimer.NormalizedElapsed + "");
                if (_showHideTimer.Output)
                {
                    Done = true;
                    Scene.RemoveGameObject(this);
                }

                break;
        }
    }

    protected override void OnDraw()
    {
        Sprites.Draw(GameSprites.OverlayBg, Vector2.Zero, _overlayColor);

        if (_state == OverlayState.Visible)
            _settings.OnDrawLayer(1f, _overlayColor);
        else 
            _settings.OnDrawLayer(_state is OverlayState.Appearing ? _showHideTimer.NormalizedElapsed : 1 - _showHideTimer.NormalizedElapsed, _overlayColor);
        base.OnDraw();
    }

    protected override void OnDispose()
    {
    }
    
    private enum OverlayState
    {
        Appearing,
        Visible,
        Disappearing,
    }
}

public class OverlaySettings
{
    public Action<float, Color> OnDrawLayer { get; set; } = (_, __) => { };
    public float AppearTimeMs { get; set; } = 1000;
    public float DisappearTimeMs { get; set; } = 1000;
    public float DurationMs { get; set; } = 30000;
}
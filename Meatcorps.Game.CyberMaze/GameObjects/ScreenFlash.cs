using System.Numerics;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Tween;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Game.CyberMaze.GameEnums;
using Meatcorps.Game.CyberMaze.GameObjects.Abstractions;
using Raylib_cs;

namespace Meatcorps.Game.CyberMaze.GameObjects;

public class ScreenFlash: ResourceGameObject
{
    private readonly TimerOn _flashTimer;
    private readonly Color _targetColor;
    private readonly Color _startColor;
    private Color _renderColor;
    private readonly IRenderTargetStrategy _renderTarget;

    public ScreenFlash(Color color, int duration = 2000)
    {
        _startColor = new Color(0, 0, 0, 0);
        _targetColor = color;
        _flashTimer = new TimerOn(duration);
        _renderTarget = GlobalObjectManager.ObjectManager.Get<IRenderTargetStrategy>()!;
        Layer = 6;
    }

    protected override void OnInitialize()
    {
        base.OnInitialize();
        if (!DemoMode)
            Sounds.Play(GameSounds.Scary);
    }
    
    protected override void OnUpdate(float deltaTime)
    {
        _flashTimer.Update(true, deltaTime);
        
        var normal = Tween.ApplyEasing(Tween.NormalToUpDown(_flashTimer.NormalizedElapsed), EaseType.EaseInOut);
        
        _renderColor = Raylib.ColorLerp(_startColor, _targetColor, normal);
        
        if (_flashTimer.Output)
            Scene.RemoveGameObject(this);
    }

    protected override void OnDraw()
    {
        var size = new Vector2(_renderTarget.RenderWidth, _renderTarget.RenderHeight);
        var rect = new Rectangle(CameraManager.Position - size / 2, size);
        
        Raylib.DrawRectangleRec(rect, _renderColor);
    }

    protected override void OnDispose()
    {
    }
}
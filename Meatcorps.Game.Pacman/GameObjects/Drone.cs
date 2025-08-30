using System.Numerics;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Tween;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Engine.RayLib.Particles;
using Meatcorps.Game.Pacman.GameEnums;
using Meatcorps.Game.Pacman.GameObjects.Abstractions;
using Meatcorps.Game.Pacman.Particles;
using Raylib_cs;

namespace Meatcorps.Game.Pacman.GameObjects;

public class Drone: ResourceGameObject
{
    private Vector2 _startPosition = new(320, -100);
    private Vector2 _endPosition = new(320, 180);
    private GameSprites _pickupSprite = GameSprites.PacmanDown1;
    private FixedTimer _timer;
    private FixedTimer _animationTimer = new(100);
    private Vector2 _currentPosition = new Vector2();
    private bool _showPickup = false;
    private bool _pickedUp = false;
    private ParticleSystemBuilder _smokeParticle;
    private EdgeDetector _edgeDetector = new();
    private Action _onPickupOrDrop = () => { };
    private Action _onDone = () => { };
    private TimerOn _startDelay;
    private bool _quickStart;

    public Drone(Vector2 targetPosition, GameSprites payload, Action onPickupOrDrop, int startDelay = 1000, bool pickedUp = false, Action? onDone = null)
    {
        _onDone = onDone ?? (() => { });
        _endPosition = targetPosition;
        _pickupSprite = payload;
        _onPickupOrDrop = onPickupOrDrop;
        _pickedUp = pickedUp;
        _startDelay = new(startDelay);
    }
    
    protected override void OnInitialize()
    {
        base.OnInitialize();
        Enabled = false;
        _timer = new FixedTimer(Raylib.GetRandomValue(2000, 3000));
        _smokeParticle = SmokeParticle.GenerateParticleSystem(Sprites, 10);
        _startPosition = new(Raylib.GetRandomValue(0, 640), -64);

        _quickStart = GlobalObjectManager.ObjectManager.Get<IUniversalConfig>()!.GetOrDefault("Debug", "QuickStart", false);

        if (_quickStart)
        {
            _timer = new FixedTimer(64);
            _startDelay = new(10);
        }
        else
        {
            _timer = new FixedTimer(Raylib.GetRandomValue(2000, 3000));
        }
        Layer = 5;
    }
    protected override void OnUpdate(float deltaTime)
    {
        _timer.Update(deltaTime);
        _animationTimer.Update(deltaTime);
        _smokeParticle.Update(deltaTime);
        _edgeDetector.Update(_timer.NormalizedElapsed > 0.5f);
        var normal = Tween.ApplyEasing(Tween.NormalToUpDown(_timer.NormalizedElapsed), EaseType.EaseInOutCubic);
        _currentPosition = Tween.Lerp(_startPosition, _endPosition, normal);
        _showPickup = (_timer.NormalizedElapsed >= 0.5f && _pickedUp) || (_timer.NormalizedElapsed <= 0.5f && !_pickedUp);
        _smokeParticle.Emit(1, _currentPosition);

        if (_edgeDetector.IsRisingEdge)
        {
            if (!DemoMode && !_quickStart)
                Sounds.Play(GameSounds.Blip, 0.3f, 0.5f + Raylib.GetRandomValue(0, 100) / 400f);
            _onPickupOrDrop();
        }

        if (_timer.Output)
        {
            Scene.RemoveGameObject(this);
            _onDone();
        }
    }

    protected override void OnAlwaysUpdate(float deltaTime)
    {
        _startDelay.Update(true, deltaTime);
        if (_startDelay.Output)
            Enabled = true;
        base.OnAlwaysUpdate(deltaTime);
    }

    protected override void OnDraw()
    {
        if (_showPickup) 
            Sprites.Draw(_pickupSprite, _currentPosition);
        Sprites.DrawAnimationWithNormal(GameSprites.DroneAnimation, _animationTimer.NormalizedElapsed, _currentPosition - new Vector2(0, 8));
        _smokeParticle.Draw();
    }

    protected override void OnDispose()
    {
    }
}
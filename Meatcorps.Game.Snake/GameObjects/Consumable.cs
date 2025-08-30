using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Tween;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Camera;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Engine.RayLib.GameObjects;
using Meatcorps.Engine.RayLib.Particles;
using Meatcorps.Game.Snake.Data;
using Meatcorps.Game.Snake.GameObjects.Abstractions;
using Meatcorps.Game.Snake.Particles;
using Meatcorps.Game.Snake.Resources;
using Raylib_cs;

namespace Meatcorps.Game.Snake.GameObjects;

public class Consumable: SnakeGameObject
{
    public IConsumableItem Item { get; }
    private readonly bool _autoRespawn;
    private PointInt _position = new(-1, -1);
    public SnakeSprites Sprite { get; private set; }
    public int ScoreAmount { get => IsRotten ? -_scoreAmount : (int)(_scoreAmount * (1 - RotState)); }
    public int OriginalScore => _scoreAmount;
    
    public float SpawnTimeSeconds { get; private set; }  // for "oldest" targeting
    public bool IsMeat => Item.CanDecay && !IsRotten;

    public bool IsRotten => RotState.EqualsSafe(1);
    public float RottenNormalized => RotState;

    public float RotState { get; private set; } = 0;
    
    private int _scoreAmount = 10;
    private bool _movingToPosition = true;
    private Vector2 _flyFromPosition = Vector2.Zero;
    private Vector2 _flyToPosition = Vector2.Zero;
    private Vector2 _flyPosition = Vector2.Zero;
    private TimerOn _flyTimer = new(500); 
    private FixedTimer _blinkTimer = new(50); 
    private CameraControllerGameObject _cameraController;
    private ParticleSystemBuilder _bloodParticle = new();
    private ParticleSystemBuilder _meatRotterParticle = new();
    private PersistentCanvas _canvas;

    private bool _isBitten = false;
    private FixedTimer _bittenTimer = new(100);
    private FixedTimer _rotationEffectTimer = new(1500);
    private TimerOn? lifetimeTimer;
    private bool _flyAway;
    private bool _blink;

    public Consumable(IConsumableItem item, PointInt? position = null, bool autoRespawn = true)
    {
        Item = item;
        _autoRespawn = autoRespawn;
        if (position != null)
            _position = position.Value;
        Sprite = item.Sprite;
        _scoreAmount = item.Points;
    }
    
    protected override void OnInitialize()
    {
        base.OnInitialize();
        _flyTimer = new TimerOn(Raylib.GetRandomValue(250, 750));

        if (Item.LifetimeInWorld > 0)
            lifetimeTimer = new TimerOn(Item.LifetimeInWorld);
        
        _canvas = Scene.GetGameObject<PersistentCanvas>()!;
        
        _cameraController = Scene.GetGameObject<CameraControllerGameObject>()!;

        SpawnTimeSeconds = (float)Raylib.GetTime();
        
        if (_position == new PointInt(-1, -1))
        {
            var position = GetFreePosition();
            if (position == null)
            {
                Scene.RemoveGameObject(this);
                return;
            }

            _position = position.Value;
        }
        else
        {
            if (LevelData.SnakeGrid.IsOccupied(_position) || LevelData.WallGrid.IsOccupied(_position))
            {
                Scene.RemoveGameObject(this);
                return;
            }
        }

        if (Sprite == SnakeSprites.Background)
        {
            var randomItem = Raylib.GetRandomValue(0, 100);
            if (randomItem < 50)
            {
                Sprite = SnakeSprites.Meat1;
                _scoreAmount = 50;
            }
            else
            {
                Sprite = SnakeSprites.Meat2;
                _scoreAmount = 100;
            }
        }
        
        _flyToPosition = LevelData.ToWorldPosition(_position);
        _flyFromPosition = new Vector2(Raylib.GetRandomValue(0, LevelData.LevelWidth * LevelData.GridSize), -20);
        
        LevelData.ConsumableGrid.Register(_position, this);
        
        _bloodParticle = BloodParticle.GenerateParticleSystem();   
        _meatRotterParticle = MeatRottenParticle.Create(LevelData.ToWorldPosition(_position, true));
        
    }

    public void AddDecay()
    {
        if (IsMeat)
        {
            _isBitten = true;
        }
    }

    private PointInt? GetFreePosition()
    {
        var freeSpots = new List<PointInt>();

        for (var x = 0; x < LevelData.LevelWidth; x++)
        {
            for (var y = 0; y < LevelData.LevelHeight; y++)
            {
                var position = new PointInt(x, y);
                
                if (LevelData.SnakeGrid.IsOccupied(position))
                    continue;
                if (LevelData.ConsumableGrid.IsOccupied(position))
                    continue;
                if (LevelData.WallGrid.IsOccupied(position))
                    continue;
                
                freeSpots.Add(position);
            }
        }
        if (freeSpots.Count == 0)
            return null;
        
        return freeSpots[Raylib.GetRandomValue(0, freeSpots.Count - 1)];
    }
    
    protected override void OnUpdate(float deltaTime)
    {
        _bloodParticle.Update(deltaTime);
        _rotationEffectTimer.Update(deltaTime);

        if (lifetimeTimer is not null)
        {
            if (lifetimeTimer.TimeRemaining < 5000)
            {
                _blinkTimer.Update(deltaTime);
                _blinkTimer.ChangeSpeed((lifetimeTimer.TimeRemaining / 10) + 32);
                if (_blinkTimer.Output)
                {
                    _blink = !_blink;
                    Sounds.Stop(SnakeSounds.ShortWarning);
                    Sounds.Play(SnakeSounds.ShortWarning, 0.5f);
                }
            }
        }

        if (IsRotten)
            _meatRotterParticle.Update(deltaTime);
        
        if (_movingToPosition)
        {
            if (LevelData.SnakeGrid.IsOccupied(_position))
                return;
            
            Layer = 4;
            _flyTimer.Update(true, deltaTime);
            
            _flyPosition = Vector2.Lerp(_flyFromPosition, _flyToPosition, Tween.ApplyEasing(_flyTimer.NormalizedElapsed, EaseType.EaseInCubic));

            if (_flyTimer.Output)
            {
                Sounds.Play(Item.DropSound);
                if (IsMeat)
                    _canvas.AddDrawAction(() =>
                    {
                        Raylib.DrawRectangleV(_flyToPosition, new Vector2(14, 14), Raylib.ColorAlpha(Color.Red, 0.5f));
                        
                        for (var i = 0; i < 4; i++)
                        {
                            var pos = _flyToPosition + new Vector2(Raylib.GetRandomValue(-10, 20), Raylib.GetRandomValue(-10, 20));
                            var size = 5 + Raylib.GetRandomValue(0, 4);
                            Raylib.DrawRectangleV(pos, new Vector2(size, size), Raylib.ColorAlpha(Color.Red, 0.5f));
                        }
                    });
                _movingToPosition = false;
            }

            return;
        }
        
        if (_flyAway)
        {
            Sounds.Stop(SnakeSounds.ShortWarning);
            Layer = 4;
            _flyTimer.Update(true, deltaTime);
            
            _flyPosition = Vector2.Lerp(_flyToPosition, _flyFromPosition, Tween.ApplyEasing(_flyTimer.NormalizedElapsed, EaseType.EaseInCubic));

            if (_flyTimer.Output)
            {
                Scene.RemoveGameObject(this);
                if (_autoRespawn)
                    Scene.AddGameObject(new Consumable(Item, null, _autoRespawn));
            }

            return;
        }
        
        lifetimeTimer?.Update(true, deltaTime);
        
        Layer = 2;
        if (LevelData.SnakeGrid.TryGet(_position, out var snakeBody))
        {
            Sounds.Play(Item.PickupSound, null, Item.PickupPitchSound);
            snakeBody.GameObject.ProcessingConsumable(this);
            Scene.RemoveGameObject(this);
            if (_autoRespawn)
                Scene.AddGameObject(new Consumable(Item, null, _autoRespawn));
            _cameraController.Shake(2, 5);
        }

        if (lifetimeTimer is not null && lifetimeTimer.Output)
        {
            _flyAway = true;
            _flyTimer = new TimerOn(1000);
            LevelData.ConsumableGrid.Remove(_position);
            Sounds.Play(Item.TimeOverSound);
            return;
        }
        
        if (LevelData.WallGrid.IsOccupied(_position))
        {
            Scene.RemoveGameObject(this);
            
            if (_autoRespawn)
                Scene.AddGameObject(new Consumable(Item, null, _autoRespawn));
        }
        
        _bittenTimer.Update(deltaTime);
        if (_isBitten && _bittenTimer.Output)
        {
            Sounds.Play(SnakeSounds.Flybit, 0.2f);
            _isBitten = false;
            RotState = MathF.Min(1, RotState + 0.01f);
            _bloodParticle.Emit(1, LevelData.ToWorldPosition(_position, true));
            if (RotState.EqualsSafe(1))
                Sounds.Play(SnakeSounds.Meatonground, 1f, 0.5f);
            else
                Sounds.Play(SnakeSounds.Flybit, 0.2f);
        }
    }

    protected override void OnDraw()
    {
        if (_movingToPosition || _flyAway)
        {
            var rotation = float.Lerp(_flyFromPosition.ToDistance(_flyToPosition), 0, _flyTimer.NormalizedElapsed);
            Sprites.Draw(Sprite, _flyPosition, Color.White, rotation);
            
            if (_movingToPosition)
                Sprites.Draw(SnakeSprites.Warning, LevelData.ToWorldPosition(_position), Raylib.ColorAlpha(Color.Green, 1 - _flyTimer.NormalizedElapsed));
        }
        else
        {
            var sourceRect = Sprites.GetSprite(Sprite);
            var tweenOffset = 0f;
            var color = Raylib.ColorLerp(Color.White, new Color(0, 25, 0), RotState);
            if (_blink)
                color = Color.Black;
            
            if (!IsMeat && !IsRotten)
            {
                tweenOffset = Tween.ApplyEasing(Tween.NormalToUpDown(_rotationEffectTimer.NormalizedElapsed),
                    EaseType.EaseInOut) * 4;
            }

            var destRect = new Rectangle(LevelData.ToWorldPosition(_position) + new Vector2(0, -tweenOffset + 2), sourceRect.Size);
            
            Raylib.DrawTexturePro(Sprites.Texture, sourceRect, destRect, new Vector2(0, 0), 0, color);
        }
        
        _bloodParticle.Draw();
        
        if (IsRotten)
            _meatRotterParticle.Draw();
        
        base.OnDraw();
    }

    protected override void OnDispose()
    {
        if (_position != new PointInt(-1, -1))
            LevelData.ConsumableGrid.Remove(_position);
    }
}
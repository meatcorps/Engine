using System.Numerics;
using Meatcorps.Engine.Arcade.Interfaces;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.Hardware.ArduinoController.ArduinoController;
using Meatcorps.Engine.RayLib.Camera;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Engine.RayLib.GameObjects;
using Meatcorps.Engine.RayLib.GameObjects.UI;
using Meatcorps.Engine.RayLib.Particles;
using Meatcorps.Engine.RayLib.Text;
using Meatcorps.Engine.RayLib.UI.Data;
using Meatcorps.Game.Snake.Data;
using Meatcorps.Game.Snake.GameObjects.Abstractions;
using Meatcorps.Game.Snake.Particles;
using Meatcorps.Game.Snake.Resources;
using Meatcorps.Game.Snake.Scenes;
using Raylib_cs;

namespace Meatcorps.Game.Snake.GameObjects;

public class Snake : SnakeGameObject
{
    public Player Player { get; }
    private readonly PointInt[] _startPositions;
    private readonly PointInt _startDirection;
    private PointInt _direction = PointInt.Zero;
    private SnakeModel _snakeModel;
    private PlayerInputRouter<SnakeInput> _controller = null!;
    private ParticleSystemBuilder _smokeParticle = new();
    private ParticleSystemBuilder _bloodParticle = new();
    private ParticleSystemBuilder _positiveTextParticle = new();
    private ParticleSystemBuilder _negativeParticle = new();
    private ParticleSystemBuilder _explosionParticle = new();
    private PulseTimer _emitRedSmoke = new(1000);
    private FixedTimer _destroySnakeEmitter = new(250);
    private TimerOn _wallHitTimer = new(3000);
    private bool _smokeEmitToggle;
    private PersistentCanvas _canvas;
    private bool _isHittingWall = false;
    private bool _isDead = false;
    private bool _isDying = false;
    private Color _deadColor;
    private CameraControllerGameObject _cameraController;
    private SnakePerkManager _perkManager;
    private UIMessageEmitter _uiMessage;
    private bool _gainedLife = false;
    public IReadOnlyList<IConsumableItem> Perks => _perkManager.Perks;
    private bool _demoMode = false;
    
    private FixedTimer _demoRandomDirectionTimer = new(3000);
    private IArcadePointsMutator _pointMutator;
    private IPlayerCheckin _playerCheckin;

    public Snake(Player player, PointInt[] startPositions, PointInt startDirection)
    {
        Player = player;
        _startPositions = startPositions;
        _startDirection = startDirection;
        _direction = startDirection;
        Layer = 3;
        _perkManager = new SnakePerkManager(player);

        var info = Raylib.ColorToHSV(player.Color);
        _deadColor = Raylib.ColorFromHSV(info.X, info.Y, 0.3f);
    }

    protected override void OnInitialize()
    {
        base.OnInitialize();
        Player.Initialize();
        _uiMessage = Scene.GetGameObject<UIMessageEmitter>()!;
        _canvas = Scene.GetGameObject<PersistentCanvas>()!;
        _controller = GlobalObjectManager.ObjectManager.Get<PlayerInputRouter<SnakeInput>>()!;
        _pointMutator = GlobalObjectManager.ObjectManager.Get<IArcadePointsMutator>()!;
        _playerCheckin = GlobalObjectManager.ObjectManager.Get<IPlayerCheckin>()!;
        _snakeModel = new SnakeModel(Sprites, LevelData, _startPositions, _startDirection);
        _cameraController = Scene.GetGameObject<CameraControllerGameObject>()!;
        _demoMode = Scene is LevelScene levelScene && levelScene.DemoMode;
        
        if (_demoMode)
            _demoRandomDirectionTimer.ChangeSpeed(Raylib.GetRandomValue(2000, 5000));
        
        SetupParticles();
    }

    public void ProcessingConsumable(Consumable item)
    {
        if (item.IsMeat || item.IsRotten)
            Player.AddValue(SnakePlayerData.MeatEaten);
        else
            Player.AddValue(SnakePlayerData.PickupsTaken);
        
        var score = (int)(Player.Modifiers.RotProof ? item.OriginalScore : item.ScoreAmount);
        score = (int)((float)score * Player.Modifiers.ScoreModifier);
        Player.Score += score; 
        if (score > 0)
            _positiveTextParticle.Emit(1, _snakeModel.HeadRenderPosition.Position, null, "+" + score.ToString());
        else
            _negativeParticle.Emit(1, _snakeModel.HeadRenderPosition.Position, null, score.ToString());

        if (Player.Score < 0)
        {
            IsDying();
            return;
        }

        _perkManager.AddPerk(item.Item);
    }

    private void IsDying()
    {
        if (_isDying || _isDead)
            return;

        if (!_demoMode)
        {
            Music.Stop();

            if (_playerCheckin.IsPlayerCheckedIn(Player.PlayerId, out var _))
            {
                _uiMessage.ClearAll();
                var style = UIMessagePresets.Warning(Fonts.GetFont());
                style.AnchorFrom = Anchor.Top;
                style.AnchorTo = Anchor.Center;
                style.AnchorAfter = Anchor.Bottom;
                _uiMessage.Show("QUICK! PRESS THE BUTTON TO GAIN A LIVE!", style);
                _controller.GetState(Player.PlayerId, SnakeInput.Start).Animation = new BlinkAnimation(100);
            }
        }


        _isDying = true;
        _cameraController.SetZoom(1.5f, 4);
    }
    
    private void SetupParticles()
    {
        _smokeParticle = SnakeTailSmokeParticle.GenerateParticleSystem(_emitRedSmoke, _snakeModel);
        _bloodParticle = BloodParticle.GenerateParticleSystem();
        _positiveTextParticle = ScoreParticle.GenerateParticleSystem(Color.White, Fonts.GetFont());
        _negativeParticle = ScoreParticle.GenerateParticleSystem(Color.Red, Fonts.GetFont());
        _explosionParticle = ExplosionParticle.GenerateParticleSystem(Sprites);
    }

    protected override void OnPreUpdate(float deltaTime)
    {
        var count = 0;
        var size = _snakeModel.Segments.Count;
        foreach (var position in _snakeModel.Segments)
        {
            var type = SnakeBodyType.Body;
            if (count == 0)
                type = SnakeBodyType.Head;
            if (count == size - 1)
                type = SnakeBodyType.Tail;
            LevelData.SnakeGrid.Register(position.Position, new SnakeBody(this, position.Position, type));
            count++;
        }
        Player.PreUpdate();
        _perkManager.Update(deltaTime);
    }

    private bool IsWallHit(PointInt position)
    {
        if (Player.Modifiers.PassThroughWalls)
            return false;
        
        return LevelData.WallGrid.IsOccupied(position);
    }

    protected override void OnUpdate(float deltaTime)
    {
        _smokeParticle.Update(deltaTime);
        _bloodParticle.Update(deltaTime);
        _positiveTextParticle.Update(deltaTime);
        _negativeParticle.Update(deltaTime);
        Player.MoveTimer.Update(deltaTime);
        _wallHitTimer.Update(_isHittingWall, deltaTime);

        var currentDirection = _snakeModel.HeadDirection;

        if (!_playerCheckin.IsPlayerCheckedIn(Player.PlayerId, out var _) && !_isDying && !_isDead)
        {
            _uiMessage.ClearAll();
            _uiMessage.Show( "PLAYER " + Player.PlayerId + " LEAVED THE GAME!", UIMessagePresets.Warning(Fonts.GetFont()));
            _gainedLife = true;
            IsDying();
        }

        if (_isDying && !_gainedLife && _controller.GetState(Player.PlayerId, SnakeInput.Action).IsPressed)
        {
            _gainedLife = true;
            if (_pointMutator.RequestPoints(Player.PlayerId))
            {
                Player.AddValue(SnakePlayerData.Lives);
                Sounds.Play(SnakeSounds.PowerUpScore);
                _uiMessage.ClearAll();
                _uiMessage.Show($"+1 Life! -{_pointMutator.GamePrice} POINTS", UIMessagePresets.Default(Fonts.GetFont()));
            }
            else
            {
                _uiMessage.ClearAll();
                Sounds.Play(SnakeSounds.Alarm);
                _uiMessage.Show("NOT ENOUGH POINTS!", UIMessagePresets.Warning(Fonts.GetFont()));
            }

            _controller.GetState(Player.PlayerId, SnakeInput.Action).Animation = null;
        }
        
        // Input
        if (!_demoMode)
        {
            var axis = _controller.GetAxis(Player.PlayerId).ToPointInt();
            if (axis != -currentDirection && axis != PointInt.Zero && !(Math.Abs(axis.X) > 0 && Math.Abs(axis.Y) > 0))
                _direction = axis;
        }
        else
        {
            _demoRandomDirectionTimer.Update(deltaTime);
            if (_demoRandomDirectionTimer.Output)
            {
                _demoRandomDirectionTimer.ChangeSpeed(Raylib.GetRandomValue(2000, 5000));
                if (_direction.X == 0)
                {
                    if (Raylib.GetRandomValue(0, 1) == 0)
                        _direction = new PointInt(1, 0);
                    else
                        _direction = new PointInt(-1, 0);
                }
                else
                {
                    if (Raylib.GetRandomValue(0, 1) == 0)
                        _direction = new PointInt(0, 1);
                    else
                        _direction = new PointInt(0, -1);
                }
            }
                
        }

        // Move snake
        if (Player.MoveTimer.Output && !_isDying && !_isDead)
        {
            _isHittingWall = false;

            var newHead = _snakeModel.NextPosition(_direction);

            if (IsWallHit(newHead) && _direction != _snakeModel.HeadDirection)
            {
                _direction = _snakeModel.HeadDirection;
                newHead = _snakeModel.NextPosition(_direction);
            }

            var processingConsumable = SnakeIsEating(newHead);

            if (LevelData.SnakeGrid.IsOccupied(newHead) || _wallHitTimer.Output)
            {
                IsDying();
                return;
            }

            if (processingConsumable) 
                _bloodParticle.Emit(25, LevelData.ToWorldPosition(newHead));

            if (IsWallHit(newHead))
            {
                if (_direction == _snakeModel.HeadDirection)
                {
                    _isHittingWall = true;
                    _negativeParticle.Emit(1, LevelData.ToWorldPosition(newHead), null,
                        (_wallHitTimer.TimeRemaining / 1000).ToString("F1") + "s MOVE!");
                    Sounds.Play(SnakeSounds.Wallhit);
                }
            }
            else
            {
                _snakeModel.Move(newHead, _direction, processingConsumable);
            }

            _canvas.AddDrawAction(() =>
            {
                var tailPosition = _snakeModel.TailRenderPosition.Position;
                Raylib.DrawRectangleV(tailPosition, new Vector2(6, 6), Raylib.ColorAlpha(Color.Black, 0.5f));

                for (var i = 0; i < 4; i++)
                {
                    var pos = tailPosition + new Vector2(Raylib.GetRandomValue(-5, 10), Raylib.GetRandomValue(-5, 10));
                    var size = 6 + Raylib.GetRandomValue(0, 4);
                    Raylib.DrawRectangleV(pos, new Vector2(size, size), Raylib.ColorAlpha(Color.Black, 0.5f));
                }
            });
            
            Player.MaxValue(SnakePlayerData.SnakeLength, _snakeModel.BodyCount);
        }
        
        if (_isDying || _isDead)
        {
            _destroySnakeEmitter.Update(deltaTime);
            if (_isDying && _destroySnakeEmitter.Output)
            {
                if (_snakeModel.RemoveRandomSegment(out var position))
                {
                    Sounds.Play(SnakeSounds.Explode1);
                    _explosionParticle.Emit(5, LevelData.ToWorldPosition(position, true));
                    _cameraController.Shake(4, 10);
                }
                else
                {
                    _isDying = false;
                    _isDead = true;
                }
            }

            _explosionParticle.Update(deltaTime);
            if (_explosionParticle.TotalParticlesAlive == 0 && _isDead)
            {
                Sounds.Play(SnakeSounds.Explode7);
                if (Scene is LevelScene levelScene)
                    levelScene.Died(this);
                return;
            }
        }

        _snakeModel.Update(deltaTime,
            !_isHittingWall && !_isDying && !_isDead);
        _emitRedSmoke.Update(_snakeModel.TailIsProcessing, deltaTime);
        
        if (_snakeModel.TailIsProcessing)
            Sounds.Play(SnakeSounds.TailProcessing);
    }

    protected override void OnLateUpdate(float deltaTime)
    {
        Player.IsDead = _isDying || _isDead;
        Player.PostUpdate();

        if (_isDying || _isDead)
            _cameraController.SetPosition(_snakeModel.CenterPosition);
        
        base.OnLateUpdate(deltaTime);
    }

    private bool SnakeIsEating(PointInt position)
    {
        return LevelData.ConsumableGrid.IsOccupied(position);
    }

    protected override void OnDraw()
    {
        _snakeModel.StartRender(_isHittingWall || _isDying || _isDead ? 1 : Player.MoveTimer.NormalizedElapsed);
        
        var texture = Sprites.Texture;
        // Draw snake
        while (_snakeModel.TryRenderSegment(out var source, out var destination, out var segment, out var tail))
        {
            Raylib.DrawTexturePro(texture, source, destination, Vector2.Zero, 0,
                segment.IsDestroyed ? _deadColor : Player.Color);

            if (segment!.IsProcessing && !tail)
                Sprites.Draw(SnakeSprites.SnakeProcessing, destination.Position,
                    Raylib.ColorLerp(Color.Black, Color.Red, Player.MoveTimer.NormalizedElapsed));
        }

        Raylib.DrawTexturePro(texture, _snakeModel.HeadSprite, _snakeModel.HeadRenderPosition, new Vector2(8, 8),
            _snakeModel.HeadRotation, _isDying || _isDead ? _deadColor : Player.Color);
        Raylib.DrawTexturePro(texture, _snakeModel.TailSprite, _snakeModel.TailRenderPosition, new Vector2(8, 8),
            _snakeModel.TailRotation, _isDying || _isDead ? _deadColor : Player.Color);

        _bloodParticle.Draw();
        _positiveTextParticle.Draw();
        _negativeParticle.Draw();

        if (_isDying || _isDead)
            _explosionParticle.Draw();
        else
            _smokeParticle.Draw();

        base.OnDraw();
    }

    protected override void OnDispose()
    {
        _controller.GetState(Player.PlayerId, SnakeInput.Action).Animation = null;
    }
}
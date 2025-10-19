using System.Numerics;
using Meatcorps.Engine.Arcade.Enums;
using Meatcorps.Engine.Arcade.Interfaces;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Tween;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Audio;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Engine.RayLib.Particles;
using Meatcorps.Game.KillTheSkulls.GameEnums;
using Meatcorps.Game.KillTheSkulls.GameObjects.Abstractions;
using Meatcorps.Game.KillTheSkulls.Particles;
using Raylib_cs;

namespace Meatcorps.Game.KillTheSkulls.GameObjects;

public class Enemy : ResourceGameObject
{
    private readonly SmoothValue _position = new(1, 0.5f, false);
    private readonly TimerOn _waitingTimer = new(400);
    private readonly TimerOn _dieTimer = new(1000);
    private readonly TimerOn _attackTimer = new(1000);
    private readonly FixedTimer _dieAnimationTimer = new(100);
    private readonly FixedTimer _soundTimer = new(500);
    public EnemyState State { get; private set; } = EnemyState.Idle;
    public bool Died;
    public bool Attacked;
    public float DieNormal => _dieTimer.NormalizedElapsed;
    public bool InRange => _getNormalValue > 0.3f;
    public ParticleSystemBuilder _smokeParticle;
    private OneSoundManager _dieSound;
    private float _pitchOffset;
    private OneSoundManager _blipSound;
    private GameSounds _attackSound = GameSounds.Attacking;
    private GameSprites _normalSprite = GameSprites.Skull;
    private GameSprites _chargedSprite = GameSprites.SkullCharged;

    private float _getNormalValue => Tween.ApplyEasing(_position.DisplayValue, EaseType.EaseInQuart);

    public Enemy(Vector2 position, int index)
    {
        _pitchOffset = index * 0.01f;
        Position = position;
    }

    protected override void OnInitialize()
    {
        base.OnInitialize();
        _smokeParticle = SmokeParticle.GenerateParticleSystem(Sprites);
        _dieSound = Sounds.GetOneSoundManager(GameSounds.SkullDieLoop, 1, false); 
        _blipSound = Sounds.GetOneSoundManager(GameSounds.Shortblip, 1, false); 
        _dieSound.Pitch = 0.9f + _pitchOffset;
        Layer = 3;

        if (GlobalObjectManager.ObjectManager.Get<IPlayerCheckin>().GetPlayerName(1).Trim() == "FILLINARANDOMNAME:)")
        {
            _attackSound = GameSounds.Attack2;
            _normalSprite = GameSprites.ExSkull;
            _chargedSprite = GameSprites.ExSkullCharged;
        }
    }

    protected override void OnUpdate(float deltaTime)
    {
        _waitingTimer.Update(_position.IsAtRealValue, deltaTime);
        _dieTimer.Update(State == EnemyState.Dying, deltaTime);
        _attackTimer.Update(State == EnemyState.Attacking, deltaTime);
        _dieAnimationTimer.Update(deltaTime);
        _position.Update(deltaTime);
        _smokeParticle.Update(deltaTime);
        _soundTimer.Update(deltaTime);
        switch (State)
        {
            case EnemyState.Idle:
                _position.RealValue = 0;
                _position.SnapToReal();
                _dieSound.Repeat = false;
                _dieSound.Stop();
                break;
            case EnemyState.Up:
                _position.RealValue = 1;
                if (_position.IsAtRealValue)
                    State = EnemyState.Waiting;
                Layer = 3;
                _soundTimer.ChangeSpeed(150);
                if (_soundTimer.Output && !DemoMode)
                {
                    _blipSound.Pitch = (_position.DisplayValue / 5) + _pitchOffset;
                    _blipSound.Volume = _position.DisplayValue;
                    _blipSound.Play();
                }
                break;
            case EnemyState.Waiting:
                _position.RealValue = 1;
                if (_waitingTimer.Output)
                {
                    State = EnemyState.Attacking;
                    if (!DemoMode)
                        Sounds.Play(_attackSound);
                    Attacked = true;
                }

                _soundTimer.ChangeSpeed((1 - _waitingTimer.NormalizedElapsed) * 500);
                if (_soundTimer.Output && !DemoMode)
                {
                    _blipSound.Pitch = 0.7f + _pitchOffset;
                    _blipSound.Volume = 0.3f;
                    _blipSound.Play();
                }
                
                Layer = 3;
                break;
            case EnemyState.Dying:
                if (_dieTimer.Output)
                {
                    _smokeParticle.Emit(10,
                        Position + (1f - _getNormalValue) * new Vector2(0, 128) + new Vector2(32, 64));
                    State = EnemyState.Idle;
                    if (!DemoMode)
                    {
                        Sounds.Play(GameSounds.SkullDieEnd, 0.9f + _pitchOffset);
                        Sounds.Play(GameSounds.Bang);
                    }

                    Died = true;
                }
                else
                {
                    if (!_dieSound.IsPlaying && !DemoMode)
                        _dieSound.Play(true);
                }

                break;
            case EnemyState.Attacking:
                if (_attackTimer.Output)
                {
                    State = EnemyState.Idle;
                }

                Layer = 13;
                break;
        }
    }

    public void Attack()
    {
        if (State is EnemyState.Up or EnemyState.Waiting or EnemyState.Dying)
        {
            Attacked = true;
            State = EnemyState.Attacking;
            _position.SnapToReal();
            if (!DemoMode)
                Sounds.Play(_attackSound);
        }
    }

    public void Start()
    {
        if (State != EnemyState.Idle)
            return;

        State = EnemyState.Up;
    }

    public void Die()
    {
        if (State is not EnemyState.Up and not EnemyState.Waiting)
            return;

        State = EnemyState.Dying;
    }

    protected override void OnDraw()
    {
        if (_getNormalValue > 0.1f)
        {
            if (State is EnemyState.Up or EnemyState.Waiting)
            {
                Sprites.Draw(_normalSprite, Position + ((1f - _getNormalValue) * new Vector2(0, 128)), Color.White);
            }

            if (State == EnemyState.Dying)
                Sprites.Draw(_dieAnimationTimer.NormalizedElapsed < 0.5f ? _chargedSprite : _normalSprite,
                    Position + ((1f - _getNormalValue) * new Vector2(0, 128)), Color.White);

            if (State == EnemyState.Attacking)
            {
                var size = _attackTimer.NormalizedElapsed * 20 + 1;
                Sprites.Draw(_normalSprite,
                    Position + ((1f - _getNormalValue) * new Vector2(0, 128)) + new Vector2(64, 64),
                    Raylib.ColorLerp(Color.Red, new Color(0, 0, 0, 0), _attackTimer.NormalizedElapsed), 0,
                    new Vector2(64, 64) * size, size);
            }
        }

        _smokeParticle.Draw();
    }

    protected override void OnDispose()
    {
        _dieSound.Dispose();
        _blipSound.Dispose();
    }
}

public enum EnemyState
{
    Idle,
    Up,
    Waiting,
    Attacking,
    Dying,
}
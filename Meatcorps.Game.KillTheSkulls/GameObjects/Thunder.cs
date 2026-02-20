using System.Numerics;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Audio;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Game.KillTheSkulls.GameEnums;
using Meatcorps.Game.KillTheSkulls.GameObjects.Abstractions;
using Raylib_cs;

namespace Meatcorps.Game.KillTheSkulls.GameObjects;

public class Thunder : ResourceGameObject
{
    private readonly TimerOn _onTimer = new(180);
    private readonly TimerOn _runningTimer = new(1000);
    private readonly FixedTimer _runningAnimation = new(180);
    private readonly TimerOn _offTimer = new(180);
    private readonly FixedTimer _blinkTimer = new(64);
    private OneSoundManager _beamSoundManager = null!;
    
    public ThunderState State { get; private set; } = ThunderState.Idle;

    public bool IsRunning => State != ThunderState.Idle;
    
    public Thunder(Vector2 position)
    {
        Position = position;
    }
    
    protected override void OnInitialize()
    {
        base.OnInitialize();
        _runningAnimation.ChangeSpeed(Raylib.GetRandomValue(170, 190));
        _beamSoundManager = Sounds.GetOneSoundManager(GameSounds.BeamSoundBoosted, 1, false);
        Layer = 5;
    }

    protected override void OnUpdate(float deltaTime)
    {
       _onTimer.Update(State == ThunderState.On, deltaTime);
       _runningTimer.Update(State == ThunderState.Running, deltaTime);
       _runningAnimation.Update(deltaTime);
       _offTimer.Update(State == ThunderState.Off, deltaTime);
       _blinkTimer.Update(deltaTime);

       if (State == ThunderState.Idle && _beamSoundManager.IsPlaying)
           _beamSoundManager.Stop();
       
       switch (State)
       {
           case ThunderState.Idle:
               break;
           case ThunderState.On:
               if (_onTimer.Output)
                   State = ThunderState.Running;
               
               _beamSoundManager.Pitch = (_onTimer.NormalizedElapsed * 0.9f + 0.1f);
               break;
           case ThunderState.Running:
               if (_runningTimer.Output)
                   State = ThunderState.Off;
               
               _beamSoundManager.Pitch = 0.9f + (Raylib.GetRandomValue(0, 100) / 1000f);
               break;
           case ThunderState.Off:
               if (_offTimer.Output)
                   State = ThunderState.Idle;
               _beamSoundManager.Pitch = 1 - (_offTimer.NormalizedElapsed * 0.9f + 0.1f);
               break;
       }
    }

    public void Start()
    {
        if (State != ThunderState.Idle)
            return;
        if (!DemoMode)
            _beamSoundManager.Play(true);
        State = ThunderState.On;
    }

    protected override void OnDraw()
    {
        if (_blinkTimer.NormalizedElapsed < 0.2f)
            return;
        switch (State)
        {
            case ThunderState.On:
                Sprites.DrawAnimationWithNormal(GameSprites.ThunderChargeAnimationOn, _onTimer.NormalizedElapsed, Position, Color.White);
                break;
            case ThunderState.Running:
                Sprites.DrawAnimationWithNormal(GameSprites.ThunderAnimation, _runningAnimation.NormalizedElapsed, Position, Color.White);
                break;
            case ThunderState.Off:
                Sprites.DrawAnimationWithNormal(GameSprites.ThunderChargeAnimationOff, _offTimer.NormalizedElapsed, Position, Color.White);
                break;
        }
    }

    protected override void OnDispose()
    {
        _beamSoundManager.Dispose();
    }
}

public enum ThunderState
{
    Idle,
    On,
    Running,
    Off
} 
using System.Numerics;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Particles;
using Meatcorps.Game.Snake.Data;
using Raylib_cs;

namespace Meatcorps.Game.Snake.Particles;

public class SnakeTailSmokeParticle
{
    private readonly PulseTimer _emitRedSmoke;
    private readonly SnakeModel _snakeModel;
    private bool _smokeEmitToggle = false;

    public SnakeTailSmokeParticle(PulseTimer emitRedSmoke, SnakeModel snakeModel)
    {
        _emitRedSmoke = emitRedSmoke;
        _snakeModel = snakeModel;
    }

    public ParticleSystemBuilder Create()
    {
        return new ParticleSystemBuilder().SetMaxParticles(20)
            .EnableAutoEmit(1, new FixedTimer(50))
            .AddMutator(() => new ParticleSizeLerp(EaseType.EaseIn)
                .WithStartValue(() =>
                    Raylib.GetRandomValue(3, 8))
                .WithEndValue(0))
            .AddMutator(() => new ParticleColorLerp(EaseType.Linear)
                .WithStartValue(() =>
                {
                    if (_emitRedSmoke.Output)
                        return Raylib.ColorLerp(Color.Red, Color.Black,
                            ((float)Raylib.GetRandomValue(0, 100)) / 100f);

                    return Raylib.ColorLerp(Color.Gray, Color.DarkGray,
                        ((float)Raylib.GetRandomValue(0, 100)) / 100f);
                })
                .WithEndValue(Raylib.ColorAlpha(Color.Black, 0)))
            .SetSpawnLogic(() => new Particle
            {
                LifeTime = 1,
                OnStart = particle =>
                {
                    particle.LifeTime = 1;
                    if (_snakeModel.TailRotation.EqualsSafe(90) || _snakeModel.TailRotation.EqualsSafe(270))
                    {
                        if (_smokeEmitToggle)
                            particle.Position = _snakeModel.TailRenderPosition.Position + new Vector2(-4, 0);
                        else
                            particle.Position = _snakeModel.TailRenderPosition.Position + new Vector2(4, 0);
                    }
                    else
                    {
                        if (_smokeEmitToggle)
                            particle.Position = _snakeModel.TailRenderPosition.Position + new Vector2(0, -4);
                        else
                            particle.Position = _snakeModel.TailRenderPosition.Position + new Vector2(0, 4);
                    }


                    particle.Velocity = new Vector2(Raylib.GetRandomValue(-20, 20), Raylib.GetRandomValue(-20, 20));
                    _smokeEmitToggle = !_smokeEmitToggle;
                },
                OnDraw = particle => { Raylib.DrawCircleV(particle.Position, particle.Size, particle.Color); }
            });
    }
    
    public static ParticleSystemBuilder GenerateParticleSystem(PulseTimer emitRedSmoke, SnakeModel snakeModel)
    {
        return new SnakeTailSmokeParticle(emitRedSmoke, snakeModel).Create();
    }
}
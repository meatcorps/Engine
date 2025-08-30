using System.Numerics;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Particles;
using Meatcorps.Game.Snake.Data;
using Raylib_cs;

namespace Meatcorps.Game.Snake.Particles;

public static class MeatRottenParticle
{
    public static ParticleSystemBuilder Create(Vector2 position)
    {
        return new ParticleSystemBuilder().SetMaxParticles(10)
            .EnableAutoEmit(1, new FixedTimer(250))
            .AddMutator(() => new ParticleSizeLerp(EaseType.EaseIn)
                .WithStartValue(() =>
                    Raylib.GetRandomValue(1, 4))
                .WithEndValue(0))
            .AddMutator(() => new ParticleColorLerp(EaseType.Linear)
                .WithStartValue(() =>
                {
                    return Raylib.ColorLerp(Color.DarkBrown, Color.DarkGreen,
                        ((float)Raylib.GetRandomValue(0, 100)) / 100f);
                })
                .WithEndValue(Raylib.ColorAlpha(Color.Black, 0)))
            .SetSpawnLogic(() => new Particle
            {
                LifeTime = 1,
                OnStart = particle =>
                {
                    particle.Position = position;
                    particle.LifeTime = 1;
                    particle.Velocity = new Vector2(Raylib.GetRandomValue(-20, 20), Raylib.GetRandomValue(-20, 0));
                },
                OnDraw = particle => { Raylib.DrawCircleV(particle.Position, particle.Size, particle.Color); }
            });
    }
    
    public static ParticleSystemBuilder GenerateParticleSystem(PulseTimer emitRedSmoke, SnakeModel snakeModel)
    {
        return new SnakeTailSmokeParticle(emitRedSmoke, snakeModel).Create();
    }
}
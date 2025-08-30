using System.Numerics;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.RayLib.Particles;
using Raylib_cs;

namespace Meatcorps.Game.ArcadeTemplate.Particles;

public static class BloodParticle
{
    public static ParticleSystemBuilder GenerateParticleSystem()
    {
        return new ParticleSystemBuilder().SetMaxParticles(50)
            .AddMutator(() => new ParticleSizeLerp(EaseType.EaseOut)
                .WithStartValue(() => Raylib.GetRandomValue(2, 10))
                .WithEndValue(0))
            .AddMutator(() => new ParticleColorLerp(EaseType.Linear)
                .WithStartValue(() =>
                {
                    var baseRed = new Color(Raylib.GetRandomValue(100, 200), Raylib.GetRandomValue(0, 30), Raylib.GetRandomValue(0, 30), 255);
                    return baseRed;
                })
                .WithEndValue(new Color(80, 0, 0, 0)))
            .SetSpawnLogic(() => new Particle
            {
                LifeTime = 0.5f + Raylib.GetRandomValue(0, 50) / 100f, // Randomize a bit
                OnStart = particle =>
                {
                    //particle.Position = _spatterOrigin; // Set this before emitting
                    particle.Velocity = new Vector2(Raylib.GetRandomValue(-100, 100), Raylib.GetRandomValue(-100, 100)); // Upward/outward spatter
                },
                OnDraw = particle =>
                {
                    Raylib.DrawRectangleV(particle.Position, new Vector2(particle.Size), particle.Color);
                }
            });
    }
}
using System.Numerics;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.Tween;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Particles;
using Raylib_cs;

namespace Meatcorps.Game.ArcadeTemplate.Particles;

public static class ScoreParticle
{
    public static ParticleSystemBuilder GenerateParticleSystem(Color color, Font font)
    {
        return new ParticleSystemBuilder().SetMaxParticles(10)
            .AddMutator(() => new ParticleColorLerp(EaseType.EaseOut)
                .WithStartValue(color).WithEndValue(Raylib.ColorAlpha(color, 0)))
            .SetSpawnLogic(() => new Particle
            {
                Color = color,
                Velocity = new Vector2(0, -20),
                LifeTime = 2f,
                Size = 8,
                OnStart = particle =>
                {
                    particle.PayloadFloat = MathHelper.RandomizedPhase();
                    particle.Origin = particle.Position;
                },
                OnUpdate = particle =>
                {
                    particle.Velocity.X = Tween.FloatBasedOnVelocity(particle.Position.X, particle.DeltaTime, particle.LifeTime, particle.Elapsed, particle.Origin.X, 10, particle.PayloadFloat);  
                },
                OnDraw = particle =>
                {
                    Raylib.DrawTextEx(font, (string)particle.Payload1!, particle.Position, particle.Size, 0, particle.Color);
                } 
            });
    }
}
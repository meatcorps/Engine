using System.Numerics;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Engine.RayLib.Particles;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Game.Snake.Resources;
using Raylib_cs;

namespace Meatcorps.Game.Snake.Particles;

public static class ExplosionParticle
{
    public static ParticleSystemBuilder GenerateParticleSystem(Texture2DItem<SnakeSprites> sprites)
    {
        return new ParticleSystemBuilder().SetMaxParticles(50)
            .AddMutator(() => new ParticleSizeLerp(EaseType.EaseOut)
                .WithStartValue(() => (float)Raylib.GetRandomValue(9, 10) / 10)
                .WithEndValue(1))
            .AddMutator(() => new ParticleColorLerp(EaseType.Linear)
                .WithStartValue(Color.White)
                .WithEndValue(new Color(0, 0, 0, 0)))
            .SetSpawnLogic(() => new Particle
            {
                LifeTime = 0.2f + Raylib.GetRandomValue(0, 50) / 100f, // Randomize a bit
                OnStart = particle =>
                {
                    particle.Position += new Vector2(Raylib.GetRandomValue(-5, 5), Raylib.GetRandomValue(-5, 5)); 
                    particle.Rotation = Raylib.GetRandomValue(0, 360);
                    particle.Velocity = Vector2.Zero;
                },
                OnDraw = particle =>
                {
                    sprites.DrawAnimationWithNormal(SnakeSprites.ExplosionAnimation, particle.NormalizedLifetime, particle.Position, particle.Color, particle.Rotation, new Vector2(16, 16), particle.Size);
                }
            });
    }
}
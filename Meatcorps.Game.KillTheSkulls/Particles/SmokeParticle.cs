using System.Numerics;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Engine.RayLib.Particles;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Game.KillTheSkulls.GameEnums;
using Raylib_cs;

namespace Meatcorps.Game.KillTheSkulls.Particles;

public static class SmokeParticle
{
    public static ParticleSystemBuilder GenerateParticleSystem(Texture2DItem<GameSprites> sprites, int maxParticles = 50)
    {
        return new ParticleSystemBuilder().SetMaxParticles(maxParticles)
            .AddMutator(() => new ParticleSizeLerp(EaseType.EaseOut)
                .WithStartValue(0)
                .WithEndValue(() => Raylib.GetRandomValue(2, 100) / 5f))
            .AddMutator(() => new ParticleColorLerp(EaseType.Linear)
                .WithStartValue(() =>
                {
                    var random = Raylib.GetRandomValue(100, 150);
                    var baseRed = new Color(0, random, random, 150);
                    return baseRed;
                })
                .WithEndValue(new Color(0, 0, 0, 0)))
            .SetSpawnLogic(() => new Particle
            {
                LifeTime = 0.5f + Raylib.GetRandomValue(0, 50) / 100f, // Randomize a bit
                OnStart = particle =>
                {
                    //particle.Position = _spatterOrigin; // Set this before emitting
                    particle.Velocity =
                        new Vector2(Raylib.GetRandomValue(100, 100),
                            Raylib.GetRandomValue(-100, 100)); // Upward/outward spatter
                },
                OnDraw = particle =>
                {
                    sprites.Draw(GameSprites.SmokeSmall, particle.Position - particle.Size * new Vector2(24, 24), particle.Color, particle.Rotation, Vector2.Zero, particle.Size);
                }
            });
    }
}
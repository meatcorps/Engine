using System.Numerics;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Engine.RayLib.Particles;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Game.Pacman.GameEnums;
using Raylib_cs;

namespace Meatcorps.Game.Pacman.Particles;

public static class DutchParticle
{
    public static ParticleSystemBuilder GenerateParticleSystem(Texture2DItem<GameSprites> sprites, int maxParticles = 50)
    {
        return new ParticleSystemBuilder().SetMaxParticles(maxParticles)
            .AddMutator(() => new ParticleSizeLerp(EaseType.EaseOut)
                .WithStartValue(() => Raylib.GetRandomValue(2, 100) / 50f)
                .WithEndValue(0))
            .AddMutator(() => new ParticleColorLerp(EaseType.Linear)
                .WithStartValue(Color.White)
                .WithEndValue(new Color(0, 0, 0, 0)))
            .SetSpawnLogic(() => new Particle
            {
                LifeTime = 0.5f + Raylib.GetRandomValue(0, 50) / 100f, // Randomize a bit
                OnStart = particle =>
                {
                    //particle.Position = _spatterOrigin; // Set this before emitting
                    particle.Velocity =
                        new Vector2(Raylib.GetRandomValue(-100, 100),
                            Raylib.GetRandomValue(-100, 100)); // Upward/outward spatter
                },
                OnDraw = particle =>
                {
                    sprites.Draw(GameSprites.DutchFlag, particle.Position, particle.Color, particle.Rotation, new Vector2(8, 8), particle.Size);
                }
            });
    }
}
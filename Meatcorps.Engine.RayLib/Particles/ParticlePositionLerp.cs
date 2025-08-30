using System.Numerics;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.RayLib.Particles.Abstractions;

namespace Meatcorps.Engine.RayLib.Particles;

public class ParticlePositionLerp : EaseParticleMutator<Vector2>
{
    public ParticlePositionLerp(EaseType easeType) : base(easeType)
    {
    }

    protected override void DoMutate(Particle particle, float easeNormalized)
    {
        particle.Velocity = Vector2.Zero;
        particle.Position = Vector2.Lerp(StartValue, EndValue, easeNormalized);
    }
}
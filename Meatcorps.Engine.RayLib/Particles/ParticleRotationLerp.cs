using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.RayLib.Particles.Abstractions;

namespace Meatcorps.Engine.RayLib.Particles;

public class ParticleRotationLerp: EaseParticleMutator<float>
{
    public ParticleRotationLerp(EaseType easeType) : base(easeType)
    {
    }

    protected override void DoMutate(Particle particle, float easeNormalized)
    {
        particle.Rotation = float.Lerp(StartValue, EndValue, easeNormalized);
    }
}
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.RayLib.Particles.Abstractions;

namespace Meatcorps.Engine.RayLib.Particles;

public class ParticleSizeLerp: EaseParticleMutator<float>
{
    public ParticleSizeLerp(EaseType easeType) : base(easeType)
    {
    }

    protected override void DoMutate(Particle particle, float easeNormalized)
    {
        particle.Size = float.Lerp(StartValue, EndValue, easeNormalized);
    }
}
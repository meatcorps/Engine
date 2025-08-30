using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.RayLib.Particles.Abstractions;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Particles;

public class ParticleColorLerp : EaseParticleMutator<Color>
{
    public ParticleColorLerp(EaseType easeType) : base(easeType)
    {
    }

    protected override void DoMutate(Particle particle, float easeNormalized)
    {
        particle.Color = Raylib.ColorLerp(StartValue, EndValue, easeNormalized);
    }
}
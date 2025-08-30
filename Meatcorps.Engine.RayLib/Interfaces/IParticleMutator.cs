using Meatcorps.Engine.RayLib.Particles;

namespace Meatcorps.Engine.RayLib.Interfaces;

public interface IParticleMutator
{
    void Start(Particle particle);
    void Mutate(Particle particle, float deltaTime);
    void End(Particle particle);
}
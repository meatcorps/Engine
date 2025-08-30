using System.Numerics;
using Meatcorps.Engine.RayLib.Interfaces;

namespace Meatcorps.Engine.RayLib.Particles;

public class ParticleGravity: IParticleMutator
{
    private Vector2 _gravity;
    private readonly Func<Vector2>? _gravityFunc;
    
    public ParticleGravity(Vector2 gravity)
    {
        _gravity = gravity;
    }
    
    public ParticleGravity(Func<Vector2> gravity)
    {
        _gravity = gravity();
        _gravityFunc = gravity;
    }
    
    
    public void Start(Particle particle)
    {
        if (_gravityFunc != null)
            _gravity = _gravityFunc();
    }

    public void Mutate(Particle particle, float deltaTime)
    {
        particle.Velocity += _gravity * deltaTime;
    }

    public void End(Particle particle)
    {
    }
}
using System.Numerics;
using Meatcorps.Engine.RayLib.Interfaces;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Particles;

public class Particle
{
    public Vector2 Origin;
    public Vector2 Position;
    public Vector2 Velocity;
    public float LifeTime;
    public float Elapsed;

    public float Size;
    public float Rotation;
    public Color Color;
    public float PayloadFloat;
    public object? Payload1;
    public object? Payload2;
    
    public bool IsAlive = true;
    public float DeltaTime;

    public Action<Particle> OnStart = particle => { particle.Origin = particle.Position; };
    public Action<Particle> OnUpdate = _ => { };
    public Action<Particle> OnDraw = _ => { };
    public Action<Particle> OnEnd = _ => { };
    public List<IParticleMutator> Mutators { get; } = new();

    public float NormalizedLifetime => LifeTime <= 0 ? 1 : MathF.Min(Elapsed / LifeTime, 1f);
}
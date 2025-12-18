using System.Numerics;
using Meatcorps.Engine.RayLib.Interfaces;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Particles;

public class Particle
{
    public Color Color;
    public float DeltaTime;
    public float Elapsed;

    public bool IsAlive = true;
    public float LifeTime;
    public Action<Particle> OnDraw = _ => { };
    public Action<Particle> OnEnd = _ => { };

    public Action<Particle> OnStart = particle => { particle.Origin = particle.Position; };
    public Action<Particle> OnUpdate = _ => { };
    public Vector2 Origin;
    public object? Payload1;
    public object? Payload2;
    public float PayloadFloat;
    public Vector2 Position;
    public float Rotation;

    public float Size;
    public Vector2 Velocity;
    public List<IParticleMutator> Mutators { get; } = new();

    public float NormalizedLifetime => LifeTime <= 0 ? 1 : MathF.Min(Elapsed / LifeTime, 1f);
}
using System.Buffers;
using System.Numerics;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Interfaces;

namespace Meatcorps.Engine.RayLib.Particles;

public class ParticleSystemBuilder
{
    private readonly List<Particle> _particles = new();
    private Func<Particle>? _spawnLogic;
    private List<Func<IParticleMutator>> _mutators = new();
    private Queue<Particle> _particlePool = new();
    private int _maxParticles = 100;
    private bool _autoEmitEnabled = false;
    private int _autoEmitCount = 1;
    public FixedTimer? AutoEmitTimer { get; private set; } = null;
    
    public int TotalParticlesAlive => _particles.Count;
    
    public ParticleSystemBuilder SetMaxParticles(int max = 10)
    {
        _maxParticles = max;
        return this;
    }

    public ParticleSystemBuilder SetSpawnLogic(Func<Particle> spawn)
    {
        _spawnLogic = spawn;
        for (var i = 0; i < _maxParticles; i++)
        {
            var particle = spawn();
            foreach (var mutator in _mutators)
                particle.Mutators.Add(mutator());
            
            _particlePool.Enqueue(particle);
        }

        return this;
    }
    
    public ParticleSystemBuilder AddMutator(Func<IParticleMutator> mutator)
    {
        _mutators.Add(mutator);
        return this;
    }

    
    public ParticleSystemBuilder EnableAutoEmit(int autoEmitCount = 1, FixedTimer? autoEmitTimer = null)
    {
        _autoEmitEnabled = true;
        _autoEmitCount = autoEmitCount;
        AutoEmitTimer = autoEmitTimer;
        return this;
    }

    public ParticleSystemBuilder DisableAutoEmit()
    {
        _autoEmitEnabled = false;
        return this;
    }

    public ParticleSystemBuilder Emit(int count, Vector2? position = null, Vector2? velocity = null, object? payload = null)
    {
        if (_spawnLogic == null)
            throw new InvalidOperationException("SpawnLogic must be defined.");

        for (var i = 0; i < count; i++)
        {
            if (_particlePool.TryDequeue(out var particle))
            {
                particle.Payload1 = payload;
                particle.Position = position ?? particle.Position;
                particle.Velocity = velocity ?? particle.Velocity;
                particle.OnStart(particle);
                particle.IsAlive = true;
                particle.Elapsed = 0;
                
                foreach (var mutator in particle.Mutators)
                    mutator.Start(particle);
                
                _particles.Add(particle);
            }
            
            if (_particles.Count >= _maxParticles)
                break;
        }

        return this;
    }

    public void Update(float delta)
    {
        if (_autoEmitEnabled)
        {
            if (AutoEmitTimer is not null)
            {
                AutoEmitTimer.Update(delta);
                if (AutoEmitTimer.Output)
                    Emit(_autoEmitCount);
            } else 
                Emit(_autoEmitCount);
        }

        for (var i = _particles.Count - 1; i >= 0; i--)
        {
            var p = _particles[i];
            p.DeltaTime = delta;
            p.Elapsed += delta;
            if (!p.IsAlive || p.Elapsed >= p.LifeTime)
            {
                foreach (var mutator in p.Mutators)
                    mutator.End(p);
                
                p.OnEnd(p);
                _particlePool.Enqueue(p);
                _particles.RemoveAt(i);
                continue;
            }

            p.Position += p.Velocity * delta;
            foreach (var mutator in p.Mutators)
                mutator.Mutate(p, delta);
            
            p.OnUpdate(p);
        }
    }

    public void Draw()
    {
        foreach (var p in _particles)
        {
            p.OnDraw(p);
        }
    }
    
    public void KillAll()
    {
        foreach (var particle in _particles)
        {
            particle.OnEnd(particle);
            _particlePool.Enqueue(particle);
        }
        _particles.Clear();
    }
}
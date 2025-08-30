using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.Tween;
using Meatcorps.Engine.RayLib.Interfaces;

namespace Meatcorps.Engine.RayLib.Particles.Abstractions;

public abstract class EaseParticleMutator<T>: IParticleMutator
{
    private Func<T>? _receiveStartValue;
    private Func<T>? _receiveEndValue;
    private readonly EaseType _easeType;
    protected T StartValue { get; private set; } = default!;
    protected T EndValue { get; private set; } = default!;
    
    public EaseParticleMutator(EaseType easeType)
    {
        _easeType = easeType;
    }

    public EaseParticleMutator<T> WithStartValue(T startValue)
    {
        StartValue = startValue;
        return this;
    }

    public EaseParticleMutator<T> WithStartValue(Func<T> startValue)
    {
        StartValue = startValue();
        _receiveStartValue = startValue;
        return this;
    }

    public EaseParticleMutator<T> WithEndValue(T endValue)
    {
        EndValue = endValue;
        return this;
    }

    public EaseParticleMutator<T> WithEndValue(Func<T> endValue)
    {
        EndValue = endValue();
        _receiveEndValue = endValue;
        return this;
    }
    
    

    public void Start(Particle particle)
    {
        if (_receiveStartValue != null)
            StartValue = _receiveStartValue();
    }

    public void Mutate(Particle particle, float deltaTime)
    {
        if (_easeType == EaseType.Linear)
            DoMutate(particle, particle.NormalizedLifetime);
        
        DoMutate(particle, Tween.ApplyEasing(particle.NormalizedLifetime, _easeType));
    }

    public void End(Particle particle)
    {
        if (_receiveEndValue != null)
            EndValue = _receiveEndValue();
    }

    protected abstract void DoMutate(Particle particle, float easeNormalized);
}
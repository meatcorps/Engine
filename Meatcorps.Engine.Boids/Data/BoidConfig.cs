using Meatcorps.Engine.Boids.Enums;

namespace Meatcorps.Engine.Boids.Data;

public class BoidConfig
{
    public float MaxSpeed { get; set; } = 120f;
    public float MaxForce { get; set; } = 220f;

    public float NeighborRadius { get; set; } = 40f;
    public float DesiredSeparation { get; set; } = 16f;

    public float WeightSeek { get; set; } = 1.3f;
    public float WeightSeparation { get; set; } = 1.6f;
    public float WeightAlignment { get; set; } = 0.8f;
    public float WeightCohesion { get; set; } = 0.7f;
    public float WeightWander { get; set; } = 0.2f;

    public float WanderJitter { get; set; } = 2.0f;
    public float RetargetIntervalSeconds { get; set; } = 0.5f;
    
    // jitter/chaos
    public float TangentSeekNearWeight { get; set; } = 0.8f; // extra sideways pull near target
    public float NearTargetRadius { get; set; } = 48f;       // when to apply tangent seek

// micro-bursts
    public float BurstDurationSeconds { get; set; } = 0.08f; // short dart
    public float BurstStrength { get; set; } = 2.6f;         // scale of accel vs MaxForce
    public (float min, float max) BurstIntervalSeconds { get; set; } = (0.4f, 1.2f);

    public BoidBoundsPolicy BoundsPolicy { get; set; } = BoidBoundsPolicy.Wrap;
}
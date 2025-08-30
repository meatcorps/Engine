namespace Meatcorps.Engine.RayLib.Interfaces;

public interface ITimeService
{
    float DeltaTime { get; }        // Raw wall-clock frame dt (for visuals only)
    float FixedDeltaTime { get; }   // The constant step (e.g., 1/60)
    double TotalTime { get; }       // Since start
    float Alpha { get; }            // 0..1 interpolation for rendering
    int StepsThisFrame { get; }     // Diagnostics
    bool HitCatchUpCapThisFrame { get; } // Diagnostics
}
using Meatcorps.Engine.RayLib.Interfaces;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Game;

public class TimeService : ITimeService
{
    public float DeltaTime { get; private set; }
    public float FixedDeltaTime { get; private set; }
    public double TotalTime { get; private set; }
    public float Alpha { get; private set; }
    public int StepsThisFrame { get; private set; }
    public bool HitCatchUpCapThisFrame { get; private set; }

    public int MaxCatchUpCycles { get; set; } = 4;
    public float TargetFps { get; set; } = 60f;

    float _accumulator;
    double _lastTime = Raylib.GetTime();
    readonly double _startTime = Raylib.GetTime();

    public void UpdateFrameTimes()
    {
        var now = Raylib.GetTime();
        var frameDeltaTime = (float)(now - _lastTime);
        _lastTime = now;

        // Clamp giant hitches (window drag, breakpoints)
        if (frameDeltaTime > 0.25f) frameDeltaTime = 0.25f;

        DeltaTime = frameDeltaTime;                // for interpolation/visuals
        FixedDeltaTime = 1f / TargetFps;    // allow runtime change if needed
        _accumulator += frameDeltaTime;

        StepsThisFrame = 0;
        HitCatchUpCapThisFrame = false;

        TotalTime = now - _startTime;
    }

    // Returns true while there is another fixed step to simulate this frame.
    public bool TryDequeueStep(out float deltaTime)
    {
        if (StepsThisFrame >= MaxCatchUpCycles && _accumulator >= FixedDeltaTime)
        {
            // We *would* keep simulating, but we’re at the cap.
            // Drop the remainder to keep the game responsive.
            _accumulator = 0f;
            HitCatchUpCapThisFrame = true;
        }

        if (_accumulator >= FixedDeltaTime && StepsThisFrame < MaxCatchUpCycles)
        {
            _accumulator -= FixedDeltaTime;
            StepsThisFrame++;
            deltaTime = FixedDeltaTime;
            return true;
        }

        deltaTime = 0f;
        return false;
    }

    public void FinalizeFrame()
    {
        // After all logic steps, compute interpolation for smooth rendering
        var lengthOfOneFixedStep = FixedDeltaTime <= 0f ? 1f : FixedDeltaTime;
        var interpolationRatio = _accumulator / lengthOfOneFixedStep;
        Alpha = interpolationRatio < 0f ? 0f : (interpolationRatio > 1f ? 1f : interpolationRatio);
    }
}
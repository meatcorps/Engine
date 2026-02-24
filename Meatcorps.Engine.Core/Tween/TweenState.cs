using Meatcorps.Engine.Core.Enums;

namespace Meatcorps.Engine.Core.Tween;

/// <summary>
/// Tracks the elapsed time and eased progress of a single tween. Update it each tick and read Progress to get the eased normalized value. Supports ping-pong mode which reverses direction each cycle.
/// </summary>
public struct TweenState
{
    /// <summary>Raw elapsed time in seconds.</summary>
    public float ElapsedTime;

    /// <summary>Total duration of the tween in seconds.</summary>
    public readonly float Duration;

    /// <summary>The easing function applied to compute Progress.</summary>
    public readonly EaseType Ease;

    /// <summary>When true, the tween reverses direction each time it completes instead of stopping.</summary>
    public readonly bool PingPong;

    /// <summary>true after the first Update call.</summary>
    public bool Started { get; private set; }

    /// <summary>true once elapsed time reaches Duration (only set when PingPong is false).</summary>
    public bool Finished { get; private set; }

    /// <summary>true while the tween is on a reverse pass (PingPong mode only).</summary>
    public bool Reversed { get; private set; }

    public TweenState(float duration, EaseType ease = EaseType.Linear, bool pingPong = false)
    {
        ElapsedTime = 0f;
        Duration = duration;
        Ease = ease;
        PingPong = pingPong;

        Started = false;
        Finished = false;
        Reversed = false;
    }

    public void Update(float deltaTime)
    {
        if (!Started)
            Started = true;

        if (Finished)
            return;

        ElapsedTime += deltaTime;

        if (ElapsedTime >= Duration)
        {
            if (PingPong)
            {
                ElapsedTime = 0f;
                Reversed = !Reversed;
            }
            else
            {
                ElapsedTime = Duration;
                Finished = true;
            }
        }
    }

    /// <summary>Elapsed time normalized to [0,1], with no easing applied.</summary>
    public float RawProgress => Tween.Clamp01(ElapsedTime / Duration);

    /// <summary>Elapsed time normalized to [0,1] with the configured easing applied. Inverted during reverse passes in PingPong mode.</summary>
    public float Progress
    {
        get
        {
            var t = Tween.ApplyEasing(RawProgress, Ease);
            return Reversed ? 1f - t : t;
        }
    }

    /// <summary>Resets all state to its initial values.</summary>
    public void Reset()
    {
        ElapsedTime = 0f;
        Started = false;
        Finished = false;
        Reversed = false;
    }
}
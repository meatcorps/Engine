using Meatcorps.Engine.Core.Enums;

namespace Meatcorps.Engine.Core.Tween;

public struct TweenState
{
    public float ElapsedTime;
    public float Duration;
    public EaseType Ease;
    public bool PingPong;

    public bool Started { get; private set; }
    public bool Finished { get; private set; }
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

    public float RawProgress => Tween.Clamp01(ElapsedTime / Duration);

    public float Progress
    {
        get
        {
            var t = Tween.ApplyEasing(RawProgress, Ease);
            return Reversed ? 1f - t : t;
        }
    }

    public void Reset()
    {
        ElapsedTime = 0f;
        Started = false;
        Finished = false;
        Reversed = false;
    }
}
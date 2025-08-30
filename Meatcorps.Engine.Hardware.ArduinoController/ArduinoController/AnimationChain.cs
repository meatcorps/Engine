using Meatcorps.Engine.Core.Interfaces.Input;

namespace Meatcorps.Engine.Hardware.ArduinoController.ArduinoController;

public enum ChainMode
{
    Once,
    Loop,
    LoopCount
}

public readonly struct AnimationStep
{
    public readonly IButtonAnimation Animation;
    public readonly long DurationMs;

    public AnimationStep(IButtonAnimation animation, long durationMs)
    {
        Animation = animation;
        DurationMs = durationMs;
    }
}

public class AnimationChain : IButtonAnimation
{
    private readonly AnimationStep[] _steps;
    private readonly ChainMode _mode;
    private int _remainingLoops;
    private int _index;
    private DateTimeOffset _stepStart;

    public static AnimationChain Once(params AnimationStep[] steps)
        => new AnimationChain(steps, ChainMode.Once);

    public static AnimationChain Loop(params AnimationStep[] steps)
        => new AnimationChain(steps, ChainMode.Loop);

    public static AnimationChain Loop(int count, params AnimationStep[] steps)
        => new AnimationChain(steps, ChainMode.LoopCount, count);

    public AnimationChain(AnimationStep[] steps, ChainMode mode, int loopCount = 0)
    {
        _steps = steps;
        _mode = mode;
        _remainingLoops = loopCount;
        _index = 0;
        _stepStart = DateTimeOffset.Now;
    }

    public bool Update(IInput input)
    {
        if (_steps.Length == 0)
            return false;

        var now = DateTimeOffset.Now;

        var step = _steps[_index];
        var state = step.Animation.Update(input);

        if (now - _stepStart >= TimeSpan.FromMilliseconds(step.DurationMs))
        {
            _index++;

            if (_index >= _steps.Length)
            {
                if (_mode == ChainMode.Once)
                {
                    // let the chain end naturally; caller can set input.Animation = null if desired
                    _index = _steps.Length - 1; // stay on last step but time won’t advance further
                    return state;
                }

                if (_mode == ChainMode.Loop)
                {
                    _index = 0;
                }
                else // LoopCount
                {
                    if (_remainingLoops > 0)
                        _remainingLoops--;

                    if (_remainingLoops <= 0)
                    {
                        _index = _steps.Length - 1;
                        return state;
                    }

                    _index = 0;
                }
            }

            _stepStart = now;
        }

        return state;
    }
}
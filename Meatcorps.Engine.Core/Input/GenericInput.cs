namespace Meatcorps.Engine.Core.Input;

/// <summary>
/// A concrete IInput binding backed by a lambda that returns the current normalized pressed value. Use for keyboard, mouse button, or any custom source that can be expressed as a Func&lt;float&gt;.
/// </summary>
public class GenericInput: BaseInput
{
    private readonly Func<float> _pressedFunc;

    /// <param name="pressedFunc">Lambda that returns the current normalized value in [-1,1].</param>
    /// <param name="label">Display label for this binding.</param>
    public GenericInput(Func<float> pressedFunc, string label): base(label)
    {
        _pressedFunc = pressedFunc;
    }

    protected override float PressedState()
    {
        return _pressedFunc();
    }
}
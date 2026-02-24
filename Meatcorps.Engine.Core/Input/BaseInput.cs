using Meatcorps.Engine.Core.Interfaces.Input;
using Meatcorps.Engine.Core.Utilities;

namespace Meatcorps.Engine.Core.Input;

/// <summary>
/// Abstract base for all input bindings. Drives Down/Up/IsPressed/Normalized state from a subclass-provided PressedState() value. Uses an EdgeDetector internally for frame-accurate IsPressed detection.
/// </summary>
public abstract class BaseInput : IInput
{
    public virtual string Label { get; }
    public bool Enable { get; set; } = true;

    /// <summary>true while the input is held (normalized above 0.5).</summary>
    public bool Down { get; private set; }

    /// <summary>true while the input is not held (normalized below 0.5).</summary>
    public bool Up { get; private set; }

    /// <summary>true only on the single frame the input goes Up→Down (rising edge).</summary>
    public bool IsPressed { get; private set; }

    /// <summary>Raw normalized value in [-1,1]. Digital buttons return 0 or 1.</summary>
    public float Normalized { get; private set; }
    public IButtonAnimation? Animation { get; set; }
    public bool EnableLightWhenPressed { get; set; }
    public bool EnableLightWhenUnpressed { get; set; }

    private readonly EdgeDetector _edgeDetector = new();

    protected BaseInput(string label)
    {
        Label = label;
    }

    /// <summary>Returns the raw pressed value in [-1,1] for this binding. Implement to map the physical input to a normalized float.</summary>
    protected abstract float PressedState();

    /// <summary>Polls PressedState(), updates all state properties, and drives any attached animation. Call once per tick.</summary>
    public void Update()
    {
        if (!Enable)
            return;

        Normalized = Math.Clamp(PressedState(), -1, 1);
        Down = Math.Abs(Normalized) > 0.5f;
        Up = Math.Abs(Normalized) < 0.5f;
        _edgeDetector.Update(Down);
        IsPressed = _edgeDetector.IsRisingEdge;
        Animation?.Update(this);
    }
}
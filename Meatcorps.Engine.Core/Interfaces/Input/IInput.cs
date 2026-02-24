namespace Meatcorps.Engine.Core.Interfaces.Input;

/// <summary>Represents a single input binding with frame-accurate pressed state.</summary>
public interface IInput
{
    /// <summary>Human-readable label for this binding (e.g. "Jump", "Left").</summary>
    string Label { get; }

    /// <summary>When <c>false</c>, all state properties stay at their default and the input is ignored.</summary>
    bool Enable { get; set; }

    /// <summary><c>true</c> while the input is held (normalized value above the 0.5 threshold).</summary>
    bool Down { get; }

    /// <summary><c>true</c> while the input is not held (normalized value below the 0.5 threshold).</summary>
    bool Up { get; }

    /// <summary><c>true</c> only on the single frame the input transitions from Up to Down (rising edge).</summary>
    bool IsPressed { get; }

    /// <summary>Raw normalized value in [-1, 1]. Digital buttons return 0 or 1.</summary>
    float Normalized { get; }

    /// <summary>Optional animation controller for button light feedback. Set to <c>null</c> to disable.</summary>
    IButtonAnimation? Animation { get; set; }

    /// <summary>When <c>true</c>, activates the associated button light while the input is held.</summary>
    bool EnableLightWhenPressed { get; set; }

    /// <summary>When <c>true</c>, activates the associated button light while the input is released.</summary>
    bool EnableLightWhenUnpressed { get; set; }
}
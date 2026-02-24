namespace Meatcorps.Engine.Core.Utilities;

/// <summary>
/// Detects rising and falling edges in a boolean signal. Call Update each tick with the current state. IsRisingEdge is true only on the frame the signal goes false→true. IsFallingEdge is true only on the frame it goes true→false.
/// </summary>
public class EdgeDetector
{
    private bool _previous;

    /// <summary>true only on the single frame the signal transitions from false to true.</summary>
    public bool IsRisingEdge { get; private set; }

    /// <summary>true only on the single frame the signal transitions from true to false.</summary>
    public bool IsFallingEdge { get; private set; }

    /// <summary>Advances the detector with the current signal state. Call once per tick.</summary>
    public void Update(bool current)
    {
        IsRisingEdge = ! _previous && current;
        IsFallingEdge = _previous && !current;
        _previous = current;
    }

    /// <summary>Resets all state to false.</summary>
    public void Reset()
    {
        _previous = false;
        IsRisingEdge = false;
        IsFallingEdge = false;
    }
}
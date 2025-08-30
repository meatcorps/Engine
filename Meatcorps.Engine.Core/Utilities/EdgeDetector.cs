namespace Meatcorps.Engine.Core.Utilities;

public class EdgeDetector
{
    private bool _previous;

    public bool IsRisingEdge { get; private set; }
    public bool IsFallingEdge { get; private set; }

    public void Update(bool current)
    {
        IsRisingEdge = ! _previous && current;
        IsFallingEdge = _previous && !current;
        _previous = current;
    }

    public void Reset()
    {
        _previous = false;
        IsRisingEdge = false;
        IsFallingEdge = false;
    }
}
namespace Meatcorps.Engine.Core.Utilities;

/// <summary>
/// Holds a value that can be reset to its original at any time. Useful for stats or properties that get temporarily modified and need to be restored.
/// </summary>
public class ResetValue<T> : IResetValue
{
    private T _originalValue;

    /// <summary>The current value. Modify this freely; Reset() will restore it to the original.</summary>
    public T Value { get; set; }

    public ResetValue(T originalValue)
    {
        _originalValue = originalValue;
        Value = originalValue;
    }

    /// <summary>Changes the original that Reset() will restore to. When updateValue is true, also updates Value immediately.</summary>
    public void PermanentValue(T value, bool updateValue = true)
    {
        _originalValue = value;
        if (updateValue)
            Value = value;
    }

    /// <summary>Restores Value to the original value supplied at construction or via PermanentValue.</summary>
    public void Reset()
    {
        Value = _originalValue;
    }
}

interface IResetValue
{
    void Reset();
}
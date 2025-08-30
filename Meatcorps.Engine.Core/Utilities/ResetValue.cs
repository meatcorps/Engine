namespace Meatcorps.Engine.Core.Utilities;

public class ResetValue<T> : IResetValue
{
    private T _originalValue;
    public T Value { get; set; }

    public ResetValue(T originalValue)
    {
        _originalValue = originalValue;
        Value = originalValue;
    }    
    
    public void PermanentValue(T value, bool updateValue = true)
    {
        _originalValue = value;
        if (updateValue)
            Value = value;       
    }
    
    public void Reset()
    {
        Value = _originalValue;
    }
}

interface IResetValue
{
    void Reset();
}
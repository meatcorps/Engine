
using Meatcorps.Engine.Core.Extensions;

namespace Meatcorps.Engine.MQTTTest;

[Serializable]
public class TestObject : EqualityComparer<TestObject>
{
    public string Test { get; init; } = string.Empty;
    public float Value { get; init; }
    
    public List<string> List { get; } = new List<string>();

    public override string ToString()
    {
        return $"TestObject: {Test}: {Value}";
    }

    public override bool Equals(TestObject? x, TestObject? y)
    {
        if (x is null || y is null)
            return x is null && y is null;
        return x.Test == y.Test && x.Value.EqualsSafe(y.Value);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not TestObject)
            return false;
        
        return Equals(this, obj as TestObject);
    }

    protected bool Equals(TestObject other)
    {
        return Test == other.Test && Value.Equals(other.Value) && List.Equals(other.List);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Test, Value, List);
    }

    public override int GetHashCode(TestObject obj)
    {
        return HashCode.Combine(Test, Value);
    }
}
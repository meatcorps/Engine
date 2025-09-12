
[Serializable]
public class TestObject : EqualityComparer<TestObject>
{
    public string test { get; set; }
    public float value { get; set; }
    
    public List<string> List { get; } = new List<string>();

    public override string ToString()
    {
        return $"TestObject: {test}: {value}";
    }

    public override bool Equals(TestObject? x, TestObject? y)
    {
        return x?.test == y?.test && x?.value == y?.value;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not TestObject)
            return false;
        
        return Equals(this, obj as TestObject);
    }

    public override int GetHashCode(TestObject obj)
    {
        return HashCode.Combine(test, value);
    }
}
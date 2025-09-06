
[Serializable]
public class TestObject
{
    public string test { get; set; }
    public float value { get; set; }

    public override string ToString()
    {
        return $"TestObject: {test}: {value}";
    }
}
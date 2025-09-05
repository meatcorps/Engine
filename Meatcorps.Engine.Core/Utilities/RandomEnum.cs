namespace Meatcorps.Engine.Core.Utilities;

public class RandomEnum<T> where T : Enum
{
    private List<Group> _values = new();
    private Random _random = Random.Shared;

    public RandomEnum()
    {
        AddGroup();
    }
    
    public RandomEnum<T> Add(T target, int weight)
    {
        if (weight < 0)
            throw new ArgumentOutOfRangeException(nameof(weight));
        
        _values[^1].Total += weight;
        _values[^1].Values.Add((target, weight));
        return this;
    }
    public RandomEnum<T> AddGroup()
    {
        _values.Add(new Group());
        return this;
    }

    public T Get()
    {
        var randomGroup = _random.Next(0, _values.Count - 1);
        var randomTarget = _random.Next(0, _values[randomGroup].Total - 1);
        var target = _values[randomGroup].Values;
        var previous = 0;
        var current = 0;
        foreach (var item in target)
        {
            current += item.Item2;
            if (randomTarget >= previous && randomTarget < current)
                return item.Item1;
            previous = current;
        }
        
        return default!;
    }

    private class Group
    {
        public List<(T, int)> Values { get; } = new();
        public int Total { get; set; } = 0;
    }
}
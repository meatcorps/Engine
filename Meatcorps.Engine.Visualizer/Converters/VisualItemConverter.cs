using System.Reflection;
using Meatcorps.Engine.Visualizer.VisualItems;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Meatcorps.Engine.Visualizer.Converters;

public class VisualItemConverter: JsonConverter<IVisualItem>
{
    private Dictionary<string, Type> _types = new Dictionary<string, Type>();
    
    public VisualItemConverter()
    {
        var result = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(IVisualItem).IsAssignableFrom(t)
                        && t.IsClass
                        && !t.IsAbstract)
            .ToList();

        foreach (var type in result)
            _types.Add(type.Name, type);
    }
    
    public override void WriteJson(JsonWriter writer, IVisualItem? value, JsonSerializer serializer)
    {
        var obj = JObject.FromObject(value);
        obj.AddFirst(new JProperty("type", value.GetType().Name));
        obj.WriteTo(writer);
    }

    public override IVisualItem? ReadJson(JsonReader reader, Type objectType, IVisualItem? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        var obj = JObject.Load(reader);

        var typeName = (string)obj["type"];

        if (!_types.TryGetValue(typeName, out var type))
        {
            Console.WriteLine("Could not find type: " + typeName);
            return null;
        }

        var target = (IVisualItem)Activator.CreateInstance(type)!;

        serializer.Populate(obj.CreateReader(), target);

        return target;
    }
}
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.Visualizer.Converters;
using Meatcorps.Engine.Visualizer.VisualItems;
using Newtonsoft.Json;

namespace Meatcorps.Engine.Visualizer.Services;

public class DataLoaderService
{
    private readonly JsonSerializerSettings _settings;
    private readonly string _path;

    public DataLoaderService()
    {
        _settings = new JsonSerializerSettings();
        _settings.Converters.Add(new VisualItemConverter());
        _settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        _settings.Formatting = Formatting.Indented;

        _path = FileUtilities.GetFullPath("Diagrams");
        
        if (!Directory.Exists(_path))
            Directory.CreateDirectory(_path);
    }

    public IEnumerable<string> GetFiles()
    {
        return Directory.EnumerateFiles(_path, "*.json")
            .Select(x =>
            {
                var fileInfo = new FileInfo(x);
                return fileInfo.Name.Replace(".json", "");
            }); 
    }

    public IEnumerable<IVisualItem>? LoadFile(string file)
    {
        if (!File.Exists(GetFullPath(file)))
            return null;
        
        return JsonConvert.DeserializeObject<List<IVisualItem>>(File.ReadAllText(GetFullPath(file)), _settings);
    }

    public void SaveFile(string file, IEnumerable<IVisualItem> items)
    {
        File.WriteAllText(GetFullPath(file), JsonConvert.SerializeObject(items, _settings));
    } 
    
    private string GetFullPath(string file) => Path.Combine(_path, file) + ".json";
}
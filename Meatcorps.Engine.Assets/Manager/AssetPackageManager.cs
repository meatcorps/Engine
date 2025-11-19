using System.Text;
using Meatcorps.Engine.Assets.Data;
using Meatcorps.Engine.Assets.Interfaces;
using Newtonsoft.Json;

namespace Meatcorps.Engine.Assets.Manager;

public class AssetPackageManager
{
    private readonly string _path;
    private readonly IEncryptDecryptSink _sink;
    private AssetMapData _map;

    public AssetPackageManager(string path, IEncryptDecryptSink sink)
    {
        _path = path;
        _sink = sink;
    }

    public void Load()
    {
        if (!int.TryParse(LoadText(0, 16), out var jsonMapSize)) 
            return;
        
        var jsonMap = LoadText(16, jsonMapSize);
        var map = JsonConvert.DeserializeObject<AssetMapData>(jsonMap);
        if (map == null)
            return;

        foreach (var item in map.Items)
        {
            item.Position += 16;
            item.Position += jsonMapSize;
        }
        
        _map = map;
    }

    public bool Exists(string path)
    {
        path = FixPath(path);
        return _map.Items.Any(x => x.Path == path);
    }

    public bool DirectoryExists(string path)
    {
        path = FixPath(path);
        var exist = _map.Items.Any(x => x.Path.StartsWith(path));
        return exist;
    }

    public IEnumerable<string> GetFiles(string path)
    {
        path = FixPath(path);
        return _map.Items
            .Where(x => x.Path.StartsWith(path))
            .Select(x => x.Path);
    }

    public bool Data(out byte[] data, string path)
    {
        path = FixPath(path);
        var item = _map.Items.FirstOrDefault(x => x.Path == path); 
        if (item is null)
        {
            data = Array.Empty<byte>();
            return false;
        }
        
        data = LoadBytes((int)item.Position, (int)item.Length);
        return true;
    }
    
    private byte[] LoadBytes(int startPosition, int length)
    {
        var data = File.Open(_path, FileMode.Open);
        data.Seek(startPosition, SeekOrigin.Begin);
        var buffer = new byte[length];
        _ = data.Read(buffer, 0, length);
        data.Close();
        return _sink.Decrypt(buffer);
    }

    private string LoadText(int startPosition, int length)
    {
        var byteData = LoadBytes(startPosition, length);
        return Encoding.UTF8.GetString(byteData);
    }

    private string FixPath(string path)
    {
        return path.Replace('/', '\\');
    }
}
using System.Diagnostics;
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
    private const int FIRST_CHUCK_SIZE = 256;
    
    public AssetPackageManager(string path, IEncryptDecryptSink sink)
    {
        _path = path;
        _sink = sink;
    }

    public void Load()
    {
        var data = LoadBytes(0, FIRST_CHUCK_SIZE, 0, rawData =>
        {
            var lengthByteData = rawData.ToList();
            while (lengthByteData.Count > 0 && lengthByteData[0] == 0)
                lengthByteData.RemoveAt(0);
            return lengthByteData.ToArray();
        });
        
        if (!int.TryParse(Encoding.UTF8.GetString(data), out var jsonMapSize)) 
            return;
        
        var jsonMap = LoadText(FIRST_CHUCK_SIZE, jsonMapSize);
        var map = JsonConvert.DeserializeObject<AssetMapData>(jsonMap);
        if (map == null)
            return;

        foreach (var item in map.Items)
        {
            item.Position += FIRST_CHUCK_SIZE;
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
            .Select(x =>
            {
                if (OperatingSystem.IsWindows())
                    return x.Path;
                
                return x.Path.Replace('\\', '/');
            });
    }

    public bool Data(out byte[] data, string path)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        path = FixPath(path);
        var item = _map.Items.FirstOrDefault(x => x.Path == path); 
        if (item is null)
        {
            data = Array.Empty<byte>();
            return false;
        }
        
        data = LoadBytes((int)item.Position, (int)item.Length, (int)item.PlainLength);
        stopwatch.Stop();
        Console.WriteLine("Loaded " + path + " in " + stopwatch.ElapsedMilliseconds + " ms");
        return true;
    }
    
    private byte[] LoadBytes(int startPosition, int length, int plainLength = 0, Func<byte[], byte[]>? middleStuff = null)
    {
        using var data = File.Open(_path, FileMode.Open);
        data.Seek(startPosition, SeekOrigin.Begin);
        var buffer = new byte[length];
        _ = data.Read(buffer, 0, length);
        data.Close();
        
        if (middleStuff != null)
            buffer = middleStuff(buffer);

        if (plainLength > 0)
        {
            var returnBuffer = new byte[plainLength];
            _sink.Decrypt(buffer, returnBuffer);
            return returnBuffer;
        }
        
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

// TODO: Arguments

using System.Text;
using Meatcorps.Engine.Assets.Data;
using Meatcorps.Engine.Assets.Manager;
using Meatcorps.Engine.Assets.Sinks;
using Newtonsoft.Json;

var assetFileName = "Assets.bin";
var encryptDecrypt = new DirectResourceSink();

var loader = new AssetPackageManager(assetFileName, encryptDecrypt);

loader.Load();

loader.Data(out var data2, "Assets\\GameSprites.png");

File.WriteAllBytes("test.png", data2);

return;


const string ASSETS_TEMP = "Assets.temp";


var files = Directory.GetFiles("Assets", "*.*", SearchOption.AllDirectories);

if (File.Exists(ASSETS_TEMP))
    File.Delete(ASSETS_TEMP);
var fileTempWriter = File.Create(ASSETS_TEMP);

var map = new AssetMapData();
var currentPosition = 0;
foreach (var file in files)
{
    var data = File.ReadAllBytes(file);
    data = encryptDecrypt.Encrypt(data);
    fileTempWriter.Write(data);
    map.Items.Add(new AssetMapItemData()
    {
        Path = file,
        Length = data.Length,
        Position = currentPosition
    });
    currentPosition += data.Length;
    Console.WriteLine("Added: " + file);
}

fileTempWriter.Close();

var jsonData = JsonConvert.SerializeObject(map, Formatting.None);
var mapData = encryptDecrypt.Encrypt(Encoding.UTF8.GetBytes(jsonData));

var length = mapData.Length;

var lengthString = encryptDecrypt.Encrypt(
    Encoding.UTF8.GetBytes(
        length.ToString("0000000000000000")));

if (File.Exists(assetFileName))
    File.Delete(assetFileName);


var fileWriter = File.Create(assetFileName);
fileWriter.Write(lengthString);
fileWriter.Write(mapData);
fileWriter.Write(File.ReadAllBytes(ASSETS_TEMP));
fileWriter.Close();

Console.WriteLine("File created: " + assetFileName);
File.Delete(ASSETS_TEMP);

//